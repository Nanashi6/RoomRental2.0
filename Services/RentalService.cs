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
        private readonly int _organizationId;
        public RentalService(RoomRentalsContext context, IMemoryCache memoryCache, UserManager<User> userManager, HttpContextAccessor httpContext, RoomService roomService)
            : base(memoryCache, context, "Rentals" + (httpContext.HttpContext.User.IsInRole("Admin") ? "" : userManager.GetUserAsync(httpContext.HttpContext.User).Result.OrganizationId))
        {
            _httpContext = httpContext.HttpContext;
            _roomService = roomService;
            _organizationId = userManager.GetUserAsync(httpContext.HttpContext.User).Result.OrganizationId;
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
            _cache.Remove("Rentals" + rental.RentalOrganizationId); ////////////////////////////////////////////////////////////////////
            await UpdateCache();
        }
        public override async Task Delete(Rental rental)
        {
            if (rental != null)
            {
                _context.Rentals.Remove(rental);
            }

            await _context.SaveChangesAsync();
            _cache.Remove("Rentals" + rental.RentalOrganizationId); ////////////////////////////////////////////////////////////////////////
            await UpdateCache();
        }

        protected override async Task<List<Rental>> UpdateCache()
        {
            List<Rental> rentals = null;
            if (_httpContext.User.IsInRole("Admin"))
                rentals = await _context.Rentals
                    .Include(e => e.Room)
                    .Include(e => e.RentalOrganization)
                    .Select(i => new Rental()
                    {
                        RentalId = i.RentalId,
                        CheckInDate = i.CheckInDate,
                        CheckOutDate = i.CheckOutDate,
                        RentalOrganizationId = i.RentalOrganizationId,
                        Amount = i.Amount,
                        RentalOrganization = new Organization()
                        {
                            OrganizationId = i.RentalOrganizationId,
                            Name = i.RentalOrganization.Name,
                        },
                        Room = new Room()
                        {
                            RoomId = i.RoomId,
                            BuildingId = i.Room.BuildingId,
                            RoomNumber = i.Room.RoomNumber,
                            Area = i.Room.Area,
                            Building = new Building
                            {
                                Name = i.Room.Building.Name
                            }
                        }
                    })
                    .ToListAsync();
            else if (_httpContext.User.IsInRole("User"))
                rentals = await _context.Rentals
                    .Where(r => _roomService.GetAll().Result.Select(e => e.RoomId).Contains(r.RoomId) || r.RentalOrganizationId == _organizationId)
                    .Include(e => e.Room)
                    .Include(e => e.RentalOrganization)
                    .Select(i => new Rental
                    {
                        RentalId = i.RentalId,
                        CheckInDate = i.CheckInDate,
                        CheckOutDate = i.CheckOutDate,
                        RentalOrganizationId = i.RentalOrganizationId,
                        Amount = i.Amount,
                        RentalOrganization = new Organization
                        {
                            OrganizationId = i.RentalOrganizationId,
                            Name = i.RentalOrganization.Name,
                        },
                        Room = new Room
                        {
                            RoomId = i.RoomId,
                            BuildingId = i.Room.BuildingId,
                            RoomNumber = i.Room.RoomNumber,
                            Area = i.Room.Area,
                            Building = new Building
                            {
                                Name = i.Room.Building.Name
                            }
                        }
                    })
                    .ToListAsync();

            if (rentals != null)
            {
                Console.WriteLine($"Список извлечен из базы данных");
                _cache.Set(_name, rentals, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }
            return rentals;
        }
    }
}