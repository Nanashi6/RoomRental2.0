using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
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
    public class InvoicesController : Controller
    {
        private readonly InvoiceService _cache;
        private readonly OrganizationService _organizationCache;
        private readonly RoomService _roomCache;
        private readonly RentalService _rentalCache;
        private readonly int _pageSize = 10;

        private readonly UserManager<User> _userManager;
        private readonly HttpContext _httpContext;

        public InvoicesController(InvoiceService cache, RoomService roomCache, OrganizationService organizationCache, RentalService rentalService,
            IConfiguration appConfig, UserManager<User> userManager, HttpContextAccessor httpContext)
        {
            _cache = cache;
            _organizationCache = organizationCache;
            _roomCache = roomCache;
            _rentalCache = rentalService;
            _pageSize = int.Parse(appConfig["Parameters:PageSize"]);
            _userManager = userManager;
            _httpContext = httpContext.HttpContext;
        }

        // GET: Invoices
        public async Task<IActionResult> Index(InvoiceFilterViewModel filterViewModel, int page = 1, InvoiceSortState sortOrder = InvoiceSortState.OrganizationNameAsc)
        {
            var dict = Infrastructure.SessionExtensions.Get(HttpContext.Session, "Invoice");

            /*if (dict != null)
            {
                filterViewModel.ResponsiblePersonFind = dict["ResponsiblePersonFind"];
                filterViewModel.OrganizationNameFind = dict["OrganizationNameFind"];

                DateTime date;
                if (dict.ContainsKey("PaymentDateFind") && DateTime.TryParse(dict["PaymentDateFind"], out date))
                    filterViewModel.PaymentDateFind = date;
                else
                    filterViewModel.PaymentDateFind = null;

                if (dict.ContainsKey("ConclusionDateFind") && DateTime.TryParse(dict["ConclusionDateFind"], out date))
                    filterViewModel.ConclusionDateFind = date;
                else
                    filterViewModel.ConclusionDateFind = null;

                if (dict.ContainsKey("PermissionDateFind") && DateTime.TryParse(dict["PermissionDateFind"], out date))
                    filterViewModel.PermissionDateFind = date;
                else
                    filterViewModel.PermissionDateFind = null;

                Decimal amountFind;
                if (dict.ContainsKey("AmountFind") && Decimal.TryParse(dict["AmountFind"], out amountFind))
                    filterViewModel.AmountFind = amountFind;
                else
                    filterViewModel.AmountFind = null;
            }*/

            List<Invoice> invoices = null;
            if (_httpContext.User.IsInRole("Admin"))
                invoices = await _cache.GetAll();
            else
                invoices = (await _cache.GetAll()).Where(e => e.RentalOrganizationId != _userManager.GetUserAsync(_httpContext.User).Result.OrganizationId).ToList();

            invoices = await SortSearch(invoices, dict, filterViewModel, sortOrder);

            /*//Фильтрация
            if (!String.IsNullOrEmpty(filterViewModel.OrganizationNameFind))
                invoices = invoices.Where(e => e.RentalOrganization.Name.Contains(filterViewModel.OrganizationNameFind)).ToList();
            if (!String.IsNullOrEmpty(filterViewModel.ResponsiblePersonFind))
                invoices = invoices.Where(e => e.ResponsiblePerson.SNL.Contains(filterViewModel.ResponsiblePersonFind)).ToList();
            if (filterViewModel.AmountFind != null)
                invoices = invoices.Where(e => e.Amount == filterViewModel.AmountFind).ToList();
            if (filterViewModel.ConclusionDateFind != null)
                invoices = invoices.Where(e => e.ConclusionDate == filterViewModel.ConclusionDateFind).ToList();
            if (filterViewModel.PaymentDateFind != null)
                invoices = invoices.Where(e => e.PaymentDate == filterViewModel.PaymentDateFind).ToList();


            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            /// Фильтрация должников
            if (filterViewModel.PermissionDateFind != null)
                invoices = invoices.Where(e => e.PaymentDate > filterViewModel.PermissionDateFind && e.ConclusionDate <= filterViewModel.PermissionDateFind
                || (e.PaymentDate <= filterViewModel.PermissionDateFind
                        && (_rentalCache.GetAll().Result).Single(r => r.CheckInDate == e.ConclusionDate && r.RentalOrganizationId == e.RentalOrganizationId && r.Room.RoomId == e.RoomId).Amount > e.Amount))
                    .ToList();
            /////////////////////////////////////////////////////////////////////////////////////////////////////////


            //Сортировка
            switch (sortOrder)
            {
                case InvoiceSortState.OrganizationNameAsc:
                    invoices = invoices.OrderBy(e => e.RentalOrganization.Name).ToList();
                    break;
                case InvoiceSortState.OrganizationNameDesc:
                    invoices = invoices.OrderByDescending(e => e.RentalOrganization.Name).ToList();
                    break;
                case InvoiceSortState.PaymentDateAsc:
                    invoices = invoices.OrderBy(e => e.PaymentDate).ToList();
                    break;
                case InvoiceSortState.PaymentDateDesc:
                    invoices = invoices.OrderByDescending(e => e.PaymentDate).ToList();
                    break;
                case InvoiceSortState.ConclusionDateAsc:
                    invoices = invoices.OrderBy(e => e.ConclusionDate).ToList();
                    break;
                case InvoiceSortState.ConclusionDateDesc:
                    invoices = invoices.OrderByDescending(e => e.ConclusionDate).ToList();
                    break;
                case InvoiceSortState.AmountAsc:
                    invoices = invoices.OrderBy(e => e.Amount).ToList();
                    break;
                case InvoiceSortState.AmountDesc:
                    invoices = invoices.OrderByDescending(e => e.Amount).ToList();
                    break;
                case InvoiceSortState.ResponsiblePersonAsc:
                    invoices = invoices.OrderBy(e => e.ResponsiblePersonId).ToList();
                    break;
                default:
                    invoices = invoices.OrderByDescending(e => e.ResponsiblePersonId).ToList();
                    break;
            }*/

            //Разбиение на страницы
            int count = invoices.Count;
            invoices = invoices.Skip((page - 1) * _pageSize).Take(_pageSize).ToList();

            //Формирование модели представления
            InvoicesViewModel invoicesViewModel = new InvoicesViewModel()
            {
                Invoices = invoices,
                PageViewModel = new PageViewModel(page, count, _pageSize),
                FilterViewModel = filterViewModel,
                SortViewModel = new InvoiceSortViewModel(sortOrder)
            };

            return View(invoicesViewModel);
        }

        // GET: Invoices/Filter
        [HttpGet]
        [SetSession("Invoice")]
        public async Task<IActionResult> Filter(InvoiceFilterViewModel filterViewModel, InvoiceSortState sortOrder = InvoiceSortState.OrganizationNameAsc)
        {
            var routeValues = new RouteValueDictionary
            {
                { "filterViewModel", filterViewModel },
                { "sortOrder", sortOrder }
            };

            return RedirectToAction(nameof(Index), routeValues);
        }

        public async Task<IActionResult> OurInvoices(InvoiceFilterViewModel filterViewModel, int page = 1, InvoiceSortState sortOrder = InvoiceSortState.OrganizationNameAsc)
        {
            var dict = Infrastructure.SessionExtensions.Get(HttpContext.Session, "OurInvoice");

            /*if (dict != null)
            {
                filterViewModel.ResponsiblePersonFind = dict["ResponsiblePersonFind"];
                filterViewModel.OrganizationNameFind = dict["OrganizationNameFind"];

                DateTime date;
                if (dict.ContainsKey("PaymentDateFind") && DateTime.TryParse(dict["PaymentDateFind"], out date))
                    filterViewModel.PaymentDateFind = date;
                else
                    filterViewModel.PaymentDateFind = null;

                if (dict.ContainsKey("ConclusionDateFind") && DateTime.TryParse(dict["ConclusionDateFind"], out date))
                    filterViewModel.ConclusionDateFind = date;
                else
                    filterViewModel.ConclusionDateFind = null;

                if (dict.ContainsKey("PermissionDateFind") && DateTime.TryParse(dict["PermissionDateFind"], out date))
                    filterViewModel.PermissionDateFind = date;
                else
                    filterViewModel.PermissionDateFind = null;

                Decimal amountFind;
                if (dict.ContainsKey("AmountFind") && Decimal.TryParse(dict["AmountFind"], out amountFind))
                    filterViewModel.AmountFind = amountFind;
                else
                    filterViewModel.AmountFind = null;
            }*/

            var invoices = (await _cache.GetAll()).Where(e => e.RentalOrganizationId == _userManager.GetUserAsync(_httpContext.User).Result.OrganizationId).ToList();

            invoices = await SortSearch(invoices, dict, filterViewModel, sortOrder);

            /*//Фильтрация
            if (!String.IsNullOrEmpty(filterViewModel.ResponsiblePersonFind))
                invoices = invoices.Where(e => e.ResponsiblePerson.SNL.Contains(filterViewModel.ResponsiblePersonFind)).ToList();
            if (filterViewModel.AmountFind != null)
                invoices = invoices.Where(e => e.Amount == filterViewModel.AmountFind).ToList();
            if (filterViewModel.ConclusionDateFind != null)
                invoices = invoices.Where(e => e.ConclusionDate == filterViewModel.ConclusionDateFind).ToList();
            if (filterViewModel.PaymentDateFind != null)
                invoices = invoices.Where(e => e.PaymentDate == filterViewModel.PaymentDateFind).ToList();
            if (filterViewModel.PermissionDateFind != null)
                invoices = invoices.Where(e => e.PaymentDate > filterViewModel.PermissionDateFind && e.ConclusionDate <= filterViewModel.PermissionDateFind).ToList();

            //Сортировка
            switch (sortOrder)
            {
                case InvoiceSortState.OrganizationNameAsc:
                    invoices = invoices.OrderBy(e => e.RentalOrganization.Name).ToList();
                    break;
                case InvoiceSortState.OrganizationNameDesc:
                    invoices = invoices.OrderByDescending(e => e.RentalOrganization.Name).ToList();
                    break;
                case InvoiceSortState.PaymentDateAsc:
                    invoices = invoices.OrderBy(e => e.PaymentDate).ToList();
                    break;
                case InvoiceSortState.PaymentDateDesc:
                    invoices = invoices.OrderByDescending(e => e.PaymentDate).ToList();
                    break;
                case InvoiceSortState.ConclusionDateAsc:
                    invoices = invoices.OrderBy(e => e.ConclusionDate).ToList();
                    break;
                case InvoiceSortState.ConclusionDateDesc:
                    invoices = invoices.OrderByDescending(e => e.ConclusionDate).ToList();
                    break;
                case InvoiceSortState.AmountAsc:
                    invoices = invoices.OrderBy(e => e.Amount).ToList();
                    break;
                case InvoiceSortState.AmountDesc:
                    invoices = invoices.OrderByDescending(e => e.Amount).ToList();
                    break;
                case InvoiceSortState.ResponsiblePersonAsc:
                    invoices = invoices.OrderBy(e => e.ResponsiblePersonId).ToList();
                    break;
                default:
                    invoices = invoices.OrderByDescending(e => e.ResponsiblePersonId).ToList();
                    break;
            }*/

            //Разбиение на страницы
            int count = invoices.Count;
            invoices = invoices.Skip((page - 1) * _pageSize).Take(_pageSize).ToList();

            //Формирование модели представления
            InvoicesViewModel invoicesViewModel = new InvoicesViewModel()
            {
                Invoices = invoices,
                PageViewModel = new PageViewModel(page, count, _pageSize),
                FilterViewModel = filterViewModel,
                SortViewModel = new InvoiceSortViewModel(sortOrder)
            };

            return View(invoicesViewModel);
        }

        // GET: Invoices/OurFilter
        [HttpGet]
        [SetSession("OurInvoice")]
        public async Task<IActionResult> OurFilter(InvoiceFilterViewModel filterViewModel, InvoiceSortState sortOrder = InvoiceSortState.OrganizationNameAsc)
        {
            var routeValues = new RouteValueDictionary
            {
                { "filterViewModel", filterViewModel },
                { "sortOrder", sortOrder }
            };

            return RedirectToAction(nameof(OurInvoices), routeValues);
        }

        // GET: Invoices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || await _cache.GetAll() == null)
            {
                return NotFound();
            }

            var invoice = await _cache.Get(id);
            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // GET: Invoices/Create
        public async Task<IActionResult> Create()
        {
            List<Organization> organizations = await _organizationCache.GetAll();
            if (!_httpContext.User.IsInRole("Admin"))
                organizations = (await _organizationCache.GetAll()).Where(e => e.OrganizationId != (_userManager.GetUserAsync(_httpContext.User).Result).OrganizationId).ToList();

            ViewData["RentalOrganizationId"] = new SelectList(organizations, "OrganizationId", "Name");

            List<User> people;
            if (_httpContext.User.IsInRole("Admin"))
                people = _userManager.Users.ToList();
            else
                people = _userManager.Users.Where(u => u.OrganizationId == _userManager.GetUserAsync(_httpContext.User).Result.OrganizationId).ToList();

            ViewData["ResponsiblePerson"] = new SelectList(people, "Id", "SNL");
            var rooms = (await _roomCache.GetAll()).Select(e => new { RoomId = e.RoomId, Info = e.Building.Name + ", №" + e.RoomNumber }).ToList();

            ViewData["RoomId"] = new SelectList(rooms, "RoomId", "Info");
            return View();
        }

        // POST: Invoices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InvoiceId,RentalOrganizationId,RoomId,Amount,ConclusionDate,PaymentDate,ResponsiblePersonId")] Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                await _cache.Add(invoice);
                return RedirectToAction(nameof(Index));
            }
            List<Organization> organizations = await _organizationCache.GetAll();
            if (!_httpContext.User.IsInRole("Admin"))
                organizations = (await _organizationCache.GetAll()).Where(e => e.OrganizationId != (_userManager.GetUserAsync(_httpContext.User).Result).OrganizationId).ToList();

            ViewData["RentalOrganizationId"] = new SelectList(organizations, "OrganizationId", "Name", invoice.RentalOrganizationId);

            List<User> people;
            if (_httpContext.User.IsInRole("Admin"))
                people = _userManager.Users.ToList();
            else
                people = _userManager.Users.Where(u => u.OrganizationId == _userManager.GetUserAsync(_httpContext.User).Result.OrganizationId).ToList();

            ViewData["ResponsiblePerson"] = new SelectList(people, "Id", "SNL", invoice.ResponsiblePersonId);

            var rooms = (await _roomCache.GetAll()).Select(e => new { RoomId = e.RoomId, Info = e.Building.Name + ", №" + e.RoomNumber }).ToList();

            ViewData["RoomId"] = new SelectList(rooms, "RoomId", "Info", invoice.RoomId);
            return View(invoice);
        }

        // GET: Invoices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || await _cache.GetAll() == null)
            {
                return NotFound();
            }

            var invoice = await _cache.Get(id);
            if (invoice == null)
            {
                return NotFound();
            }
            List<Organization> organizations = await _organizationCache.GetAll();
            if (!_httpContext.User.IsInRole("Admin"))
                organizations = (await _organizationCache.GetAll()).Where(e => e.OrganizationId != (_userManager.GetUserAsync(_httpContext.User).Result).OrganizationId).ToList()

            ; ViewData["RentalOrganizationId"] = new SelectList(organizations, "OrganizationId", "Name", invoice.RentalOrganizationId);
            List<User> people;
            if (_httpContext.User.IsInRole("Admin"))
                people = _userManager.Users.ToList();
            else
                people = _userManager.Users.Where(u => u.OrganizationId == _userManager.GetUserAsync(_httpContext.User).Result.OrganizationId).ToList();

            ViewData["ResponsiblePerson"] = new SelectList(people, "Id", "SNL");

            var rooms = (await _roomCache.GetAll()).Select(e => new { RoomId = e.RoomId, Info = e.Building.Name + ", №" + e.RoomNumber }).ToList();

            ViewData["RoomId"] = new SelectList(rooms, "RoomId", "Info", invoice.RoomId);
            return View(invoice);
        }

        // POST: Invoices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("InvoiceId,RentalOrganizationId,RoomId,Amount,ConclusionDate,PaymentDate,ResponsiblePersonId")] Invoice invoice)
        {
            if (id != invoice.InvoiceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _cache.Update(invoice);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!(await InvoiceExists(invoice.InvoiceId)))
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

             ViewData["RentalOrganizationId"] = new SelectList(organizations, "OrganizationId", "Name", invoice.RentalOrganizationId);
            List<User> people;
            if (_httpContext.User.IsInRole("Admin"))
                people = _userManager.Users.ToList();
            else
                people = _userManager.Users.Where(u => u.OrganizationId == _userManager.GetUserAsync(_httpContext.User).Result.OrganizationId).ToList();

            ViewData["ResponsiblePerson"] = new SelectList(people, "Id", "SNL", invoice.ResponsiblePersonId);
            var rooms = (await _roomCache.GetAll()).Select(e => new { RoomId = e.RoomId, Info = e.Building.Name + ", №" + e.RoomNumber }).ToList();

            ViewData["RoomId"] = new SelectList(rooms, "RoomId", "Info", invoice.RoomId);
            return View(invoice);
        }

        // GET: Invoices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || await _cache.GetAll() == null)
            {
                return NotFound();
            }

            var invoice = await _cache.Get(id);
            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // POST: Invoices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await _cache.GetAll() == null)
            {
                return Problem("Entity set 'RoomRentalsContext.Invoices'  is null.");
            }
            await _cache.Delete(await _cache.Get(id));
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> InvoiceExists(int id)
        {
            return ((await _cache.GetAll())?.Any(e => e.InvoiceId == id)).GetValueOrDefault();
        }

        private async Task<List<Invoice>> SortSearch(List<Invoice> invoices, Dictionary<string, string> dict, InvoiceFilterViewModel filterViewModel, InvoiceSortState sortOrder = InvoiceSortState.OrganizationNameAsc)
        {
            if (dict != null)
            {
                filterViewModel.ResponsiblePersonFind = dict["ResponsiblePersonFind"];
                filterViewModel.OrganizationNameFind = dict["OrganizationNameFind"];

                DateTime date;
                if (dict.ContainsKey("PaymentDateFind") && DateTime.TryParse(dict["PaymentDateFind"], out date))
                    filterViewModel.PaymentDateFind = date;
                else
                    filterViewModel.PaymentDateFind = null;

                if (dict.ContainsKey("ConclusionDateFind") && DateTime.TryParse(dict["ConclusionDateFind"], out date))
                    filterViewModel.ConclusionDateFind = date;
                else
                    filterViewModel.ConclusionDateFind = null;

                if (dict.ContainsKey("PermissionDateFind") && DateTime.TryParse(dict["PermissionDateFind"], out date))
                    filterViewModel.PermissionDateFind = date;
                else
                    filterViewModel.PermissionDateFind = null;

                Decimal amountFind;
                if (dict.ContainsKey("AmountFind") && Decimal.TryParse(dict["AmountFind"], out amountFind))
                    filterViewModel.AmountFind = amountFind;
                else
                    filterViewModel.AmountFind = null;
            }

            //Фильтрация
            if (!String.IsNullOrEmpty(filterViewModel.OrganizationNameFind))
                invoices = invoices.Where(e => e.RentalOrganization.Name.Contains(filterViewModel.OrganizationNameFind)).ToList();
            if (!String.IsNullOrEmpty(filterViewModel.ResponsiblePersonFind))
                invoices = invoices.Where(e => e.ResponsiblePerson.SNL.Contains(filterViewModel.ResponsiblePersonFind)).ToList();
            if (filterViewModel.AmountFind != null)
                invoices = invoices.Where(e => e.Amount == filterViewModel.AmountFind).ToList();
            if (filterViewModel.ConclusionDateFind != null)
                invoices = invoices.Where(e => e.ConclusionDate == filterViewModel.ConclusionDateFind).ToList();
            if (filterViewModel.PaymentDateFind != null)
                invoices = invoices.Where(e => e.PaymentDate == filterViewModel.PaymentDateFind).ToList();


            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            /// Фильтрация должников
            if (filterViewModel.PermissionDateFind != null)
                invoices = invoices.Where(e => e.PaymentDate > filterViewModel.PermissionDateFind && e.ConclusionDate <= filterViewModel.PermissionDateFind
                || (e.PaymentDate <= filterViewModel.PermissionDateFind
                        && (_rentalCache.GetAll().Result).Single(r => r.CheckInDate == e.ConclusionDate && r.RentalOrganizationId == e.RentalOrganizationId && r.Room.RoomId == e.RoomId).Amount > e.Amount))
                    .ToList();
            /////////////////////////////////////////////////////////////////////////////////////////////////////////


            //Сортировка
            switch (sortOrder)
            {
                case InvoiceSortState.OrganizationNameAsc:
                    invoices = invoices.OrderBy(e => e.RentalOrganization.Name).ToList();
                    break;
                case InvoiceSortState.OrganizationNameDesc:
                    invoices = invoices.OrderByDescending(e => e.RentalOrganization.Name).ToList();
                    break;
                case InvoiceSortState.PaymentDateAsc:
                    invoices = invoices.OrderBy(e => e.PaymentDate).ToList();
                    break;
                case InvoiceSortState.PaymentDateDesc:
                    invoices = invoices.OrderByDescending(e => e.PaymentDate).ToList();
                    break;
                case InvoiceSortState.ConclusionDateAsc:
                    invoices = invoices.OrderBy(e => e.ConclusionDate).ToList();
                    break;
                case InvoiceSortState.ConclusionDateDesc:
                    invoices = invoices.OrderByDescending(e => e.ConclusionDate).ToList();
                    break;
                case InvoiceSortState.AmountAsc:
                    invoices = invoices.OrderBy(e => e.Amount).ToList();
                    break;
                case InvoiceSortState.AmountDesc:
                    invoices = invoices.OrderByDescending(e => e.Amount).ToList();
                    break;
                case InvoiceSortState.ResponsiblePersonAsc:
                    invoices = invoices.OrderBy(e => e.ResponsiblePersonId).ToList();
                    break;
                default:
                    invoices = invoices.OrderByDescending(e => e.ResponsiblePersonId).ToList();
                    break;
            }

            return invoices;
        }
    }
}
