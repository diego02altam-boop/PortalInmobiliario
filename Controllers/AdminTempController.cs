using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace PortalInmobiliario.Controllers;

public class AdminTempController : Controller
{
    private readonly UserManager<IdentityUser> _um;
    private readonly RoleManager<IdentityRole> _rm;
    public AdminTempController(UserManager<IdentityUser> um, RoleManager<IdentityRole> rm)
    { _um = um; _rm = rm; }

    // Úsalo una vez: /AdminTemp/MakeBroker?email=tu@correo.com
    public async Task<IActionResult> MakeBroker(string email)
    {
        var u = await _um.FindByEmailAsync(email);
        if (u is null) return Content("Usuario no encontrado");
        if (!await _rm.RoleExistsAsync("Broker")) await _rm.CreateAsync(new IdentityRole("Broker"));
        await _um.AddToRoleAsync(u, "Broker");
        return Content("OK: agregado al rol Broker");
    }
}
