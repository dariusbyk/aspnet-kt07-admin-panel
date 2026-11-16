using AdminPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public AdminController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            List<IdentityUser> users = await userManager.Users.ToListAsync();
            List<UserRow> rows = new List<UserRow>();
            foreach (IdentityUser user in users)
            {
                UserRow row = new UserRow();
                row.Id = user.Id;
                row.UserName = user.UserName;
                row.Email = user.Email;
                row.Role = string.Join(", ", await userManager.GetRolesAsync(user));
                rows.Add(row);
            }
            return View(rows);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new UserForm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserForm model)
        {
            CheckRole(model.Role);
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Введите пароль.");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            IdentityUser user = new IdentityUser();
            user.UserName = model.UserName;
            user.Email = model.Email;
            IdentityResult result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                AddErrors(result);
                return View(model);
            }
            result = await userManager.AddToRoleAsync(user, model.Role);
            if (!result.Succeeded)
            {
                await userManager.DeleteAsync(user);
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            IdentityUser user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            UserForm model = new UserForm();
            model.UserName = user.UserName;
            model.Email = user.Email;
            if (await userManager.IsInRoleAsync(user, "Admin"))
            {
                model.Role = "Admin";
            }
            ViewBag.UserId = id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserForm model)
        {
            ViewBag.UserId = id;
            IdentityUser user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            CheckRole(model.Role);
            bool currentUser = userManager.GetUserId(User) == id;
            if (currentUser && model.Role != "Admin")
            {
                ModelState.AddModelError("", "Нельзя снять роль администратора у самого себя.");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            user.UserName = model.UserName;
            user.Email = model.Email;
            IdentityResult result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                AddErrors(result);
                return View(model);
            }

            if (!await userManager.IsInRoleAsync(user, model.Role))
            {
                var oldRoles = await userManager.GetRolesAsync(user);
                result = await userManager.RemoveFromRolesAsync(user, oldRoles);
                if (!result.Succeeded)
                {
                    AddErrors(result);
                    return View(model);
                }
                result = await userManager.AddToRoleAsync(user, model.Role);
                if (!result.Succeeded)
                {
                    await userManager.AddToRolesAsync(user, oldRoles);
                    AddErrors(result);
                    return View(model);
                }
                await userManager.UpdateSecurityStampAsync(user);
            }
            if (currentUser)
            {
                await signInManager.RefreshSignInAsync(user);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (userManager.GetUserId(User) == id)
            {
                TempData["Error"] = "Нельзя удалить собственную учётную запись.";
                return RedirectToAction("Index");
            }
            IdentityUser user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            IdentityResult result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Не удалось удалить пользователя.";
            }
            return RedirectToAction("Index");
        }

        public IActionResult Settings()
        {
            return View();
        }

        public IActionResult Reports()
        {
            return View();
        }

        private void CheckRole(string role)
        {
            if (role != "Admin" && role != "User")
            {
                ModelState.AddModelError("Role", "Выберите роль Admin или User.");
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
    }
}
