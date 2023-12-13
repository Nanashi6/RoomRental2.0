using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RoomRental.Attributes;
using RoomRental.Models;
using RoomRental.Services;
using RoomRental.ViewModels;
using RoomRental.ViewModels.FilterViewModels;
using RoomRental.ViewModels.SortStates;
using RoomRental.ViewModels.SortViewModels;

namespace RoomRental.Controllers
{
    [Authorize]
    public class RentalsController : Controller
    {
        private readonly RentalService _cache;
        private readonly BuildingService _buildingCache;
        private readonly OrganizationService _organizationCache;
        private readonly RoomService _roomCache;
        private readonly int _pageSize = 10;

        private readonly UserManager<User> _userManager;
        private readonly HttpContext _httpContext;

        public RentalsController(RentalService cache, OrganizationService organizationCache, RoomService roomCache, BuildingService buildingCache,
            IConfiguration appConfig, UserManager<User> userManager, HttpContextAccessor httpContext)
        {
            _cache = cache;
            _buildingCache = buildingCache;
            _organizationCache = organizationCache;
            _roomCache = roomCache;
            _pageSize = int.Parse(appConfig["Parameters:PageSize"]);
            _httpContext = httpContext.HttpContext;
            _userManager = userManager;
        }

        // GET: Rentals
        public async Task<IActionResult> Index(RentalFilterViewModel filterViewModel, int page = 1, RentalSortState sortOrder = RentalSortState.OrganizationNameAsc)
        {
            var dict = Infrastructure.SessionExtensions.Get(HttpContext.Session, "Rental");

            /*if (dict != null)
            {
                filterViewModel.OrganizationNameFind = dict["OrganizationNameFind"];

                if (dict.ContainsKey("BuildingIdFind") && Int32.TryParse(dict["BuildingIdFind"], out int id))
                    filterViewModel.BuildingIdFind = id;
                else
                    filterViewModel.BuildingIdFind = null;

                DateTime data;
                if (dict.ContainsKey("CheckInDateStartFind") && DateTime.TryParse(dict["CheckInDateStartFind"], out data))
                    filterViewModel.CheckInDateStartFind = data;
                else
                    filterViewModel.CheckInDateStartFind = null;

                if (dict.ContainsKey("CheckInDateEndFind") && DateTime.TryParse(dict["CheckInDateEndFind"], out data))
                    filterViewModel.CheckInDateEndFind = data;
                else
                    filterViewModel.CheckInDateEndFind = null;

                if (dict.ContainsKey("CheckOutDateStartFind") && DateTime.TryParse(dict["CheckOutDateStartFind"], out data))
                    filterViewModel.CheckOutDateStartFind = data;
                else
                    filterViewModel.CheckOutDateStartFind = null;

                if (dict.ContainsKey("CheckOutDateEndFind") && DateTime.TryParse(dict["CheckOutDateEndFind"], out data))
                    filterViewModel.CheckOutDateEndFind = data;
                else
                    filterViewModel.CheckOutDateEndFind = null;
            }*/

            List<Rental> rentals = null;
            if (_httpContext.User.IsInRole("Admin"))
                rentals = await _cache.GetAll();
            else
                rentals = (await _cache.GetAll()).Where(e => e.RentalOrganizationId != _userManager.GetUserAsync(_httpContext.User).Result.OrganizationId).ToList();

            rentals = await SortSearch(rentals, dict, filterViewModel, sortOrder);

            /*//Фильтрация
            if (!String.IsNullOrEmpty(filterViewModel.OrganizationNameFind))
                rentals = rentals.Where(e => e.RentalOrganization.Name.Contains(filterViewModel.OrganizationNameFind)).ToList();
            if (filterViewModel.BuildingIdFind != null && filterViewModel.BuildingIdFind != 0)
                rentals = rentals.Where(e => e.Room.BuildingId == filterViewModel.BuildingIdFind).ToList();

            if (filterViewModel.CheckInDateStartFind != null && filterViewModel.CheckOutDateEndFind != null)
                rentals = rentals.Where(e => (e.CheckOutDate >= filterViewModel.CheckInDateStartFind && e.CheckOutDate <= filterViewModel.CheckOutDateEndFind) || (e.CheckInDate <= filterViewModel.CheckOutDateEndFind && e.CheckInDate >= filterViewModel.CheckInDateStartFind) || (e.CheckInDate <= filterViewModel.CheckInDateStartFind && e.CheckOutDate >= filterViewModel.CheckOutDateEndFind)).ToList();
            else if (filterViewModel.CheckInDateStartFind != null)
                rentals = rentals.Where(e => e.CheckInDate >= filterViewModel.CheckInDateStartFind).ToList();
            else if (filterViewModel.CheckOutDateEndFind != null)
                rentals = rentals.Where(e => e.CheckOutDate <= filterViewModel.CheckOutDateEndFind).ToList();

            if (filterViewModel.CheckInDateStartFind != null && filterViewModel.CheckInDateEndFind != null)
                rentals = rentals.Where(e => e.CheckInDate <= filterViewModel.CheckInDateEndFind && e.CheckInDate >= filterViewModel.CheckInDateStartFind).ToList();
            else if (filterViewModel.CheckInDateEndFind != null)
                rentals = rentals.Where(e => e.CheckInDate <= filterViewModel.CheckInDateEndFind).ToList();

            if (filterViewModel.CheckOutDateStartFind != null && filterViewModel.CheckOutDateEndFind != null)
                rentals = rentals.Where(e => e.CheckOutDate >= filterViewModel.CheckOutDateStartFind && e.CheckOutDate <= filterViewModel.CheckOutDateEndFind).ToList();
            else if (filterViewModel.CheckOutDateStartFind != null)
                rentals = rentals.Where(e => e.CheckOutDate >= filterViewModel.CheckOutDateStartFind).ToList();

            //Сортировка
            switch (sortOrder)
            {
                case RentalSortState.OrganizationNameAsc:
                    rentals = rentals.OrderBy(e => e.RentalOrganization.Name).ToList();
                    break;
                case RentalSortState.OrganizationNameDesc:
                    rentals = rentals.OrderByDescending(e => e.RentalOrganization.Name).ToList();
                    break;
                case RentalSortState.CheckInDateAsc:
                    rentals = rentals.OrderBy(e => e.CheckInDate).ToList();
                    break;
                case RentalSortState.CheckInDateDesc:
                    rentals = rentals.OrderByDescending(e => e.CheckInDate).ToList();
                    break;
                case RentalSortState.CheckOutDateAsc:
                    rentals = rentals.OrderBy(e => e.CheckOutDate).ToList();
                    break;
                default:
                    rentals = rentals.OrderByDescending(e => e.CheckOutDate).ToList();
                    break;
            }*/

            //Разбиение на страницы
            int count = rentals.Count();
            rentals = rentals.Skip((page - 1) * _pageSize).Take(_pageSize).ToList();

            //Формирование модели представления
            RentalsViewModel rentalsViewModel = new RentalsViewModel()
            {
                Rentals = rentals,
                PageViewModel = new PageViewModel(page, count, _pageSize),
                FilterViewModel = filterViewModel,
                SortViewModel = new RentalSortViewModel(sortOrder)
            };

            var buildings = (await _buildingCache.GetAll()).ToList();
            buildings.Insert(0, new Building() { BuildingId = 0, Name = "Все здания" });
            if (filterViewModel.BuildingIdFind != null)
                ViewData["BuildingId"] = new SelectList(buildings, "BuildingId", "Name", filterViewModel.BuildingIdFind);
            else
                ViewData["BuildingId"] = new SelectList(buildings, "BuildingId", "Name");
            return View(rentalsViewModel);
        }

        // GET: Rentals/Filter
        [HttpGet]
        [SetSession("Rental")]
        public async Task<IActionResult> Filter(RentalFilterViewModel filterViewModel, RentalSortState sortOrder = RentalSortState.OrganizationNameAsc)
        {
            var routeValues = new RouteValueDictionary
            {
                { "filterViewModel", filterViewModel },
                { "sortOrder", sortOrder }
            };

            return RedirectToAction(nameof(Index), routeValues);
        }

        // GET: Rentals
        public async Task<IActionResult> OurRentals(RentalFilterViewModel filterViewModel, int page = 1, RentalSortState sortOrder = RentalSortState.OrganizationNameAsc)
        {
            var dict = Infrastructure.SessionExtensions.Get(HttpContext.Session, "OurRental");

            /*if (dict != null)
            {
                if (dict.ContainsKey("BuildingIdFind") && Int32.TryParse(dict["BuildingIdFind"], out int id))
                    filterViewModel.BuildingIdFind = id;
                else
                    filterViewModel.BuildingIdFind = null;

                DateTime data;
                if (dict.ContainsKey("CheckInDateStartFind") && DateTime.TryParse(dict["CheckInDateStartFind"], out data))
                    filterViewModel.CheckInDateStartFind = data;
                else
                    filterViewModel.CheckInDateStartFind = null;

                if (dict.ContainsKey("CheckInDateEndFind") && DateTime.TryParse(dict["CheckInDateEndFind"], out data))
                    filterViewModel.CheckInDateEndFind = data;
                else
                    filterViewModel.CheckInDateEndFind = null;

                if (dict.ContainsKey("CheckOutDateStartFind") && DateTime.TryParse(dict["CheckOutDateStartFind"], out data))
                    filterViewModel.CheckOutDateStartFind = data;
                else
                    filterViewModel.CheckOutDateStartFind = null;

                if (dict.ContainsKey("CheckOutDateEndFind") && DateTime.TryParse(dict["CheckOutDateEndFind"], out data))
                    filterViewModel.CheckOutDateEndFind = data;
                else
                    filterViewModel.CheckOutDateEndFind = null;
            }*/

            var rentals = (await _cache.GetAll()).Where(e => e.RentalOrganizationId == _userManager.GetUserAsync(_httpContext.User).Result.OrganizationId).ToList();

            rentals = await SortSearch(rentals, dict, filterViewModel, sortOrder);

            /*//Фильтрация
            if (!String.IsNullOrEmpty(filterViewModel.OrganizationNameFind))
                rentals = rentals.Where(e => e.RentalOrganization.Name.Contains(filterViewModel.OrganizationNameFind)).ToList();
            if (filterViewModel.BuildingIdFind != null && filterViewModel.BuildingIdFind != 0)
                rentals = rentals.Where(e => e.Room.BuildingId == filterViewModel.BuildingIdFind).ToList();

            if (filterViewModel.CheckInDateStartFind != null && filterViewModel.CheckOutDateEndFind != null)
                rentals = rentals.Where(e => (e.CheckOutDate >= filterViewModel.CheckInDateStartFind && e.CheckOutDate <= filterViewModel.CheckOutDateEndFind) || (e.CheckInDate <= filterViewModel.CheckOutDateEndFind && e.CheckInDate >= filterViewModel.CheckInDateStartFind) || (e.CheckInDate <= filterViewModel.CheckInDateStartFind && e.CheckOutDate >= filterViewModel.CheckOutDateEndFind)).ToList();
            else if (filterViewModel.CheckInDateStartFind != null)
                rentals = rentals.Where(e => e.CheckInDate >= filterViewModel.CheckInDateStartFind).ToList();
            else if (filterViewModel.CheckOutDateEndFind != null)
                rentals = rentals.Where(e => e.CheckOutDate <= filterViewModel.CheckOutDateEndFind).ToList();

            if (filterViewModel.CheckInDateStartFind != null && filterViewModel.CheckInDateEndFind != null)
                rentals = rentals.Where(e => e.CheckInDate <= filterViewModel.CheckInDateEndFind && e.CheckInDate >= filterViewModel.CheckInDateStartFind).ToList();
            else if (filterViewModel.CheckInDateEndFind != null)
                rentals = rentals.Where(e => e.CheckInDate <= filterViewModel.CheckInDateEndFind).ToList();

            if (filterViewModel.CheckOutDateStartFind != null && filterViewModel.CheckOutDateEndFind != null)
                rentals = rentals.Where(e => e.CheckOutDate >= filterViewModel.CheckOutDateStartFind && e.CheckOutDate <= filterViewModel.CheckOutDateEndFind).ToList();
            else if (filterViewModel.CheckOutDateStartFind != null)
                rentals = rentals.Where(e => e.CheckOutDate >= filterViewModel.CheckOutDateStartFind).ToList();

            //Сортировка
            switch (sortOrder)
            {
                case RentalSortState.OrganizationNameAsc:
                    rentals = rentals.OrderBy(e => e.RentalOrganization.Name).ToList();
                    break;
                case RentalSortState.OrganizationNameDesc:
                    rentals = rentals.OrderByDescending(e => e.RentalOrganization.Name).ToList();
                    break;
                case RentalSortState.CheckInDateAsc:
                    rentals = rentals.OrderBy(e => e.CheckInDate).ToList();
                    break;
                case RentalSortState.CheckInDateDesc:
                    rentals = rentals.OrderByDescending(e => e.CheckInDate).ToList();
                    break;
                case RentalSortState.CheckOutDateAsc:
                    rentals = rentals.OrderBy(e => e.CheckOutDate).ToList();
                    break;
                default:
                    rentals = rentals.OrderByDescending(e => e.CheckOutDate).ToList();
                    break;
            }*/

            //Разбиение на страницы
            int count = rentals.Count();
            rentals = rentals.Skip((page - 1) * _pageSize).Take(_pageSize).ToList();

            //Формирование модели представления
            RentalsViewModel rentalsViewModel = new RentalsViewModel()
            {
                Rentals = rentals,
                PageViewModel = new PageViewModel(page, count, _pageSize),
                FilterViewModel = filterViewModel,
                SortViewModel = new RentalSortViewModel(sortOrder)
            };

            var buildings = (await _buildingCache.GetAll()).ToList();
            buildings.Insert(0, new Building() { BuildingId = 0, Name = "Все здания" });
            if (filterViewModel.BuildingIdFind != null)
                ViewData["BuildingId"] = new SelectList(buildings, "BuildingId", "Name", filterViewModel.BuildingIdFind);
            else
                ViewData["BuildingId"] = new SelectList(buildings, "BuildingId", "Name");
            return View(rentalsViewModel);
        }

        // GET: Rentals/Filter
        [HttpGet]
        [SetSession("OurRental")]
        public async Task<IActionResult> OurFilter(RentalFilterViewModel filterViewModel, RentalSortState sortOrder = RentalSortState.OrganizationNameAsc)
        {
            var routeValues = new RouteValueDictionary
            {
                { "filterViewModel", filterViewModel },
                { "sortOrder", sortOrder }
            };

            return RedirectToAction(nameof(OurRentals), routeValues);
        }

        // GET: Rentals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || await _cache.GetAll() == null)
            {
                return NotFound();
            }

            var rental = await _cache.Get(id);
            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // GET: Rentals/Create
        public async Task<IActionResult> Create()
        {
            List<Organization> organizations = await _organizationCache.GetAll();
            if (!_httpContext.User.IsInRole("Admin"))
                organizations = (await _organizationCache.GetAll()).Where(e => e.OrganizationId != (_userManager.GetUserAsync(_httpContext.User).Result).OrganizationId).ToList();

            ViewData["RentalOrganizationId"] = new SelectList(organizations, "OrganizationId", "Name");
            var rooms = (await _roomCache.GetAll()).Select(e => new { RoomId = e.RoomId, Name = $"{e.Building.Name}, №{e.RoomNumber}" });
            ViewData["RoomId"] = new SelectList(rooms, "RoomId", "Name");
            return View();
        }

        // POST: Rentals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RentalId,RoomId,RentalOrganizationId,CheckInDate,CheckOutDate,Amount")] Rental rental)
        {
            if (ModelState.IsValid)
            {
                await _cache.Add(rental);
                return RedirectToAction(nameof(Index));
            }
            List<Organization> organizations = await _organizationCache.GetAll();
            if (!_httpContext.User.IsInRole("Admin"))
                organizations = (await _organizationCache.GetAll()).Where(e => e.OrganizationId != (_userManager.GetUserAsync(_httpContext.User).Result).OrganizationId).ToList();

            ViewData["RentalOrganizationId"] = new SelectList(organizations, "OrganizationId", "Name", rental.RentalOrganizationId);
            var rooms = (await _roomCache.GetAll()).Select(e => new { RoomId = e.RoomId, Name = $"{e.Building.Name}, №{e.RoomNumber}" });
            ViewData["RoomId"] = new SelectList(rooms, "RoomId", "Name", rental.RoomId);
            return View(rental);
        }

        // GET: Rentals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || await _cache.GetAll() == null)
            {
                return NotFound();
            }

            var rental = await _cache.Get(id);
            if (rental == null)
            {
                return NotFound();
            }
            List<Organization> organizations = await _organizationCache.GetAll();
            if (!_httpContext.User.IsInRole("Admin"))
                organizations = (await _organizationCache.GetAll()).Where(e => e.OrganizationId != (_userManager.GetUserAsync(_httpContext.User).Result).OrganizationId).ToList();

            ViewData["RentalOrganizationId"] = new SelectList(organizations, "OrganizationId", "Name", rental.RentalOrganizationId);

            var rooms = (await _roomCache.GetAll()).Select(e => new { RoomId = e.RoomId, Info = e.Building.Name + ", №" + e.RoomNumber }).ToList();
            ViewData["RoomId"] = new SelectList(rooms, "RoomId", "Info", rental.RoomId);
            Console.WriteLine(rental.RoomId);
            return View(rental);
        }

        // POST: Rentals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RentalId,RoomId,RentalOrganizationId,CheckInDate,CheckOutDate,Amount")] Rental rental)
        {
            if (id != rental.RentalId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _cache.Update(rental);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!(await RentalExistsAsync(rental.RentalId)))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            List<Organization> organizations = await _organizationCache.GetAll();
            if (!_httpContext.User.IsInRole("Admin"))
                organizations = (await _organizationCache.GetAll()).Where(e => e.OrganizationId != (_userManager.GetUserAsync(_httpContext.User).Result).OrganizationId).ToList();

            ViewData["RentalOrganizationId"] = new SelectList(organizations, "OrganizationId", "Name", rental.RentalOrganizationId);
            var rooms = (await _roomCache.GetAll()).Select(e => new { RoomId = e.RoomId, Info = e.Building.Name + ", №" + e.RoomNumber }).ToList();

            ViewData["RoomId"] = new SelectList(rooms, "RoomId", "Info", rental.RoomId);
            return View(rental);
        }

        // GET: Rentals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || await _cache.GetAll() == null)
            {
                return NotFound();
            }

            var rental = await _cache.Get(id);
            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // POST: Rentals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await _cache.GetAll() == null)
            {
                return Problem("Entity set 'RoomRentalsContext.Rentals'  is null.");
            }
            await _cache.Delete(await _cache.Get(id));
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> RentalExistsAsync(int id)
        {
            return ((await _cache.GetAll())?.Any(e => e.RentalId == id)).GetValueOrDefault();
        }

        private async Task<List<Rental>> SortSearch(List<Rental> rentals, Dictionary<string, string> dict, RentalFilterViewModel filterViewModel, RentalSortState sortOrder = RentalSortState.OrganizationNameAsc)
        {
            if (dict != null)
            {
                filterViewModel.OrganizationNameFind = dict["OrganizationNameFind"];

                if (dict.ContainsKey("BuildingIdFind") && Int32.TryParse(dict["BuildingIdFind"], out int id))
                    filterViewModel.BuildingIdFind = id;
                else
                    filterViewModel.BuildingIdFind = null;

                DateTime data;
                if (dict.ContainsKey("CheckInDateStartFind") && DateTime.TryParse(dict["CheckInDateStartFind"], out data))
                    filterViewModel.CheckInDateStartFind = data;
                else
                    filterViewModel.CheckInDateStartFind = null;

                if (dict.ContainsKey("CheckInDateEndFind") && DateTime.TryParse(dict["CheckInDateEndFind"], out data))
                    filterViewModel.CheckInDateEndFind = data;
                else
                    filterViewModel.CheckInDateEndFind = null;

                if (dict.ContainsKey("CheckOutDateStartFind") && DateTime.TryParse(dict["CheckOutDateStartFind"], out data))
                    filterViewModel.CheckOutDateStartFind = data;
                else
                    filterViewModel.CheckOutDateStartFind = null;

                if (dict.ContainsKey("CheckOutDateEndFind") && DateTime.TryParse(dict["CheckOutDateEndFind"], out data))
                    filterViewModel.CheckOutDateEndFind = data;
                else
                    filterViewModel.CheckOutDateEndFind = null;
            }

            //Фильтрация
            if (!String.IsNullOrEmpty(filterViewModel.OrganizationNameFind))
                rentals = rentals.Where(e => e.RentalOrganization.Name.Contains(filterViewModel.OrganizationNameFind)).ToList();
            if (filterViewModel.BuildingIdFind != null && filterViewModel.BuildingIdFind != 0)
                rentals = rentals.Where(e => e.Room.BuildingId == filterViewModel.BuildingIdFind).ToList();

            if (filterViewModel.CheckInDateStartFind != null && filterViewModel.CheckOutDateEndFind != null)
                rentals = rentals.Where(e => (e.CheckOutDate >= filterViewModel.CheckInDateStartFind && e.CheckOutDate <= filterViewModel.CheckOutDateEndFind) || (e.CheckInDate <= filterViewModel.CheckOutDateEndFind && e.CheckInDate >= filterViewModel.CheckInDateStartFind) || (e.CheckInDate <= filterViewModel.CheckInDateStartFind && e.CheckOutDate >= filterViewModel.CheckOutDateEndFind)).ToList();
            else if (filterViewModel.CheckInDateStartFind != null)
                rentals = rentals.Where(e => e.CheckInDate >= filterViewModel.CheckInDateStartFind).ToList();
            else if (filterViewModel.CheckOutDateEndFind != null)
                rentals = rentals.Where(e => e.CheckOutDate <= filterViewModel.CheckOutDateEndFind).ToList();

            if (filterViewModel.CheckInDateStartFind != null && filterViewModel.CheckInDateEndFind != null)
                rentals = rentals.Where(e => e.CheckInDate <= filterViewModel.CheckInDateEndFind && e.CheckInDate >= filterViewModel.CheckInDateStartFind).ToList();
            else if (filterViewModel.CheckInDateEndFind != null)
                rentals = rentals.Where(e => e.CheckInDate <= filterViewModel.CheckInDateEndFind).ToList();

            if (filterViewModel.CheckOutDateStartFind != null && filterViewModel.CheckOutDateEndFind != null)
                rentals = rentals.Where(e => e.CheckOutDate >= filterViewModel.CheckOutDateStartFind && e.CheckOutDate <= filterViewModel.CheckOutDateEndFind).ToList();
            else if (filterViewModel.CheckOutDateStartFind != null)
                rentals = rentals.Where(e => e.CheckOutDate >= filterViewModel.CheckOutDateStartFind).ToList();

            //Сортировка
            switch (sortOrder)
            {
                case RentalSortState.OrganizationNameAsc:
                    rentals = rentals.OrderBy(e => e.RentalOrganization.Name).ToList();
                    break;
                case RentalSortState.OrganizationNameDesc:
                    rentals = rentals.OrderByDescending(e => e.RentalOrganization.Name).ToList();
                    break;
                case RentalSortState.CheckInDateAsc:
                    rentals = rentals.OrderBy(e => e.CheckInDate).ToList();
                    break;
                case RentalSortState.CheckInDateDesc:
                    rentals = rentals.OrderByDescending(e => e.CheckInDate).ToList();
                    break;
                case RentalSortState.CheckOutDateAsc:
                    rentals = rentals.OrderBy(e => e.CheckOutDate).ToList();
                    break;
                case RentalSortState.AmountAsc:
                    rentals = rentals.OrderBy(e => e.Amount).ToList();
                    break;
                case RentalSortState.AmountDesc:
                    rentals = rentals.OrderByDescending(e => e.Amount).ToList();
                    break;
                default:
                    rentals = rentals.OrderByDescending(e => e.CheckOutDate).ToList();
                    break;
            }

            return rentals;
        }
    }
}
