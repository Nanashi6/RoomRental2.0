using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RoomRental.Data;
using RoomRental.Models;
using RoomRental.Services;
using RoomRental.ViewModels.IdentityViewModels;

namespace RoomRental.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        UserManager<User> _userManager;
        RoomRentalsContext _context;
        OrganizationService _organizationCache;

        public UsersController(UserManager<User> userManager, RoomRentalsContext context, OrganizationService organizationCache)
        {
            _userManager = userManager;
            _context = context;
            _organizationCache = organizationCache;
        }

        public async Task<IActionResult> Index()
        {
            var AdminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var allUsers = await _userManager.Users.ToListAsync();

            var result = allUsers.Except(AdminUsers).ToList();

            return View(result);
        }
        public async Task<IActionResult> Create() 
        {
            ViewData["OrganizationId"] = new SelectList((await _organizationCache.GetAll()), "OrganizationId", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User { Email = model.Email, UserName = model.UserName, Surname = model.Surname, Name = model.Name, Lastname = model.Lastname, OrganizationId = model.OrganizationId };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            ViewData["OrganizationId"] = new SelectList((await _organizationCache.GetAll()), "OrganizationId", "Name", model.OrganizationId);
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            EditUserViewModel model = new EditUserViewModel { Id = user.Id, UserName = user.UserName, Email = user.Email, Surname = user.Surname, Name = user.Name, Lastname = user.Lastname, OrganizationId = user.OrganizationId };
            ViewData["OrganizationId"] = new SelectList((await _organizationCache.GetAll()), "OrganizationId", "Name", model.OrganizationId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    user.Email = model.Email;
                    user.UserName = model.UserName;
                    user.Surname = model.Surname;
                    user.Name = model.Name;
                    user.Lastname = model.Lastname;
                    user.OrganizationId = model.OrganizationId;

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
            }
            ViewData["OrganizationId"] = new SelectList((await _organizationCache.GetAll()), "OrganizationId", "Name", model.OrganizationId);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            User user = await _userManager.FindByIdAsync(id);

            var invoices = _context.Invoices.Where(e => e.ResponsiblePersonId == id).ToList();

            if (invoices != null)
            {
                _context.RemoveRange(invoices);
                _context.SaveChanges();
            }

            if (user != null)
            {
                IdentityResult result = await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> ChangePassword(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ChangePasswordViewModel model = new ChangePasswordViewModel { Id = user.Id, Email = user.Email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    var _passwordValidator =
                        HttpContext.RequestServices.GetService(typeof(IPasswordValidator<User>)) as IPasswordValidator<User>;
                    var _passwordHasher =
                        HttpContext.RequestServices.GetService(typeof(IPasswordHasher<User>)) as IPasswordHasher<User>;

                    IdentityResult result =
                        await _passwordValidator.ValidateAsync(_userManager, user, model.NewPassword);
                    if (result.Succeeded)
                    {
                        user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword);
                        await _userManager.UpdateAsync(user);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Пользователь не найден");
                }
            }
            return View(model);
        }
    }
}
