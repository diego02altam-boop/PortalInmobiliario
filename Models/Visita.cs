using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PortalInmobiliario.Models;

public class Visita : IValidatableObject
{
    public int Id { get; set; }

    public int InmuebleId { get; set; }
    public Inmueble Inmueble { get; set; } = default!;

    public string UsuarioId { get; set; } = default!;
    public IdentityUser Usuario { get; set; } = default!;

    [DataType(DataType.DateTime)]
    public DateTime FechaInicio { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime FechaFin { get; set; }

    public EstadoVisita Estado { get; set; } = EstadoVisita.Solicitada;

    [StringLength(500)]
    public string? Notas { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (FechaInicio >= FechaFin)
            yield return new ValidationResult("FechaInicio debe ser menor que FechaFin", new[] { "FechaInicio", "FechaFin" });
    }
}
