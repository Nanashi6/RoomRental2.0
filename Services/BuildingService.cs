using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RoomRental.Data;
using RoomRental.Models;

namespace RoomRental.Services
{
    public class BuildingService : CachedService<Building>
    {
        private readonly UserManager<User> _userManager;
        private readonly HttpContextAccessor _httpContext;
        public BuildingService(RoomRentalsContext context, IMemoryCache memoryCache, UserManager<User> userManager, HttpContextAccessor httpContext)
            : base(memoryCache, context, "Buildings" + (httpContext.HttpContext.User.IsInRole("Admin") ? "" : userManager.GetUserAsync(httpContext.HttpContext.User).Result.OrganizationId), userManager.GetUserAsync(httpContext.HttpContext.User).Result) 
        { 
            _userManager = userManager;
            _httpContext = httpContext;
        }
        /// <summary>
        /// Возвращает объект Building
        /// </summary>
        /// <returns></returns>
        public override async Task<Building> Get(int? id)
        {
            return (await GetAll()).Single(e => e.BuildingId == id);
        }

        public async Task<Building> TryGet(int? id)
        {
            return _context.Buildings.Single(r => r.BuildingId == id);
        }
        /// <summary>
        /// Удаляет объект Building
        /// </summary>
        /// <returns></returns>
        public override async Task Delete(Building building)
        {
            var rooms = await _context.Rooms
                        .Where(r => building.BuildingId == r.BuildingId)
                        .Select(r => r.RoomId)
                        .ToListAsync();

            var rentals = await _context.Rentals.Where(r => rooms.Contains(r.RoomId)).ToListAsync();
            var invoices = await _context.Invoices.Where(i => rooms.Contains(i.RoomId)).ToListAsync();

            if (rentals != null)
            {
                _context.Rentals.RemoveRange(rentals);
            }
            if (invoices != null)
            {
                _context.Invoices.RemoveRange(invoices);
            }
            await _context.SaveChangesAsync();

            if (building != null)
            {
                _context.Buildings.Remove(building);
            }
            await _context.SaveChangesAsync();

            _cache.Remove("Rooms" + _user.OrganizationId);
            _cache.Remove("Rooms");
            _cache.Remove("Rentals" + _user.OrganizationId);
            _cache.Remove("Rentals");
            _cache.Remove("Invoices" + _user.OrganizationId);
            _cache.Remove("Invoices");

            await UpdateCache();
        }
        /// <summary>
        /// Обновляет кэш
        /// </summary>
        /// <returns></returns>
        public override async Task<List<Building>> UpdateCache()
        {
            List<Building> buildings = null;
            if (_httpContext.HttpContext.User.IsInRole("Admin"))
                buildings = await _context.Buildings
                    .Include(e => e.OwnerOrganization)
                    .ToListAsync();
            else if (_httpContext.HttpContext.User.IsInRole("User"))
            {
                var organizationId = (await _userManager.GetUserAsync(_httpContext.HttpContext.User)).OrganizationId;
                buildings = await _context.Buildings
                    .Where(b => b.OwnerOrganizationId == organizationId)
                    .Include(e => e.OwnerOrganization)
                    .ToListAsync();
            }

            if (buildings != null)
            {
                Console.WriteLine($"Список извлечен из базы данных");
                _cache.Set(_name, buildings, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }
            return buildings.ToList();
        }

        public async Task RemoveCache()
        {
            _cache.Remove(_name);
        }
    }
}
