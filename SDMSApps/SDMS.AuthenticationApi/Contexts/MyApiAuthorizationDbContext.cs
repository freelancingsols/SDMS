using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Extensions;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SDMS.AuthenticationApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDMS.AuthenticationApi.Contexts
{
    public class MyApiAuthorizationDbContext : IdentityDbContext<User,Role,Guid>//, IPersistedGrantDbContext
    {
        private readonly IOptions<OperationalStoreOptions> _operationalStoreOptions;
        public MyApiAuthorizationDbContext(
            DbContextOptions options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options)
        {
            _operationalStoreOptions = operationalStoreOptions;
        }
        /// <summary>
        /// Gets or sets the <see cref="DbSet{PersistedGrant}"/>.
        /// </summary>
        public DbSet<PersistedGrant> PersistedGrants { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{DeviceFlowCodes}"/>.
        /// </summary>
        public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }

        //Task<int> IPersistedGrantDbContext.SaveChangesAsync() => base.SaveChangesAsync();

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ConfigurePersistedGrantContext(_operationalStoreOptions.Value);
            builder.Entity<IdentityUserRole<Guid>>().ToTable("Authentication_IdentityUserRoles");
            builder.Entity<IdentityUserRole<Guid>>().HasKey(p => new { p.UserId, p.RoleId });
            builder.Entity<User>().ToTable("Authentication_Users");
            builder.Entity<Role>().ToTable("Authentication_Roles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("Authentication_IdentityUserClaims");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("Authentication_IdentityRoleClaim");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("Authentication_IdentityUserLogin");
            builder.Entity<IdentityUserLogin<Guid>>().HasKey(p => new { p.UserId });
            builder.Entity<IdentityUserToken<Guid>>().ToTable("Authentication_IdentityUserToken");
            builder.Entity<IdentityUserToken<Guid>>().HasKey(p => new { p.UserId });

        }
    }
}
