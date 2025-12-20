using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommentService.Api.Data;
using CommentService.Api.Enums;
using CommentService.Api.Models.Comment;
using CommentService.Api.Models.Comment.DTOs;
using CommentService.Api.Models.Reaction;
using CommentService.Api.Models.Reaction.DTOs;
using CommentService.Api.Models.VideoCourse;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace CommentService.IntegrationTests.Controllers
{
    public class CommentControllerTests : IClassFixture<IntegrationTestFactory>
    {
        private readonly HttpClient _client;
        private readonly IntegrationTestFactory _factory;

        public CommentControllerTests(IntegrationTestFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");

            _factory.permissionsServiceMock.Reset();
            _factory.videoServiceMock.Reset(); 
        }

        [Fact]
        public async Task Post_ShouldCreateComment_WhenAuthorized()
        {
            // 1. ARRANGE
            var videoId = Guid.NewGuid();
            var courseId = Guid.NewGuid();
            var userId = Guid.NewGuid(); 

            _factory.permissionsServiceMock
                .Setup(x => x.CanCommentAsync(It.IsAny<Guid>(), courseId))
                .ReturnsAsync(true);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.EnsureCreatedAsync();

                if (!await db.VideoCourses.AnyAsync(vc => vc.VideoId == videoId))
                {
                    db.VideoCourses.Add(new VideoCourse { VideoId = videoId, CourseId = courseId });
                    await db.SaveChangesAsync();
                }
            }

            var dto = new CreateCommentDTO
            {
                VideoId = videoId,
                Content = "Integration Test Comment"
            };

            // 2. ACT
            var response = await _client.PostAsJsonAsync("/api/Comment", dto);

            // 3. ASSERT
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {response.StatusCode} - {error}");
            }

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Weryfikacja w bazie
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var comment = await db.Comments.FirstOrDefaultAsync(c => c.Content == "Integration Test Comment");

                comment.Should().NotBeNull();
                comment!.VideoId.Should().Be(videoId);
            }
        }

        [Fact]
        public async Task Get_ShouldReturnComments_WhenAuthorized()
        {
            // 1. ARRANGE
            var videoId = Guid.NewGuid();
            var courseId = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.EnsureCreatedAsync();

                db.VideoCourses.Add(new VideoCourse { VideoId = videoId, CourseId = courseId });

                db.Comments.Add(new Comment
                {
                    Id = Guid.NewGuid(),
                    VideoId = videoId,
                    UserId = Guid.NewGuid(),
                    Content = "Existing Comment",
                    CreatedAt = DateTime.UtcNow,
                    UserName = "Tester"
                });
                await db.SaveChangesAsync();
            }

            _factory.permissionsServiceMock
                .Setup(x => x.HasAccessAsync(It.IsAny<Guid>(), courseId))
                .ReturnsAsync(true);

            // 2. ACT
            var response = await _client.GetAsync($"/api/Comment/{videoId}");

            // 3. ASSERT
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var comments = await response.Content.ReadFromJsonAsync<List<GetCommentDTO>>();

            comments.Should().NotBeNull();
            comments.Should().Contain(c => c.Content == "Existing Comment");
        }

        [Fact]
        public async Task Delete_ShouldRemoveComment_WhenAuthorized()
        {
            // 1. ARRANGE
            var commentId = Guid.NewGuid();
            var videoId = Guid.NewGuid();
            var courseId = Guid.NewGuid();


            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.EnsureCreatedAsync();

                db.VideoCourses.Add(new VideoCourse { VideoId = videoId, CourseId = courseId });

                db.Comments.Add(new Comment
                {
                    Id = commentId,
                    VideoId = videoId,
                    UserId = Guid.NewGuid(), 
                    Content = "To be deleted",
                    UserName = "Spammer"
                });
                await db.SaveChangesAsync();
            }

            _factory.permissionsServiceMock
                .Setup(x => x.CanModerateAsync(It.IsAny<Guid>(), courseId))
                .ReturnsAsync(true);

            // 2. ACT
            var response = await _client.DeleteAsync($"/api/Comment/{commentId}");

            // 3. ASSERT
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var comment = await db.Comments.FindAsync(commentId);
                comment.Should().BeNull(); // Powinien zniknąć
            }
        }
        [Fact]
        public async Task ToggleReaction_ShouldAddLikeToComment_WhenAuthorized()
        {
            // 1. ARRANGE
            var commentId = Guid.NewGuid(); 
            var videoId = Guid.NewGuid();
            var courseId = Guid.NewGuid();
            var commentAuthorId = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.EnsureCreatedAsync();

                if (!await db.VideoCourses.AnyAsync(vc => vc.VideoId == videoId))
                {
                    db.VideoCourses.Add(new VideoCourse { VideoId = videoId, CourseId = courseId });
                }

                db.Comments.Add(new Comment
                {
                    Id = commentId,
                    VideoId = videoId, 
                    UserId = commentAuthorId,
                    UserName = "Comment Author",
                    Content = "Great video!",
                    CreatedAt = DateTime.UtcNow
                });

                await db.SaveChangesAsync();
            }

            _factory.permissionsServiceMock
                .Setup(x => x.HasAccessAsync(It.IsAny<Guid>(), courseId))
                .ReturnsAsync(true);

            var dto = new ToggleReactionDTO
            {
                TargetId = commentId, 
                ToggleType = (char)ToggleReactionEnum.Like
            };

            // 2. ACT
            var response = await _client.PostAsJsonAsync("/api/Comment/ToggleReaction", dto);

            // 3. ASSERT
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {response.StatusCode} - {error}");
            }
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var reaction = await db.Reactions.FirstOrDefaultAsync(r => r.TargetId == commentId);

                reaction.Should().NotBeNull();
                reaction!.IsLike.Should().BeTrue();
            }
        }
    }
}