using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Washme.Identity.Service.Models;

namespace Washme.Identity.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(AuthUser login)
        {
            var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, login.RememberLogin, lockoutOnFailure: true);
            return Ok(result);
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(AuthUser authUser)
        {
            var user = new ApplicationUser
            {
                UserName = authUser.Email,
                Email = authUser.Email,
                EmailConfirmed = true,
            };

            var result = await _userManager.CreateAsync(user, authUser.Password);
            return Ok(result);

        }
    }
}
