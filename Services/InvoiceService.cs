using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RoomRental.Data;
using RoomRental.Models;

namespace RoomRental.Services
{
    public class InvoiceService : CachedService<Invoice>
    {
        private readonly RoomService _roomService;
        private readonly int _organizationId;
        private readonly bool _isAdmin;
        public InvoiceService(RoomRentalsContext context, IMemoryCache memoryCache, UserManager<User> userManager, HttpContextAccessor httpContext, RoomService roomService)
            : base(memoryCache, context, "Invoices" + (httpContext.HttpContext.User.IsInRole("Admin") ? "" : userManager.GetUserAsync(httpContext.HttpContext.User).Result.OrganizationId), userManager.GetUserAsync(httpContext.HttpContext.User).Result)
        {
            _roomService = roomService;
            _organizationId = userManager.GetUserAsync(httpContext.HttpContext.User).Result.OrganizationId;
            _isAdmin = httpContext.HttpContext.User.IsInRole("Admin");
        }
        /// <summary>
        /// Возвращает объект Invoice
        /// </summary>
        /// <returns></returns>
        public override async Task<Invoice> Get(int? id)
        {
            return (await GetAll()).Single(e => e.InvoiceId == id);
        }
        public async override Task Add(Invoice invoice)
        {
            await _context.AddAsync(invoice);
            await _context.SaveChangesAsync();
            _cache.Remove("Invoices" + invoice.RentalOrganizationId); ////////////////////////////////////////////////////////////////
            await UpdateCache();
        }
        public async override Task Update(Invoice invoice)
        {
            _context.Update(invoice);
            await _context.SaveChangesAsync();
            await UpdateCache();
        }
        /// <summary>
        /// Удаляет объект Invoice
        /// </summary>
        /// <returns></returns>
        public override async Task Delete(Invoice invoice)
        {
            invoice.ResponsiblePerson = null;
            _context.Entry(invoice).State = EntityState.Modified;
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
            }

            await _context.SaveChangesAsync();
            _cache.Remove("Rentals" + _user.OrganizationId);
            _cache.Remove("Rentals");
            await UpdateCache();
        }
        /// <summary>
        /// Обновляет кэш
        /// </summary>
        /// <returns></returns>
        public override async Task<List<Invoice>> UpdateCache()
        {
            List<Invoice> invoices = null;
            if (_isAdmin)
                invoices = await _context.Invoices
                    .Include(e => e.RentalOrganization)
                    .Include(e => e.ResponsiblePerson)
                    .ToListAsync();
            else
                invoices = await _context.Invoices
                    .Where(r => _roomService.GetAll().Result.Select(e => e.RoomId).Contains(r.RoomId) || r.RentalOrganizationId == _organizationId)
                    .Include(e => e.RentalOrganization)
                    .Include(e => e.ResponsiblePerson)
                    .ToListAsync();

            if (invoices != null)
            {
                Console.WriteLine($"Список извлечен из базы данных");
                _cache.Set(_name, invoices, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }
            return invoices;
        }

        public async Task RemoveCache()
        {
            _cache.Remove(_name);
        }
    }
}
