using BlogDomain.Model;
using BlogInfrastructure.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BlogInfrastructure.Controllers
{
    [Authorize(Roles = "admin, reader")]
    public class RolesController : Controller
    {
        RoleManager<IdentityRole> _roleManager;
        UserManager<User> _userManager;
        
        private readonly LldbContext _context;
        private readonly WritersController _writersController;
        private readonly ReadersController _readersController;
        
        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<User> userManager, LldbContext context, WritersController writersController, ReadersController readersController)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            
            _context = context;
            _writersController = writersController;
            _readersController = readersController;
        }
        
        [Authorize(Roles = "admin")]
        public IActionResult Index() => View(_roleManager.Roles.ToList());
        
        [Authorize(Roles = "admin")]
        public IActionResult UserList() => View(_userManager.Users.ToList());

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(string userId)
        {
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.ToList();
                ChangeRoleViewModel model = new ChangeRoleViewModel
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    UserRoles = userRoles,
                    AllRoles = allRoles
                };
                return View(model);
            }

            return NotFound();
        }
        
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(string userId, List<string> roles)
        {
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.ToList();
                var addedRoles = roles.Except(userRoles);
                var removedRoles = userRoles.Except(roles);

                await _userManager.AddToRolesAsync(user, addedRoles);

                await _userManager.RemoveFromRolesAsync(user, removedRoles);
                
                foreach (var role in addedRoles)
                {
                    switch (role)
                    {
                        case "writer":
                            await _writersController.AddWriter(user.UserName, user.Id);
                            break;
                        case "reader":
                            await _readersController.AddReader(user.UserName, user.Id);
                            break;
                    }
                }

                foreach (var role in removedRoles)
                {
                    switch (role)
                    {
                        case "writer":
                            await _writersController.RemoveWriter(user.Id);
                            break;
                        case "reader":
                            await _readersController.RemoveReader(user.Id);
                            break;
                    }
                }
                
                return RedirectToAction("UserList");
            }

            return NotFound();
        }
        
        [Authorize(Roles = "reader")]
        [HttpPost]
        public async Task<IActionResult> PromotionToWriter(string userId)
        {
            User user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            
            var isWriter = await _userManager.IsInRoleAsync(user, "writer");
            if (isWriter)
            {
                return Conflict("Користувач вже автор.");
            }
            
            var result = await _userManager.AddToRoleAsync(user, "writer");
            if (!result.Succeeded)
            {
                return StatusCode(500, "Не вийшло підвищити користувача до автора.");
            }
            
            await _writersController.AddWriter(user.UserName, user.Id);

            return Ok("Користувач підвищений до автора успішно. Для відображення змін вийдіть та зайдіть.");
        }
    }
}
