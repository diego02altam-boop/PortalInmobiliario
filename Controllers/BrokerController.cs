using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Controllers;

[Authorize(Roles = "Broker")]
public class BrokerController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IDistributedCache _cache;

    public BrokerController(ApplicationDbContext db, IDistributedCache cache)
    { _db = db; _cache = cache; }

    // ---------- INMUEBLES ----------
    public async Task<IActionResult> Index()
    {
        var lista = await _db.Inmuebles.OrderByDescending(i => i.Activo)
                                       .ThenBy(i => i.Ciudad).ToListAsync();
        return View(lista);
    }

    public IActionResult Create() => View(new Inmueble());

    [HttpPost]
    public async Task<IActionResult> Create(Inmueble m)
    {
        if (!ModelState.IsValid) return View(m);
        _db.Add(m);
        await _db.SaveChangesAsync();
        await BumpCatalogVersion();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var m = await _db.Inmuebles.FindAsync(id);
        if (m is null) return NotFound();
        return View(m);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Inmueble m)
    {
        if (!ModelState.IsValid) return View(m);
        _db.Update(m);
        await _db.SaveChangesAsync();
        await BumpCatalogVersion();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(int id)
    {
        var m = await _db.Inmuebles.FindAsync(id);
        if (m is null) return NotFound();
        m.Activo = !m.Activo;
        await _db.SaveChangesAsync();
        await BumpCatalogVersion();
        return RedirectToAction(nameof(Index));
    }

    // ---------- AGENDA (solo lectura) ----------
    public async Task<IActionResult> AgendaHoy()
    {
        var hoy = DateTime.Today;
        var man = hoy.AddDays(1);

        var visitas = await _db.Visitas
            .Include(v => v.Inmueble)
            .Where(v => v.FechaInicio >= hoy && v.FechaInicio < man)
            .OrderBy(v => v.Inmueble.Ciudad).ThenBy(v => v.FechaInicio)
            .ToListAsync();

        return View(visitas);
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmarVisita(int id)
    {
        var v = await _db.Visitas.FindAsync(id);
        if (v is null) return NotFound();
        v.Estado = EstadoVisita.Confirmada;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(AgendaHoy));
    }

    [HttpPost]
    public async Task<IActionResult> CancelarVisita(int id)
    {
        var v = await _db.Visitas.FindAsync(id);
        if (v is null) return NotFound();
        v.Estado = EstadoVisita.Cancelada;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(AgendaHoy));
    }

    // ---------- RESERVAS ACTIVAS ----------
    public async Task<IActionResult> ReservasActivas()
    {
        var ahora = DateTime.UtcNow;
        var rs = await _db.Reservas.Include(r => r.Inmueble)
            .Where(r => r.FechaExpiracion > ahora)
            .OrderBy(r => r.FechaExpiracion).ToListAsync();
        return View(rs);
    }

    [HttpPost]
    public async Task<IActionResult> LiberarReserva(int id)
    {
        var r = await _db.Reservas.FindAsync(id);
        if (r is null) return NotFound();
        r.FechaExpiracion = DateTime.UtcNow; // liberar
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(ReservasActivas));
    }

    // --------- Invalidación simple de caché catálogo ---------
    private async Task BumpCatalogVersion()
    {
        await _cache.SetStringAsync("catalogo:ver", Guid.NewGuid().ToString());
    }
}
