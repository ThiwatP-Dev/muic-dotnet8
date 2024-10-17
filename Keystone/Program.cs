using KeystoneLibrary.Config;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using KeystoneLibrary.Helpers;
using System.ComponentModel;
using KeystoneLibrary.Services;
using KeystoneLibrary.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Keystone.BackgroundTask;
using Keystone.Permission;
using Keystone.Extensions;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.HttpOverrides;
using Vereyon.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
var configuration = builder.Configuration;
services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

// Specific dotnet ef command to lookup Migrations in KeystoneLibrary instead
var connectionString = configuration.GetConnectionString("DefaultConnection");
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
                         x => x.MigrationsAssembly("KeystoneLibrary")));
// To support ASP.NET Identity
services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

services.Configure<IdentityOptions>(options =>
{
    // Password settings.`
    options.Password.RequiredLength = 5;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
});

services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Account/Login";
    options.LogoutPath = $"/Account/Logout";
    options.AccessDeniedPath = $"/Account/AccessDenied";
});

services.AddHttpContextAccessor();

services.AddMemoryCache();

services.AddHttpClient();

services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-GB");
});

services.Configure<PaymentConfiguration>(configuration.GetSection("Payment"));
services.Configure<LdapConfig>(configuration.GetSection("Ldap"));


services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .SetIsOriginAllowed(x => true);
    });
});

services.AddControllersWithViews();
// services.AddMvc(options =>
// {
//     options.AllowCombiningAuthorizeFilters = false; //Bug of version 2.1 
// }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

services.AddAntiforgery(x => x.HeaderName = "X-XSRF-TOKEN");

services.AddAutoMapper(typeof(MappingProfile).Assembly);

TypeDescriptor.AddAttributes(typeof(DateTime), new TypeConverterAttribute(typeof(DefaultDateTimeFormat)));
// Only call RegisterDI to AddTransient (Real code is inside ./KeysonteLibrary/Helpers/RegisterDIComponent.cs)
services.RegisterDI();

//Flash Message
services.AddFlashMessage();
services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

//var serviceProvider = services.BuildServiceProvider();
//var httpclientFactory = serviceProvider.GetService<IHttpClientFactory>();

//services.AddSingleton<IHttpClientProxy>(x => new HttpClientProxy(httpclientFactory.CreateClient()));

services.AddSingleton<IHttpClientProxy>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    return new HttpClientProxy(httpClientFactory.CreateClient());
});

services.AddHostedService<LongRunningService>();
services.AddSingleton<BackgroundWorkerQueue>();

services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
// Overrides the DefaultAuthorizationPolicyProvider with our own
services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

// Register the Swagger generator, defining 1 or more Swagger documents
services.AddSwaggerGen(c =>
{
    // Define the Swagger document
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Keystone Open API",
        Description = "Keystone API needed api-key",
    });

    // Filter to include only certain APIs in the documentation
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        return apiDesc.RelativePath.Contains("HolidayAPI") || apiDesc.RelativePath.Contains("SectionAPI");
    });

    // MIGRATE RECHECK
    // Include XML comments from the current assembly
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // c.IncludeXmlComments(xmlPath);
    
    // MIGRATE RECHECK
    // Include XML comments from the KeystoneLibrary
    // xmlPath = Path.Combine(AppContext.BaseDirectory, "KeystoneLibrary.xml");
    // c.IncludeXmlComments(xmlPath);

    // Custom filters
    c.DocumentFilter<AddStandaloneSchemasFilter>();
    c.SchemaFilter<EnsureTypesAreIncludedFilter>();

    // MIGRATE RECHECK
    // Enable example filters
    // c.ExampleFilters();

    // Define security scheme for API key
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "x-api-key",
        Type = SecuritySchemeType.ApiKey,
        Description = "API Key needed to access the endpoints"
    });

    // Apply the security scheme globally
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] {}
        }
    });
});

// Make sure to add middleware for Swagger in the Configure method
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = "api-docs";
});

// Configure the HTTP request pipeline.
var env = app.Environment;
if (env.IsDevelopment() || env.IsStaging())
{
    app.UseDeveloperExceptionPage();
    app.UseDatabaseErrorPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    // port 80 is for running in docker
    context.Response.Headers.Add("X-Frame-Options", "ALLOW-FROM https://aukeystone.azurewebsites.net, https://localhost:5248, http://localhost:80");
    await next();
});

app.UseAuthentication();
app.UseStaticFiles(new StaticFileOptions()
{
    OnPrepareResponse = s =>
    {
        if (s.Context.Request.Path.StartsWithSegments(new PathString("/uploaded")) &&
           !s.Context.User.Identity.IsAuthenticated)
        {
            s.Context.Response.StatusCode = 401;
            s.Context.Response.Body = Stream.Null;
            s.Context.Response.ContentLength = 0;
        }
    }
});

if (env.IsDevelopment())
{
    app.UseStaticFiles(new StaticFileOptions()
    {
        FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), @"node_modules")),
        RequestPath = new PathString("/vendor")
    });
}

app.UseCors("CorsPolicy");
app.UseRequestLocalization();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "home",
    pattern: "/",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
