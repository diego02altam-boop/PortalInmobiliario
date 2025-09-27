using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Controllers;

[Authorize]
public class VisitasController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public VisitasController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> Crear(int inmuebleId, DateTime fechaInicio, DateTime fechaFin, string? notas)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Validación horario laboral (8am–7pm)
        if (fechaInicio.Hour < 8 || fechaFin.Hour > 19)
        {
            TempData["Error"] = "Las visitas deben estar entre las 08:00 y 19:00.";
            return RedirectToAction("Details", "Inmuebles", new { id = inmuebleId });
        }

        // Validación rango
        if (fechaInicio >= fechaFin)
        {
            TempData["Error"] = "La fecha de inicio debe ser menor que la fecha fin.";
            return RedirectToAction("Details", "Inmuebles", new { id = inmuebleId });
        }

        // Validación solapamiento
        bool existeSolape = await _db.Visitas
            .AnyAsync(v => v.InmuebleId == inmuebleId &&
                           v.FechaInicio < fechaFin &&
                           v.FechaFin > fechaInicio);
        if (existeSolape)
        {
            TempData["Error"] = "Ya existe una visita en ese horario para este inmueble.";
            return RedirectToAction("Details", "Inmuebles", new { id = inmuebleId });
        }

        var visita = new Visita
        {
            InmuebleId = inmuebleId,
            UsuarioId = user.Id,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Notas = notas
        };

        _db.Visitas.Add(visita);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Visita agendada correctamente.";
        return RedirectToAction("Details", "Inmuebles", new { id = inmuebleId });
    }
}
