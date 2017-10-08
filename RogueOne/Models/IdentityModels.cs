﻿using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RogueOne.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string Avatar { get; set; }
        public virtual List<LocationEntry> Diary { get; set; }
        public virtual List<Request> ConnectRequests { get; set; }
        public virtual List<Request> PendingRequests { get; set; }
        public virtual List<ApplicationUser> Friends { get; set; }
        public virtual LocationEntry emergencyCheckIn { get; set; }
        public virtual List<Trip> Trips { get; set; }
        [ForeignKey("UserSettings")]
        public long SettingsID { get; set; }
        public virtual Settings UserSettings { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        
        public ApplicationDbContext()
            : base("RogueOne", throwIfV1Schema: false)
        {

        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Friends)
                .WithMany()
                .Map(m =>
                {
                    m.MapLeftKey("FriendID");
                    m.MapRightKey("RelatedID");
                    m.ToTable("Friend_related");
                });
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<LocationEntry> LocationEntries { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<CheckIn> CheckIns { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripMate> TripMates { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}