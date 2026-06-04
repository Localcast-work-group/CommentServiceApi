using CommentService.Api.Data;
using CommentService.Api.Interfaces.Repositories;
using CommentService.Api.Models.Reaction;
using CommentService.Api.Models.VideoCourse;
using Microsoft.EntityFrameworkCore;

namespace CommentService.Api.Repositories
{
    public class VideoCourseRepository : IVideoCourseRepository
    {
        private readonly ApplicationDbContext _dBContext;
        public VideoCourseRepository(ApplicationDbContext dBContext)
        {
            _dBContext = dBContext;
        }
        public async Task AddAsync(VideoCourse model)
        {
            await _dBContext.AddAsync(model);
        }

        public void Delete(VideoCourse model)
        {
            _dBContext.Remove(model);
        }
        public Task<VideoCourse?> GetOneAsync(Guid videoId)
        {
            return _dBContext.VideoCourses.Where(x => x.VideoId == videoId).FirstOrDefaultAsync();
        }
    }
}
