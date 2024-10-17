using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneLibrary.Models;
using Microsoft.Extensions.Options;
using System.DirectoryServices.Protocols;
using System.Net;

namespace Keystone.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly LdapConfig _ldapConfiguration;

        public LoginModel(SignInManager<IdentityUser> signInManager,
                          ILogger<LoginModel> logger,
                          UserManager<IdentityUser> userManager,
                          IOptions<LdapConfig> ldapConfiguration)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _ldapConfiguration = ldapConfiguration.Value;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var succeeded = IsLdapAuthenticatedUser(Input.Username, Input.Password);
                
                if (succeeded)
                {
                    var user = await _userManager.FindByNameAsync(Input.Username);
                    if (user == null)
                    {
                        user = new IdentityUser 
                        {
                            UserName = Input.Username
                        };

                        await _userManager.CreateAsync(user);
                    }

                    await _signInManager.SignInAsync(user, true);
                    return LocalRedirect(returnUrl);
                }
            }

            return Page();
        }

        public bool IsLdapAuthenticatedUser(string username, string password)
        {
            // MIGRATE RECHECK
            string userDn = $"{username}@{_ldapConfiguration.Domain}";

            try
            {
                var identifier = new LdapDirectoryIdentifier(_ldapConfiguration.Url, _ldapConfiguration.Port);
                using (var connection = new LdapConnection(identifier))
                {
                    connection.SessionOptions.SecureSocketLayer = false;

                    // Set credentials and authentication type (basic auth)
                    var credential = new NetworkCredential(userDn, password);
                    connection.Credential = credential;
                    connection.AuthType = AuthType.Basic;

                    // Bind the connection
                    connection.Bind(); // This will throw an exception if it fails

                    _logger.LogInformation("-------------Passed");
                    return true;
                }
            }
            catch (LdapException ex)
            {
                _logger.LogInformation($"-------{ex}");
            }
            //string userDn = $"{ username }@{ _ldapConfiguration.Domain }";
            //try
            //{
            //    using (var connection = new LdapConnection { SecureSocketLayer = false })
            //    {
            //        connection.Connect(_ldapConfiguration.Url, _ldapConfiguration.Port);
            //        connection.Bind(userDn, password);
                    
            //        if (connection.Bound)
            //            _logger.LogInformation("-------------Passed");
            //            return true;
            //    }
            //}
            //catch (LdapException ex)
            //{
            //    // Log exception
            //    _logger.LogInformation($"-------{ ex }");
            //}
            return false;
        }
    }
}