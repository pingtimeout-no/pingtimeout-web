using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PingTimeout.Web.Data;
using PingTimeout.Web.Models;

namespace PingTimeout.Web.Controllers {
    public class AdminController : Controller
    {

        private readonly IOptions<PingTimeoutConfig> config;
        private ApplicationDbContext context;
 
        private UserManager<IdentityUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        public AdminController(ApplicationDbContext context, IOptions<PingTimeoutConfig> config, UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
        {
            this.context = context;
            this.config = config;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task<IActionResult> Elevate() {

            bool adminExists = await roleManager.RoleExistsAsync("Admin");
            if (!adminExists)
            {
                var role = new IdentityRole();
                role.Name = "Admin";
                await roleManager.CreateAsync(role);
            }

            var adminConfig = config.Value.Admins;
            if (!string.IsNullOrWhiteSpace(adminConfig)) {

                var admins = config.Value.Admins.Split(',');
                foreach (var admin in admins) {
                    var user = await userManager.FindByEmailAsync(admin);
                    if (user != null)
                        await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            return RedirectToAction("Index", "Home");
        }

    }
}