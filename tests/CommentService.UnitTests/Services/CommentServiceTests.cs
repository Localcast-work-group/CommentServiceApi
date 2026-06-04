using CommentService.Api.Authorization.Requiremments;
using CommentService.Api.Interfaces;
using CommentService.Api.Interfaces.Repositories;
using CommentService.Api.Interfaces.Services;
using CommentService.Api.Models.Comment;
using CommentService.Api.Models.Comment.DTOs;
using CommentService.Api.Models.Reaction;
using CommentService.Api.Models.VideoCourse;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using MockQueryable.Moq;
using Moq;
using System.Security.Claims;

namespace CommentService.UnitTests.Services
{
    public class CommentServiceTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IReactionService> _reactionServiceMock;
        private readonly Mock<IAuthorizationService> _authServiceMock;
        private readonly Mock<IUserContext> _userContextMock;
        private readonly Mock<IVideoCourseService> _videoCourseServiceMock;
        private readonly Mock<ICommentRepository> _commentRepoMock;

        private readonly Api.Services.CommentService _service;
        private readonly Guid _currentUserId = Guid.NewGuid();

        public CommentServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _reactionServiceMock = new Mock<IReactionService>();
            _authServiceMock = new Mock<IAuthorizationService>();
            _userContextMock = new Mock<IUserContext>();
            _videoCourseServiceMock = new Mock<IVideoCourseService>();
            _commentRepoMock = new Mock<ICommentRepository>();

            _userContextMock.Setup(x => x.UserId).Returns(_currentUserId);
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "TestUser") });
            _userContextMock.Setup(x => x.User).Returns(new ClaimsPrincipal(identity));

            _uowMock.Setup(x => x.Comments).Returns(_commentRepoMock.Object);

            _service = new Api.Services.CommentService(
                _uowMock.Object,
                _reactionServiceMock.Object,
                _authServiceMock.Object,
                _userContextMock.Object,
                Mock.Of<Serilog.ILogger>(), // Ignorujemy logger
                _videoCourseServiceMock.Object
            );
        }

        [Fact]
        public async Task Add_ShouldCreateComment_WhenAuthorized()
        {
            // 1. ARRANGE
            var dto = new CreateCommentDTO
            {
                VideoId = Guid.NewGuid(),
                Content = "Test content"
            };

            _videoCourseServiceMock.Setup(x => x.GetCourseForVideo(dto.VideoId))
                .ReturnsAsync(new VideoCourse { CourseId = Guid.NewGuid() });
            _authServiceMock.Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // 2. ACT
            var result = await _service.Add(dto);

            // 3. ASSERT
            result.Should().NotBeNull();
            result.Content.Should().Be("Test content");
            result.UserName.Should().Be("TestUser");

            // Sprawdzamy zapis do bazy
            _commentRepoMock.Verify(x => x.AddAsync(It.Is<Comment>(c =>
                c.Content == dto.Content &&
                c.VideoId == dto.VideoId &&
                c.UserId == _currentUserId
            )), Times.Once);

            _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Add_ShouldThrowUnauthorized_WhenAuthorizationFails()
        {
            // 1. ARRANGE
            var dto = new CreateCommentDTO { Content = "saasdsadsa" };
            _videoCourseServiceMock.Setup(x => x.GetCourseForVideo(dto.VideoId))
                .ReturnsAsync(new VideoCourse { CourseId = Guid.NewGuid() });
            _authServiceMock.Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            // 2. ACT
            Func<Task> act = async () => await _service.Add(dto);

            // 3. ASSERT
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            _commentRepoMock.Verify(x => x.AddAsync(It.IsAny<Comment>()), Times.Never);
        }

        [Fact]
        public async Task Delete_ShouldDeleteCommentAndRelations_WhenUserIsOwner()
        {
            // 1. ARRANGE
            var commentId = Guid.NewGuid();
            var videoId = Guid.NewGuid();
            var courseId = Guid.NewGuid();

            var comment = new Comment
            {
                Id = commentId,
                VideoId = videoId,
                UserId = _currentUserId 
            };

            _commentRepoMock.Setup(x => x.GetByIdAsync(commentId)).ReturnsAsync(comment);

            _videoCourseServiceMock.Setup(x => x.GetCourseForVideo(videoId))
                .ReturnsAsync(new VideoCourse { CourseId = courseId });

            _authServiceMock.Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(), courseId, It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // 2. ACT
            await _service.Delete(commentId);

            // 3. ASSERT
            _reactionServiceMock.Verify(x => x.DeleteReactionsForCommentAsync(commentId), Times.Once);
            _commentRepoMock.Verify(x => x.DeleteChildren(commentId), Times.Once);
            _commentRepoMock.Verify(x => x.Delete(comment), Times.Once);
            _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task GetAllForVideo_ShouldReturnMappedDtos_WhenAuthorized()
        {
            // 1. ARRANGE
            var videoId = Guid.NewGuid();
            var courseId = Guid.NewGuid();

            _videoCourseServiceMock.Setup(x => x.GetCourseForVideo(videoId))
                .ReturnsAsync(new VideoCourse { CourseId = courseId });

            _authServiceMock.Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(), courseId, It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var comments = new List<Comment>
            {
                new Comment
                {
                    Id = Guid.NewGuid(),
                    Content = "Comment 1",
                    UserId = Guid.NewGuid(),
                    Reactions = new List<Reaction>
                    {
                        new Reaction { IsLike = true, UserId = _currentUserId }, 
                        new Reaction { IsLike = true, UserId = Guid.NewGuid() }  
                    }
                },
                new Comment
                {
                    Id = Guid.NewGuid(),
                    Content = "Comment 2",
                    Reactions = new List<Reaction>
                    {
                        new Reaction { IsLike = false, UserId = _currentUserId }
                    }
                }
            };

            var mockDbSet = comments.AsQueryable().BuildMock();

            _commentRepoMock.Setup(x => x.GetAllForVideo(videoId, true))
                .Returns(mockDbSet); 

            // 2. ACT
            var result = await _service.GetAllForVideo(videoId);

            // 3. ASSERT
            result.Should().HaveCount(2);

            var c1 = result.First(c => c.Content == "Comment 1");
            c1.TotalLikes.Should().Be(2);
            c1.IsLiked.Should().BeTrue();
            c1.IsDisLiked.Should().BeFalse();

            var c2 = result.First(c => c.Content == "Comment 2");
            c2.TotalDisLikes.Should().Be(1);
            c2.IsLiked.Should().BeFalse();
            c2.IsDisLiked.Should().BeTrue();
        }
    }
}