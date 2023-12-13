using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RoomRental.Data;
using RoomRental.Models;

namespace RoomRental.Services
{
    public class RentalService : CachedService<Rental>
    {
        private readonly HttpContext _httpContext;
        private readonly RoomService _roomService;
        private readonly InvoiceService _invoiceCache;
        private readonly int _organizationId;
        public RentalService(RoomRentalsContext context, IMemoryCache memoryCache, UserManager<User> userManager, HttpContextAccessor httpContext, RoomService roomService, InvoiceService invoiceService)
            : base(memoryCache, context, "Rentals" + (httpContext.HttpContext.User.IsInRole("Admin") ? "" : userManager.GetUserAsync(httpContext.HttpContext.User).Result.OrganizationId), userManager.GetUserAsync(httpContext.HttpContext.User).Result)
        {
            _httpContext = httpContext.HttpContext;
            _roomService = roomService;
            _organizationId = userManager.GetUserAsync(httpContext.HttpContext.User).Result.OrganizationId;
            _invoiceCache = invoiceService;
        }

        public async override Task<Rental> Get(int? id)
        {
            return (await GetAll()).Single(e => e.RentalId == id);
        }
        public async override Task Add(Rental rental)
        {
            await _context.AddAsync(rental);
            await _context.SaveChangesAsync();
            _cache.Remove("Rentals" + rental.RentalOrganizationId); ////////////////////////////////////////////////////////////////
            await UpdateCache();
        }
        public async override Task Update(Rental rental)
        {
            _context.Update(rental);
            await _context.SaveChangesAsync();
            await UpdateCache();
        }
        public override async Task Delete(Rental rental)
        {
            var invoices = _context.Invoices.Where(e => rental.CheckInDate == e.ConclusionDate && rental.RentalOrganizationId == e.RentalOrganizationId && rental.Room.RoomId == e.RoomId).ToArray();
            if (invoices != null)
            {
                _context.Invoices.RemoveRange(invoices);
            }

            if (rental != null)
            {
                _context.Rentals.Remove(rental);
            }

            await _context.SaveChangesAsync();
            _invoiceCache.RemoveCache();

            _cache.Remove("Invoices" + _user.OrganizationId);
            _cache.Remove("Invoices");

            await UpdateCache();
        }

        public override async Task<List<Rental>> UpdateCache()
        {
            List<Rental> rentals = null;
            if (_httpContext.User.IsInRole("Admin"))
                rentals = await _context.Rentals
                    .Include(e => e.Room)
                    .Include(e => e.RentalOrganization)
                    .ToListAsync();
            else if (_httpContext.User.IsInRole("User"))
                rentals = await _context.Rentals
                    .Where(r => _roomService.GetAll().Result.Select(e => e.RoomId).Contains(r.RoomId) || r.RentalOrganizationId == _organizationId)
                    .Include(e => e.Room)
                    .Include(e => e.RentalOrganization)
                    .ToListAsync();

            if (rentals != null)
            {
                Console.WriteLine($"Список извлечен из базы данных");
                _cache.Set(_name, rentals, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }
            return rentals;
        }

        public async Task RemoveCache()
        {
            _cache.Remove(_name);
        }
    }
}