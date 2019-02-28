using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TournamentPage.Models;

namespace TournamentPage.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
        public DbSet<RegisterType> RegisterType {get;set;}
        public DbSet<SportType> SportType {get;set;}
        public DbSet<Team> Team {get;set;}
        public DbSet<TeamUser> TeamUser {get;set;}
        public DbSet<Tournament> Tournament {get;set;}
        public DbSet<TournamentTeam> TournamentTeam {get;set;}
        public DbSet<TournamentUser> TournamentUser {get;set;}
        public DbSet<TournamentType> TournamentType {get;set;}
        public DbSet<GenderType> GenderType {get;set;}
        public DbSet<Matchups> Matchups {get;set;}
        public DbSet<Notifications> Notifications {get;set;}
        public DbSet<InvitationsReserved> InvitationsReserved {get;set;}
        

    }
}
