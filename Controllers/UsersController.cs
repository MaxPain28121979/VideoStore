using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoStore.Data;
using VideoStore.Models;
using VideoStore.Services;
using System.Security.Claims;

namespace VideoStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher<User> _hasher;

        public UsersController(AppDbContext db, IPasswordHasher<User> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _db.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role
            };

            return View(model);
        }

        public IActionResult Create()
        {
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (_db.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Email already taken");
            }

            if (!ModelState.IsValid) return View(model);

            var user = new User
            {
                Email = model.Email,
                Role = model.Role
            };
            user.PasswordHash = _hasher.HashPassword(user, model.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditUserViewModel model)
        {
            if (id != model.Id) return BadRequest();

            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (_db.Users.Any(u => u.Email == model.Email && u.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Email), "Email already taken");
            }

            if (!ModelState.IsValid) return View(model);

            user.Email = model.Email;
            user.Role = model.Role;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(currentUserId, out var parsedId) && parsedId == user.Id)
            {
                ModelState.AddModelError(string.Empty, "You cannot delete the account you are currently signed in with.");
                return View(user);
            }

            if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var adminCount = await _db.Users.CountAsync(u => u.Role == "Admin");
                if (adminCount <= 1)
                {
                    ModelState.AddModelError(string.Empty, "Cannot delete the last admin account.");
                    return View(user);
                }
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
