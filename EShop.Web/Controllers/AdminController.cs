using System.Collections.Generic;
using System.Threading.Tasks;
using EShop.Domain.DTO;
using EShop.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<EShopApplicationUser> _userManager;

        public AdminController(UserManager<EShopApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> ImportUsers([FromBody] List<UserDTO> users)
        {
            if (users == null || users.Count == 0)
                return BadRequest("No users provided.");

            var createdCount = 0;

            foreach (var u in users)
            {
                if (string.IsNullOrEmpty(u.Email) || string.IsNullOrEmpty(u.Password))
                    continue;

                var existing = await _userManager.FindByEmailAsync(u.Email);
                if (existing != null)
                    continue;

                var newUser = new EShopApplicationUser
                {
                    UserName = u.Email,
                    Email = u.Email,
                    EmailConfirmed = true
                };

                // if ConfirmPassword is null, just use Password
                var password = string.IsNullOrEmpty(u.ConfirmPassword)
                    ? u.Password
                    : u.ConfirmPassword;

                var result = await _userManager.CreateAsync(newUser, password);

                if (result.Succeeded)
                    createdCount++;
                // (you could log result.Errors here if you want)
            }

            return Ok(new { created = createdCount });
        }
    }
}