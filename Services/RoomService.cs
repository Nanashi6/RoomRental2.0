using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using RoomRental.Data;
using RoomRental.Models;

namespace RoomRental.Services
{
    public class RoomService : CachedService<Room>
    {
        private readonly HttpContextAccessor _httpContext;
        private readonly BuildingService _buildingService;
        public RoomService(RoomRentalsContext context, IMemoryCache memoryCache, UserManager<User> userManager, HttpContextAccessor httpContext,
            BuildingService buildingService)
            : base(memoryCache, context, "Rooms" + (httpContext.HttpContext.User.IsInRole("Admin") ? "" : userManager.GetUserAsync(httpContext.HttpContext.User).Result.OrganizationId), userManager.GetUserAsync(httpContext.HttpContext.User).Result)
        { 
            _httpContext = httpContext;
            _buildingService = buildingService;
        }

        public async override Task<Room> Get(int? id)
        {
            return (await GetAll()).Single(e => e.RoomId == id);
        }

        public async Task<Room> TryGet(int? id)
        {                
            return _context.Rooms.Where(r => r.RoomId == id).Include(e => e.Building).First();
        }

        public override async Task<int?> Add(Room room)
        {
            EntityEntry<Room> entRoom = await _context.AddAsync(room);
            await _context.SaveChangesAsync();
            await UpdateCache();

            _cache.Remove("RoomImages");

            return entRoom.Entity.RoomId;
        }

        public async override Task Delete(Room room)
        {
            var rentals = await _context.Rentals.Where(e => e.RoomId == room.RoomId).ToListAsync();
            var invoices = await _context.Invoices.Where(e => e.RoomId == room.RoomId).ToListAsync();

            if (rentals != null)
            {
                _context.Rentals.RemoveRange(rentals);
            }
            if (invoices != null)
            {
                _context.Invoices.RemoveRange(invoices);
            }
            await _context.SaveChangesAsync();

            if (room != null)
            {
                _context.Rooms.Remove(room);
            }

            await _context.SaveChangesAsync();

            _cache.Remove("Rentals" + _user.OrganizationId);
            _cache.Remove("Rentals");
            _cache.Remove("Invoices" + _user.OrganizationId);
            _cache.Remove("Invoices");

            await UpdateCache();
        }

        public async override Task<List<Room>> UpdateCache()
        {
            List<Room> rooms = null;
            if (_httpContext.HttpContext.User.IsInRole("Admin"))
                rooms = await _context.Rooms
                    .Include(r => r.Building)
                    .Include(r => r.RoomImages)
                    .ToListAsync();
            else if (_httpContext.HttpContext.User.IsInRole("User"))
                rooms = await _context.Rooms
                    .Where(r => _buildingService.GetAll().Result.Select(e => e.BuildingId).Contains(r.BuildingId))
                    .Include(r => r.Building)
                    .Include(r => r.RoomImages)
                    .ToListAsync();

            if (rooms != null)
            {
                Console.WriteLine($"Список извлечен из базы данных");
                _cache.Set(_name, rooms, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }
            return rooms;
        }
        public async Task RemoveCache()
        {
            _cache.Remove(_name);
        }
    }
}