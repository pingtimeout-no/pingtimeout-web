using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PingTimeout.Web.Models;

namespace PingTimeout.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Seat> Seats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Event>().HasData(
                new Event {
                    Id = 1,
                    Name = "RomjulsLAN 2018",
                    StartDate = new DateTime(2018, 12, 28, 12, 0, 0, DateTimeKind.Local),
                    EndDate = new DateTime(2018, 12, 30, 15, 0, 0, DateTimeKind.Local)
                }
            );
        }
    }
}
