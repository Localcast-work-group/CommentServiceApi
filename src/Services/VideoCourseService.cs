using CommentService.Api.Interfaces;
using CommentService.Api.Interfaces.Services;
using CommentService.Api.Models.VideoCourse;
using Microsoft.AspNetCore.Authorization;
using VideoService.Contracts.ApiResponses;
using VideoService.Contracts.Clients;

namespace CommentService.Api.Services
{
    public class VideoCourseService : IVideoCourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Serilog.ILogger _logger;
        private readonly IVideoServiceClient _videoServiceClient;
        public VideoCourseService(IUnitOfWork unitOfWork, Serilog.ILogger logger, IVideoServiceClient videoServiceClient)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _videoServiceClient = videoServiceClient;
        }

        public async Task CreateCourseVideo(Guid videoId, Guid courseId, bool isAllowAnonymous)
        {
            VideoCourse videoCourse = new VideoCourse
            {
                VideoId = videoId,
                CourseId = courseId,
                IsAllowAnonymousComments = isAllowAnonymous
            };
            await _unitOfWork.VideoCourses.AddAsync(videoCourse);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task UpdateCourseVideo(Guid videoId, Guid courseId, bool isAllowAnonymous)
        {
            VideoCourse? videoCourse = await _unitOfWork.VideoCourses.GetOneAsync(videoId);
            if (videoCourse == null)
            {
                VideoCourse newVideoCourse = new VideoCourse
                {
                    VideoId = videoId,
                    CourseId = courseId,
                    IsAllowAnonymousComments = isAllowAnonymous
                };
                await _unitOfWork.VideoCourses.AddAsync(newVideoCourse);
            }
            else
            {
                videoCourse.CourseId = courseId;
                videoCourse.IsAllowAnonymousComments = isAllowAnonymous;

            }
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task DeleteCourseVideo(Guid id)
        {
            VideoCourse? videoCourse = await _unitOfWork.VideoCourses.GetOneAsync(id);
            await _unitOfWork.VideoCourses.DeleteAsync(videoCourse);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<VideoCourse> GetCourseForVideo(Guid videoId)
        {
            var videoCourse = await _unitOfWork.VideoCourses.GetOneAsync(videoId);
            if (videoCourse == null)
            {
                var videoCourseResponse = await _videoServiceClient.GetVideoCourseAsync(videoId);
                if (videoCourseResponse == null)
                {
                    return null;
                }
                else
                {
                    videoCourse = new VideoCourse
                    {
                        VideoId = videoCourseResponse.VideoId,
                        CourseId = videoCourseResponse.CourseId,
                        IsAllowAnonymousComments = videoCourseResponse.AllowAnonymous
                    };
                    await _unitOfWork.VideoCourses.AddAsync(videoCourse);
                    await _unitOfWork.SaveChangesAsync();
                }

            }
            return videoCourse;
        }
    }
}
