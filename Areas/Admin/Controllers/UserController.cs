using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebApp.Areas.Admin.Models;
using MyWebApp.Models; // Đổi từ MyWebApp.Areas.Admin.Models nếu cần
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)] // Chỉ Admin được phép truy cập
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, string>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "No Role";
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        public async Task<IActionResult> Promote(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any()) return BadRequest("User has no assigned role.");

            var currentRole = roles.First();

            if (currentRole == SD.Role_Customer && await _roleManager.RoleExistsAsync(SD.Role_Company))
            {
                await _userManager.RemoveFromRoleAsync(user, SD.Role_Customer);
                await _userManager.AddToRoleAsync(user, SD.Role_Company);
            }
            else if (currentRole == SD.Role_Company && await _roleManager.RoleExistsAsync(SD.Role_Admin))
            {
                await _userManager.RemoveFromRoleAsync(user, SD.Role_Company);
                await _userManager.AddToRoleAsync(user, SD.Role_Admin);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Demote(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any()) return BadRequest("User has no assigned role.");

            var currentRole = roles.First();

            if (currentRole == SD.Role_Admin)
            {
                await _userManager.RemoveFromRoleAsync(user, SD.Role_Admin);
                await _userManager.AddToRoleAsync(user, SD.Role_Company);
            }
            else if (currentRole == SD.Role_Company)
            {
                await _userManager.RemoveFromRoleAsync(user, SD.Role_Company);
                await _userManager.AddToRoleAsync(user, SD.Role_Customer);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest("Unable to delete user.");

            return RedirectToAction(nameof(Index));
        }
    }
}
