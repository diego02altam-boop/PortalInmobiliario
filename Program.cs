using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity con roles (para Pregunta 5)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(o =>
{
    o.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

builder.Services.AddControllersWithViews();

// Cache distribuida: Redis si hay cadena, sino memoria.
var redis = builder.Configuration.GetValue<string>("Redis:ConnectionString");
if (!string.IsNullOrWhiteSpace(redis))
    builder.Services.AddStackExchangeRedisCache(o => o.Configuration = redis);
else
    builder.Services.AddDistributedMemoryCache();

// Sesión (se usará en Pregunta 4)
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromHours(1);
    o.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Migrar y sembrar datos mínimos
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    await Seed.SeedAsync(db);

    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleMgr.RoleExistsAsync("Broker"))
        await roleMgr.CreateAsync(new IdentityRole("Broker"));
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages(); // UI de Identity

app.Run();

static class Seed
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (await db.Inmuebles.AnyAsync()) return;

        db.Inmuebles.AddRange(
            new Inmueble { Codigo="DEP-001", Titulo="Depto céntrico", Tipo=TipoInmueble.Departamento, Ciudad="Bogotá", Direccion="Calle 1 #2-3", Dormitorios=2, Banos=2, MetrosCuadrados=58, Precio=255000000, Activo=true },
            new Inmueble { Codigo="CAS-101", Titulo="Casa familiar", Tipo=TipoInmueble.Casa, Ciudad="Jamundí", Direccion="Cra 10 #20-30", Dormitorios=3, Banos=3, MetrosCuadrados=100, Precio=350000000, Activo=true },
            new Inmueble { Codigo="LOC-210", Titulo="Local comercial", Tipo=TipoInmueble.Local, Ciudad="Rionegro", Direccion="Av 4 #12-19", Dormitorios=0, Banos=1, MetrosCuadrados=54, Precio=190000000, Activo=true },
            new Inmueble { Codigo="OFI-501", Titulo="Oficina moderna", Tipo=TipoInmueble.Oficina, Ciudad="Barranquilla", Direccion="Cll 80 #50-10", Dormitorios=0, Banos=1, MetrosCuadrados=80, Precio=321100000, Activo=true }
        );

        await db.SaveChangesAsync();
    }
}
