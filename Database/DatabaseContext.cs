using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext([NotNull] DbContextOptions options) : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Player> Players { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Server> Servers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
               .HasIndex(u => u.Email)
               .IsUnique();

            modelBuilder.Entity<User>()
               .HasIndex(u => u.Username)
               .IsUnique();

            modelBuilder.Entity<Server>()
               .HasIndex(u => u.Name)
               .IsUnique();

            modelBuilder.Entity<Server>()
               .HasIndex(u => u.Address)
               .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
