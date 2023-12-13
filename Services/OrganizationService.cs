using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RoomRental.Data;
using RoomRental.Models;

namespace RoomRental.Services
{
    public class OrganizationService : CachedService<Organization>
    {
        public OrganizationService(RoomRentalsContext context, IMemoryCache memoryCache, UserManager<User> userManager, HttpContextAccessor httpContext) : base(memoryCache, context, "Organizations", userManager.GetUserAsync(httpContext.HttpContext.User).Result)
        {
        }

        public async override Task<Organization> Get(int? id)
        {
            return (await GetAll()).Single(e => e.OrganizationId == id);
        }

        public async override Task Delete(Organization organization)
        {
            var rentals = await _context.Rentals.Where(e => e.RentalOrganizationId == organization.OrganizationId).ToListAsync();
            var invoices = await _context.Invoices.Where(e => e.RentalOrganizationId == organization.OrganizationId).ToListAsync();

            var rooms = await _context.Rooms
                        .Where(r => _context.Buildings
                            .Where(b => organization.OrganizationId == b.OwnerOrganizationId)
                            .Select(b => b.BuildingId)
                            .Contains((int)r.BuildingId))
                        .Select(r => r.RoomId)
                        .ToListAsync();

            rentals.AddRange(await _context.Rentals.Where(r => rooms.Contains(r.RoomId)).ToListAsync());
            invoices.AddRange(await _context.Invoices.Where(i => rooms.Contains(i.RoomId)).ToListAsync());

            if (rentals != null)
            {
                _context.Rentals.RemoveRange(rentals);
            }
            if (invoices != null)
            {
                _context.Invoices.RemoveRange(invoices);
            }
            await _context.SaveChangesAsync();

            if (organization != null)
            {
                _context.Organizations.Remove(organization);
            }
            await _context.SaveChangesAsync();

            _cache.Remove("Buildings" + _user.OrganizationId);
            _cache.Remove("Buildings");
            _cache.Remove("Rooms" + _user.OrganizationId);
            _cache.Remove("Rooms");
            _cache.Remove("Rentals" + _user.OrganizationId);
            _cache.Remove("Rentals");
            _cache.Remove("Invoices" + _user.OrganizationId);
            _cache.Remove("Invoices");

            await UpdateCache();
        }

        public async override Task<List<Organization>> UpdateCache()
        {
            var organizations = await _context.Organizations.ToListAsync();
            // если пользователь найден, то добавляем в кэш - время кэширования 5 минут
            if (organizations != null)
            {
                Console.WriteLine($"Список извлечен из базы данных");
                _cache.Set(_name, organizations, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }
            return organizations;
        }
    }
}
