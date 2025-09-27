using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models.ViewModels;

namespace PortalInmobiliario.Controllers;

public class InmueblesController : Controller
{
    private readonly ApplicationDbContext _db;
    public InmueblesController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CatalogoFiltros f)
    {
        TryValidateModel(f);
        var q = _db.Inmuebles.AsNoTracking().Where(i => i.Activo);
        if (!string.IsNullOrWhiteSpace(f.Ciudad)) q = q.Where(i => i.Ciudad == f.Ciudad);
        if (f.Tipo.HasValue) q = q.Where(i => i.Tipo == f.Tipo);
        if (f.PrecioMin.HasValue) q = q.Where(i => i.Precio >= f.PrecioMin);
        if (f.PrecioMax.HasValue) q = q.Where(i => i.Precio <= f.PrecioMax);
        if (f.Dormitorios.HasValue) q = q.Where(i => i.Dormitorios >= f.Dormitorios);
        var total = await q.CountAsync();
        var res = await q.OrderBy(i=>i.Ciudad).ThenBy(i=>i.Precio)
                         .Skip((f.Page-1)*f.PageSize).Take(f.PageSize).ToListAsync();
        var vm = new CatalogoVM { Filtros = f, Resultados = res, Total = total };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var inm = await _db.Inmuebles.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id && i.Activo);
        if (inm == null) return NotFound();
        HttpContext.Session.SetInt32("UltimoInmuebleId", id);
        return View(inm);
    }
}
