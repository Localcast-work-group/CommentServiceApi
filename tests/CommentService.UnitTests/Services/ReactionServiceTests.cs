using System.Security.Claims;
using CommentService.Api.Authorization.Requiremments;
using CommentService.Api.Enums;
using CommentService.Api.Interfaces;
using CommentService.Api.Interfaces.Repositories;
using CommentService.Api.Interfaces.Services;
using CommentService.Api.Models.Comment;
using CommentService.Api.Models.Reaction;
using CommentService.Api.Models.VideoCourse;
using CommentService.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;

namespace CommentService.UnitTests.Services
{
    public class ReactionServiceTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IUserContext> _userContextMock;
        private readonly Mock<IAuthorizationService> _authServiceMock;
        private readonly Mock<IVideoCourseService> _videoCourseServiceMock;
        private readonly Mock<IReactionRepository> _reactionRepoMock; 
        private readonly Mock<ICommentRepository> _commentRepoMock; 
        private readonly ReactionService _service;
        private readonly Guid _currentUserId = Guid.NewGuid();

        public ReactionServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _userContextMock = new Mock<IUserContext>();
            _authServiceMock = new Mock<IAuthorizationService>();
            _videoCourseServiceMock = new Mock<IVideoCourseService>();
            _reactionRepoMock = new Mock<IReactionRepository>();
            _commentRepoMock = new Mock<ICommentRepository>();

            _userContextMock.Setup(x => x.UserId).Returns(_currentUserId);
            _userContextMock.Setup(x => x.User).Returns(new ClaimsPrincipal());

            _uowMock.Setup(x => x.Reactions).Returns(_reactionRepoMock.Object);
            _uowMock.Setup(x => x.Comments).Returns(_commentRepoMock.Object);

            _service = new ReactionService(
                _uowMock.Object,
                _userContextMock.Object,
                _authServiceMock.Object,
                _videoCourseServiceMock.Object,
                Mock.Of<Serilog.ILogger>() 
            );
        }

        [Fact]
        public async Task ToggleReaction_ShouldAddReaction_WhenNotExistsAndStatusIsLike()
        {
            // 1. ARRANGE
            var targetId = Guid.NewGuid();
            var courseId = Guid.NewGuid();
            _commentRepoMock.Setup(x => x.GetByIdAsync(targetId))
                .ReturnsAsync(new Comment { Id = targetId, VideoId = Guid.NewGuid() });

            _videoCourseServiceMock.Setup(x => x.GetCourseForVideo(It.IsAny<Guid>()))
                .ReturnsAsync(new VideoCourse {CourseId = courseId, VideoId = targetId, IsAllowAnonymousComments = true });

            _authServiceMock.Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                     It.IsAny<VideoCourse>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());

            _reactionRepoMock.Setup(x => x.GetOneAsync(_currentUserId, targetId))
                .ReturnsAsync((Reaction?)null);

            // 2. ACT
            await _service.ToggleReaction(targetId, ToggleReactionEnum.Like);

            // 3. ASSERT
            _reactionRepoMock.Verify(x => x.AddAsync(It.Is<Reaction>(r =>
                r.UserId == _currentUserId &&
                r.TargetId == targetId &&
                r.IsLike == true
            )), Times.Once);

            _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ToggleReaction_ShouldUpdateReaction_WhenExistsAndStatusIsDislike()
        {
            // 1. ARRANGE
            var targetId = Guid.NewGuid();
            var courseId = Guid.NewGuid();
            _commentRepoMock.Setup(x => x.GetByIdAsync(targetId))
    .ReturnsAsync(new Comment { Id = targetId, VideoId = Guid.NewGuid() });
            var existingReaction = new Reaction
            {
                UserId = _currentUserId,
                TargetId = targetId,
                IsLike = true
            };

            _videoCourseServiceMock.Setup(x => x.GetCourseForVideo(It.IsAny<Guid>()))
                .ReturnsAsync(new VideoCourse { CourseId = courseId, VideoId = targetId, IsAllowAnonymousComments = true });

            _authServiceMock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<VideoCourse>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());

            _reactionRepoMock.Setup(x => x.GetOneAsync(_currentUserId, targetId))
                .ReturnsAsync(existingReaction);

            // 2. ACT
            await _service.ToggleReaction(targetId, ToggleReactionEnum.Dislike);

            // 3. ASSERT
            _reactionRepoMock.Verify(x => x.AddAsync(It.IsAny<Reaction>()), Times.Never);

            existingReaction.IsLike.Should().BeFalse();

            _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ToggleReaction_ShouldDeleteReaction_WhenStatusIsUndo()
        {
            // 1. ARRANGE
            var targetId = Guid.NewGuid();
            var courseId = Guid.NewGuid();

            var existingReaction = new Reaction { UserId = _currentUserId, TargetId = targetId };
            _commentRepoMock.Setup(x => x.GetByIdAsync(targetId))
    .ReturnsAsync(new Comment { Id = targetId, VideoId = Guid.NewGuid() });
            _videoCourseServiceMock.Setup(x => x.GetCourseForVideo(It.IsAny<Guid>()))
                .ReturnsAsync(new VideoCourse { CourseId = courseId, VideoId = targetId, IsAllowAnonymousComments = true });

            _authServiceMock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<VideoCourse>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());

            _reactionRepoMock.Setup(x => x.GetOneAsync(_currentUserId, targetId))
                .ReturnsAsync(existingReaction);

            // 2. ACT
            await _service.ToggleReaction(targetId, ToggleReactionEnum.Undo);

            // 3. ASSERT
            _reactionRepoMock.Verify(x => x.DeleteAsync(existingReaction), Times.Once);
            _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ToggleReaction_ShouldThrowUnauthorized_WhenUserHasNoAccessToCourse()
        {
            // 1. ARRANGE
            var targetId = Guid.NewGuid();
            var courseId = Guid.NewGuid();
            _commentRepoMock.Setup(x => x.GetByIdAsync(targetId))
    .ReturnsAsync(new Comment { Id = targetId, VideoId = Guid.NewGuid() });
            _videoCourseServiceMock.Setup(x => x.GetCourseForVideo(It.IsAny<Guid>()))
                .ReturnsAsync(new VideoCourse { CourseId = courseId, VideoId = targetId, IsAllowAnonymousComments = true });

            _authServiceMock.Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                     It.IsAny<VideoCourse>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            // 2. ACT
            Func<Task> act = async () => await _service.ToggleReaction(targetId, ToggleReactionEnum.Like);

            // 3. ASSERT
            await act.Should().ThrowAsync<UnauthorizedAccessException>();

            _reactionRepoMock.Verify(x => x.AddAsync(It.IsAny<Reaction>()), Times.Never);
            _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteReactionsForCommentAsync_ShouldDeleteAll_WhenAuthorized()
        {
            // 1. ARRANGE
            var commentId = Guid.NewGuid(); 
            var courseId = Guid.NewGuid();

            _commentRepoMock.Setup(x => x.GetByIdAsync(commentId))
                .ReturnsAsync(new Comment { Id = commentId, VideoId = Guid.NewGuid() });
            _videoCourseServiceMock.Setup(x => x.GetCourseForVideo(It.IsAny<Guid>()))
                .ReturnsAsync(new VideoCourse { CourseId = courseId, VideoId = commentId, IsAllowAnonymousComments = true });

            _authServiceMock.Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    courseId,
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // 2. ACT
            await _service.DeleteReactionsForCommentAsync(commentId);

            // 3. ASSERT
            _reactionRepoMock.Verify(x => x.DeleteReactionsForCommentAsync(commentId), Times.Once);
            _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        }
    }
}