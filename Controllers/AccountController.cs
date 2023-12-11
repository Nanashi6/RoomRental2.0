using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoomRental.Models;
using RoomRental.Services;
using RoomRental.ViewModels.IdentityViewModels;

namespace RoomRental.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly OrganizationService _organizationCache;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, OrganizationService organizationCache)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _organizationCache = organizationCache;
        }
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewBag.OrganizationId = new SelectList(await _organizationCache.GetAll(), "OrganizationId", "Name");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User { Email = model.Email, UserName = model.Login, Surname = model.Surname, Name = model.Name, Lastname = model.Lastname, OrganizationId = model.OrganizationId };
                // добавляем пользователя
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRolesAsync(user, new List<string>() { "User" });
                    // установка куки
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            ViewBag.OrganizationId = new SelectList(await _organizationCache.GetAll(), "OrganizationId", "Name", model.OrganizationId);

            return View(model);
        }
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(model.Login, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    // проверяем, принадлежит ли URL приложению
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Неправильный логин и (или) пароль");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            // удаляем аутентификационные куки
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> AccessDenied()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
