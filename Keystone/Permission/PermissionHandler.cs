using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using KeystoneLibrary.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Keystone.Permission
{
    //create this thing from https://learn.microsoft.com/en-us/answers/questions/1300372/how-to-set-authorize-for-each-page-and-each-module
    // but it doesn't work so will randomly modify it 

    public static class PolicyGenerator
    {
        public const string Prefix = "Permit";
        public const string Separator = "_";
        public const string Write = "Write";
    }

    public class PermissionAuthorizeAttribute : AuthorizeAttribute
    {
        public PermissionAuthorizeAttribute(string controller, string action)
        {
            Policy = $"{PolicyGenerator.Prefix}{PolicyGenerator.Separator}{controller}{PolicyGenerator.Separator}{action}";
        }
    }

    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Controller { get; }

        public string Action { get; }
        public string Permission { get; }
        public PermissionRequirement(
            string controller, string action)
        {
            Controller = controller;
            Action = action;
            Permission = $"{PolicyGenerator.Prefix}{PolicyGenerator.Separator}{controller}{PolicyGenerator.Separator}{action}";
        }
    }

    public class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermissionAuthorizationPolicyProvider(
            IOptions<AuthorizationOptions> options) : base(options) { }
        public override async Task<AuthorizationPolicy> GetPolicyAsync(
            string policyName)
        {
            if (!policyName.StartsWith(PolicyGenerator.Prefix, StringComparison.OrdinalIgnoreCase))
                return await base.GetPolicyAsync(policyName);

            //  extract the permissions from the policyname ,you could modify the method for your requirement
            string[] permissions = GetPermissionsFromPolicy(policyName);

            var requirement = new PermissionRequirement(permissions[1], permissions[2]);
            // create a policy, add the requirement
            return new AuthorizationPolicyBuilder()
                .AddRequirements(requirement).Build();
        }
        public string[] GetPermissionsFromPolicy(string policyName)
        {
            return policyName.Split(PolicyGenerator.Separator);
        }
    }
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public PermissionHandler(IServiceScopeFactory scopeFactory)
        {
            _serviceScopeFactory = scopeFactory;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, PermissionRequirement requirement)
        {

            if (context.User.HasClaim("SpecialPermission", "Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (_db != null && context != null && context.User != null)
                {
                    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userId != null)
                    {
                        var roleIds = _db.UserRoles.AsNoTracking().Where(x => x.UserId == userId).Select(x => x.RoleId).ToList();

                        var menu = _db.MenuPermissions.AsNoTracking()
                                                      .Where(x => x.UserId == userId
                                                                && x.Menu.Url.Replace("Index", "").Replace("/", "") == requirement.Controller
                                                            )
                                                      .ToList();
                        var roleMenuPermission = _db.MenuPermissions.AsNoTracking()
                                                                    .Where(x => roleIds.Contains(x.RoleId)
                                                                              && x.Menu.Url.Replace("Index", "").Replace("/", "") == requirement.Controller
                                                                          )
                                                                    .ToList();
                        if (requirement.Action == PolicyGenerator.Write)
                        {
                            bool isWriteable = (menu != null && menu.Any(x => x.IsWritable)) || (roleMenuPermission != null && roleMenuPermission.Any(x => x.IsWritable));
                            if (isWriteable)
                            {
                                context.Succeed(requirement);
                                return Task.CompletedTask;
                            }
                        }
                        else
                        {
                            bool isReadable = (menu != null && menu.Any(x => x.IsReadable)) || (roleMenuPermission != null && roleMenuPermission.Any(x => x.IsReadable));
                            if (isReadable)
                            {
                                context.Succeed(requirement);
                                return Task.CompletedTask;
                            }
                        }
                    }
                }
            }

            context.Fail();
            return Task.CompletedTask;
        }
    }
}
