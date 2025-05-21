using Microsoft.EntityFrameworkCore;
using ProjetoLabKarts.Models;

namespace ProjetoLabKarts.Data;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SessaoKart> SessoesKart { get; set; }

    public DbSet<AppSettings> AppSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Garante que não gere valor automático para Id
        modelBuilder.Entity<AppSettings>()
                    .HasIndex(x => x.ViewName)
                    .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}
