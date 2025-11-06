using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using SDSM.AuthenticationApi.Models;

namespace SDSM.AuthenticationApi.Contexts
{
    public class MyDbContext: DbContext
    {
        public MyDbContext(DbContextOptions opt):base(opt)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("Authentication_IdentityUserRoles");
            modelBuilder.Entity<IdentityUserRole<Guid>>().HasKey(p => new {p.UserId,p.RoleId });
            modelBuilder.Entity<User>().ToTable("Authentication_Users");
            modelBuilder.Entity<Role>().ToTable("Authentication_Roles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("Authentication_IdentityUserClaims");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("Authentication_IdentityRoleClaim");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("Authentication_IdentityUserLogin");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().HasKey(p => new { p.UserId });
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("Authentication_IdentityUserToken");
            modelBuilder.Entity<IdentityUserToken<Guid>>().HasKey(p => new { p.UserId});

            //modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("IdentityUserRoles", "Authentication");
            //modelBuilder.Entity<IdentityUserRole<Guid>>().HasKey(p => new { p.UserId, p.RoleId });
            //modelBuilder.Entity<User>().ToTable("Users", "Authentication");
            //modelBuilder.Entity<Role>().ToTable("Roles", "Authentication");
            //modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("IdentityUserClaims", "Authentication");
            //modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("IdentityRoleClaim", "Authentication");
            //modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("IdentityUserLogin", "Authentication");
            //modelBuilder.Entity<IdentityUserLogin<Guid>>().HasKey(p => new { p.UserId });
            //modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("IdentityUserToken", "Authentication");
            //modelBuilder.Entity<IdentityUserToken<Guid>>().HasKey(p => new { p.UserId });
        }
        //public DbSet<User> UserSet { get; set; }
        //public DbSet<Role> RoleSet { get; set; }
        //public DbSet<IdentityUserClaim<Guid>> UserClaimSet { get; set; }
        //public DbSet<IdentityUserRole<Guid>> UserRoleSet { get; set; }
    }
}
