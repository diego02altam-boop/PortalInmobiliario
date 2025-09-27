using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Controllers;

[Authorize]
public class ReservasController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public ReservasController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> Crear(int inmuebleId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Validar si ya existe reserva activa
        bool reservaActiva = await _db.Reservas
            .AnyAsync(r => r.InmuebleId == inmuebleId && r.FechaExpiracion > DateTime.UtcNow);
        if (reservaActiva)
        {
            TempData["Error"] = "Ya existe una reserva activa para este inmueble.";
            return RedirectToAction("Details", "Inmuebles", new { id = inmuebleId });
        }

        var reserva = new Reserva
        {
            InmuebleId = inmuebleId,
            UsuarioId = user.Id,
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = DateTime.UtcNow.AddHours(48)
        };

        _db.Reservas.Add(reserva);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Reserva creada correctamente. Válida por 48 horas.";
        return RedirectToAction("Details", "Inmuebles", new { id = inmuebleId });
    }
}
