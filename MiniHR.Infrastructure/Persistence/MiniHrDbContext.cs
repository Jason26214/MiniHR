using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniHR.Domain.Entities;

namespace MiniHR.Infrastructure.Persistence
{
    public class MiniHrDbContext : DbContext
    {
        public MiniHrDbContext(DbContextOptions<MiniHrDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.Salary).HasPrecision(18, 2);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
            });
        }
    }
}
