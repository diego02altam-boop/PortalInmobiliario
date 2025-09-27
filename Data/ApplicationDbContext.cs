using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Inmueble> Inmuebles => Set<Inmueble>();
    public DbSet<Visita> Visitas => Set<Visita>();
    public DbSet<Reserva> Reservas => Set<Reserva>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Inmueble>()
            .HasIndex(i => i.Codigo).IsUnique();

        b.Entity<Inmueble>()
            .HasCheckConstraint("CK_Inmueble_Precio", "Precio > 0")
            .HasCheckConstraint("CK_Inmueble_M2", "MetrosCuadrados > 0");

        b.Entity<Visita>()
            .HasCheckConstraint("CK_Visita_Rango", "FechaInicio < FechaFin");
            
             b.Entity<Inmueble>()
        .Property(i => i.Precio)
        .HasConversion<double>();
    }
    
}
