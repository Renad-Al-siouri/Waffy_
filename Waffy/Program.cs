using Microsoft.AspNetCore.Http.Features;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Repositories;
using Waffy.Repositories.Interfaces;
using Waffy.Repository;

var builder = WebApplication.CreateBuilder(args);

AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    var ex = e.ExceptionObject as Exception;
    var msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] FATAL\nType   : {ex?.GetType().FullName}\nMessage: {ex?.Message}\nStack  :\n{ex?.StackTrace}\n" + new string('-', 80) + "\n";
    File.AppendAllText("crash.log", msg);
    Console.WriteLine(msg);
};

TaskScheduler.UnobservedTaskException += (_, e) =>
{
    var msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] UNOBSERVED TASK\nType   : {e.Exception?.GetType().FullName}\nMessage: {e.Exception?.Message}\n" + new string('-', 80) + "\n";
    File.AppendAllText("crash.log", msg);
    Console.WriteLine(msg);
    e.SetObserved();
};

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 52_428_800;
    options.AllowSynchronousIO = true;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52_428_800;
    options.ValueLengthLimit = 52_428_800;
    options.MultipartHeadersLengthLimit = 52_428_800;
    options.MemoryBufferThreshold = 52_428_800;
});

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<IStoreDocumentRepository, StoreDocumentRepository>();
builder.Services.AddScoped<IClinicRepository, ClinicRepository>();
builder.Services.AddScoped<IClinicDocumentRepository, ClinicDocumentRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IAdoptionRepository, AdoptionRepository>();
builder.Services.AddScoped<ILostPetRepository, LostPetRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IFoundPetClaimRepository, FoundPetClaimRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IDonationRepository, DonationRepository>();
builder.Services.AddScoped<IAdminMapMarkerRepository, AdminMapMarkerRepository>();
builder.Services.AddScoped<IDangerousAnimalReportRepository, DangerousAnimalReportRepository>();
builder.Services.AddScoped<IComplaintRepository, ComplaintRepository>();
builder.Services.AddScoped<IDonationAnimalRepository, DonationAnimalRepository>();
builder.Services.AddScoped<IPetLawRepository, PetLawRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
builder.Services.AddScoped<IStoreImageRepository, StoreImageRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IPostFlagRepository, PostFlagRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ISurpriseRewardRepository, SurpriseRewardRepository>();
builder.Services.AddScoped<IPendingPaymentRepository, PendingPaymentRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IRewardLogRepository, RewardLogRepository>();
builder.Services.AddScoped<IClinicScheduleRepository, ClinicScheduleRepository>();
builder.Services.AddScoped<IClinicServiceRepository, ClinicServiceRepository>();
builder.Services.AddScoped<IClinicImageRepository, ClinicImageRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();




builder.Services.AddHostedService<Waffy.Services.AppointmentReminderService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Home_Before}/{id?}");

app.Run();