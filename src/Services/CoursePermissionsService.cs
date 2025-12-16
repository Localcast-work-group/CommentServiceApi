using CourseService.Contracts.Clients;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using CommentService.Api.Interfaces.Services;

namespace CommentService.Api.Services
{
    public class CoursePermissionsService : ICoursePermissionsService
    {
        private readonly IDistributedCache _cache;
        private readonly ICourseServiceClient _courseClient;
        private readonly Serilog.ILogger _logger;
        private readonly DistributedCacheEntryOptions _cacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
        };
        public CoursePermissionsService(
            IDistributedCache cache,
            ICourseServiceClient courseClient,
            Serilog.ILogger logger)
        {
            _cache = cache;
            _courseClient = courseClient;
            _logger = logger;
        }
        private static string GetCacheAccessKey(Guid userId, Guid courseId)
=> $"access:{userId}:{courseId}";
        private static string GetCacheCommentKey(Guid userId, Guid courseId)
        => $"comment:{userId}:{courseId}";
        private static string GetCacheModerateKey(Guid userId, Guid courseId)
            => $"moderate:{userId}:{courseId}";
        public async Task CacheAccessAsync(Guid userId, Guid courseId)
        {
            var key = GetCacheAccessKey(userId, courseId);
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(true), _cacheOptions);
            _logger.Information("Access cached for {UserId} -> {CourseId}", userId, courseId);
        }
        public async Task CacheCommentAsync(Guid userId, Guid courseId)
        {
            var key = GetCacheCommentKey(userId, courseId);
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(true), _cacheOptions);
            _logger.Information("Comment cached for {UserId} -> {CourseId}", userId, courseId);
        }
        public async Task CacheModerateAsync(Guid userId, Guid courseId)
        {
            var key = GetCacheModerateKey(userId, courseId);
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(true), _cacheOptions);
            _logger.Information("Moderate cached for {UserId} -> {CourseId}", userId, courseId);
        }

        public async Task<bool> CanCommentAsync(Guid userId, Guid courseId)
        {
            
            var key = GetCacheCommentKey(userId, courseId);

            var cachedValue = await _cache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return JsonSerializer.Deserialize<bool>(cachedValue);
            }

            try
            {
                var response = await _courseClient.CheckUserPermissionsAsync(userId,courseId);
                if (response != null && response.CanModerate )
                {
                    await CacheModerateAsync(userId, courseId);
                }
                if (response != null && response.CanWatch)
                {
                    await CacheAccessAsync(userId, courseId);
                }
                if (response != null && response.CanComment)
                {
                    await CacheCommentAsync(userId, courseId);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error calling CourseService API");
                return false;

            }

            return false;
        }
        public async Task<bool> CanModerateAsync(Guid userId, Guid courseId)
        {
            var key = GetCacheModerateKey(userId, courseId);

            var cachedValue = await _cache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return JsonSerializer.Deserialize<bool>(cachedValue);
            }

            try
            {
                var response = await _courseClient.CheckUserPermissionsAsync(userId, courseId);

                if (response != null && response.CanWatch)
                {
                    await CacheAccessAsync(userId, courseId);
                }

                if (response != null && response.CanComment)
                {
                    await CacheCommentAsync(userId, courseId);
                }
                if (response != null && response.CanModerate)
                {
                    await CacheModerateAsync(userId, courseId);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error calling CourseService API");
                return false;

            }

            return false;
        }
        public async Task<bool> HasAccessAsync(Guid userId, Guid courseId)
        {
            var key = GetCacheAccessKey(userId, courseId);

            var cachedValue = await _cache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return JsonSerializer.Deserialize<bool>(cachedValue);
            }

            try
            {
                var response = await _courseClient.CheckUserPermissionsAsync(userId, courseId);
                if (response != null && response.CanModerate)
                {
                    await CacheModerateAsync(userId, courseId);
                }
                if (response != null && response.CanComment)
                {
                    await CacheCommentAsync(userId, courseId);
                }
                if (response != null && response.CanWatch)
                {
                    await CacheAccessAsync(userId, courseId);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error calling CourseService API");
                return false;

            }

            return false;
        }
        public async Task InvalidateCommentAsync(Guid userId, Guid courseId)
        {
            var key = GetCacheCommentKey(userId, courseId);
            await _cache.RemoveAsync(key);
            _logger.Information("Comment invalidated for {UserId} -> {CourseId}", userId, courseId);
        }
        public async Task InvalidateModerateAsync(Guid userId, Guid courseId)
        {
            var key = GetCacheModerateKey(userId, courseId);
            await _cache.RemoveAsync(key);
            _logger.Information("Moderate invalidated for {UserId} -> {CourseId}", userId, courseId);
        }
        public async Task InvalidateAccessAsync(Guid userId, Guid courseId)
        {
            var key = GetCacheAccessKey(userId, courseId);
            await _cache.RemoveAsync(key);
            _logger.Information("Access invalidated for {UserId} -> {CourseId}", userId, courseId);
        }



    }
}
