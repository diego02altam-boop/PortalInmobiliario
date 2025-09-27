using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PortalInmobiliario.Models;

public class Reserva
{
    public int Id { get; set; }

    public int InmuebleId { get; set; }
    public Inmueble Inmueble { get; set; } = default!;

    public string UsuarioId { get; set; } = default!;
    public IdentityUser Usuario { get; set; } = default!;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaExpiracion { get; set; }

    public bool Activa => DateTime.UtcNow < FechaExpiracion;
}
