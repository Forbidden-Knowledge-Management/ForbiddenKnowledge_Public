using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;

using NLog.Web;
using NLog;
using Audit.WebApi;

using ForbiddenKnowledge.Components;
using ForbiddenKnowledge.Data;
using ForbiddenKnowledge.Services.Audit;
using ForbiddenKnowledge.Hubs;
using ForbiddenKnowledge.Services;
using ForbiddenKnowledge.Data.DbModels;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    //Setup Nlog for dependency Injection.
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
    builder.Host.UseNLog();

    // This method pulls in configurations set in 'appsettings.json'
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        // Configuration is automatically bound to Kestrel
    });

    //Retrieve configuration settings
    builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    });

    //Set up DB Contexts
    //First we make the DBContext for the ForbiddenKnowledgeContext.
    builder.Services.AddDbContext<ForbiddenKnowledgeContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("ForbiddenKnowledgeContext")));

    //We also need to make the separate DBContext for the audit logs
    builder.Services.AddDbContext<AuditDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("ForbiddenKnowledgeContext")), ServiceLifetime.Scoped);

    //services needed by the balzor web stack
    builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();
    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();
    builder.Services.AddHttpClient();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddSignalR(); // Add SignalR services. needed for SignalR logging.

    //.NET Identity Core Auth
    builder.Services.AddIdentityCore<User>(options =>
    {
        options.Password.RequiredLength = 12;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireDigit = true;
    })
    .AddSignInManager()
    .AddDefaultTokenProviders()
    .AddUserStore<CustomUserStore>();

    //ZM to-do: review these settings

    builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.LoginPath = "/Account/Login";                   // Adjust as needed
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);           // Cookie expiration
        options.SlidingExpiration = true;                        // Extend expiration on activity
        options.Cookie.HttpOnly = true;                          // Secure cookie
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Only over HTTPS
        options.Cookie.SameSite = SameSiteMode.Lax;             // Cross-site prevention
    });


    //Custom Services
    builder.Services.AddScoped<IUserStore<User>, CustomUserStore>();
    builder.Services.AddScoped<IBlogPostService, BlogPostService>();
    //builder.Services.AddScoped<OrderState>();


    var app = builder.Build();

    //************************************* Configure App *******************************************************

    app.UseMiddleware<ForbiddenKnowledge.Middleware.ErrorLogger>();

    // Configure Audit.NET to use our custom data provider
    Audit.Core.Configuration.DataProvider = new GenericAuditDataProvider(app.Services.CreateScope().ServiceProvider.GetService<AuditDbContext>());

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    // Add Audit.NET middleware to audit all requests
    app.UseAuditMiddleware(config => config
     .FilterByRequest(req => !req.Path.StartsWithSegments("/health")) // Ignore health check requests
     .WithEventType("{verb}:{url}") // Custom event type including HTTP verb and URL
     .IncludeHeaders() // Include request and response headers
     .IncludeRequestBody() // Include the request body in the audit event
     .IncludeResponseBody() // Include the response body in the audit event
     .IncludeResponseHeaders() // Optionally include response headers
    );

    app.MapRazorPages();
    app.MapBlazorHub();
    app.MapHub<LoggingHub>("/logginghub"); //This is needed to log SignalR activity
    //app.MapFallbackToPage("/_Host");
    app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");


    app.Run();

}
catch (Exception e)
{
    logger.Error(e, "Program stopped due to exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}