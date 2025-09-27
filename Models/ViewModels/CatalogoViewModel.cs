using System.ComponentModel.DataAnnotations;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Models.ViewModels;

public class CatalogoFiltros : IValidatableObject
{
    public string? Ciudad { get; set; }
    public TipoInmueble? Tipo { get; set; }
    [Range(0,double.MaxValue)] public decimal? PrecioMin { get; set; }
    [Range(0,double.MaxValue)] public decimal? PrecioMax { get; set; }
    [Range(0,int.MaxValue)] public int? Dormitorios { get; set; }
    [Range(1,int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1,100)] public int PageSize { get; set; } = 12;
    public IEnumerable<ValidationResult> Validate(ValidationContext _) {
        if (PrecioMin.HasValue && PrecioMax.HasValue && PrecioMin > PrecioMax)
            yield return new ValidationResult("Precio mínimo no puede ser mayor al máximo.",
                new[] { nameof(PrecioMin), nameof(PrecioMax) });
    }
}

public class CatalogoVM
{
    public CatalogoFiltros Filtros { get; set; } = new();
    public List<Inmueble> Resultados { get; set; } = new();
    public int Total { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / Math.Max(Filtros.PageSize,1));
}
