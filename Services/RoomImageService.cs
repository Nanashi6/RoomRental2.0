using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RoomRental.Data;
using RoomRental.Models;

namespace RoomRental.Services
{
    public class RoomImageService : CachedService<RoomImage>
    {
        public RoomImageService(RoomRentalsContext context, IMemoryCache memoryCache, UserManager<User> userManager, HttpContextAccessor httpContext) : base(memoryCache, context, "RoomImages", userManager.GetUserAsync(httpContext.HttpContext.User).Result) { }

        public async Task<RoomImage> GetImageAtId(int? id)
        {
            return (await GetAll()).Single(e => e.ImageId == id);
        }

        public async override Task<RoomImage> Get(int? id)
        {
            return (await GetAll()).Single(e => e.RoomId == id);
        }

        public async Task<List<RoomImage>> GetImageForRoom(int? roomId)
        {
            return (await GetAll()).Where(e => e.RoomId == roomId).ToList();
        }

        public override async Task Delete(RoomImage image)
        {
            if (image != null)
            {
                _context.RoomImages.Remove(image);
            }

            await _context.SaveChangesAsync();
            base.RefreshCache();

            await UpdateCache();
        }

        public async Task DeleteImageForRoom(int roomId)
        {
            _context.RemoveRange(_context.RoomImages.Where(e => roomId == e.RoomId));
            await _context.SaveChangesAsync();
            await UpdateCache();
        }

        public override async Task<List<RoomImage>> UpdateCache()
        {
            var images = await _context.RoomImages.ToListAsync();

            if (images != null)
            {
                Console.WriteLine($"Список извлечен из базы данных");
                _cache.Set(_name, images, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }
            return images;
        }
    }
}