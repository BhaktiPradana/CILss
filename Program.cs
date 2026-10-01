using LssTraining.Web.Data;
using LssTraining.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/", "Authenticated");
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Participants/Index");
    options.Conventions.AllowAnonymousToPage("/Participants/Roadmap");
    options.Conventions.AllowAnonymousToPage("/Participants/Export");
    options.Conventions.AllowAnonymousToPage("/Training/Index");
    options.Conventions.AllowAnonymousToPage("/Training/Details");
    options.Conventions.AllowAnonymousToPage("/Training/Enrollments");
    options.Conventions.AllowAnonymousToPage("/Training/Progress");
    options.Conventions.AllowAnonymousToPage("/Reviews/Index");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/Register");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AllowAnonymousToPage("/Privacy");
    options.Conventions.AllowAnonymousToPage("/Electricity/Index"); 
    options.Conventions.AllowAnonymousToPage("/Electricity/DbMapping");
    options.Conventions.AllowAnonymousToPage("/Electricity/DbMappingAdd");
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Path = "/ci-hub";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();
builder.Services.AddScoped<ITrainingRepository, TrainingRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IParticipantExportService, ParticipantExportService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<KwhRepository>();
builder.Services.AddScoped<DistributionBoardRepository>();

var app = builder.Build();

app.UsePathBase("/ci-hub");

if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // app.UseExceptionHandler("/Error");
    // app.UseHsts();
    // app.UseHttpsRedirection(); 
} 
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.MapGet("/Participants/Export/CILeanSixSigma_Participants.xlsx", async (HttpContext context, IParticipantExportService exportService, CancellationToken ct) =>
{
    var bytes = await exportService.GenerateExcelExportAsync(ct);
    context.Response.Headers.ContentDisposition = "attachment; filename=\"CILeanSixSigma_Participants.xlsx\"";
    return Results.File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CILeanSixSigma_Participants.xlsx");
}).AllowAnonymous();

app.MapGet("/Participants/Export", async (HttpContext context, IParticipantExportService exportService, CancellationToken ct) =>
{
    var bytes = await exportService.GenerateExcelExportAsync(ct);
    context.Response.Headers.ContentDisposition = "attachment; filename=\"CILeanSixSigma_Participants.xlsx\"";
    return Results.File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CILeanSixSigma_Participants.xlsx");
}).AllowAnonymous();

app.Run();

public partial class Program { }