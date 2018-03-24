using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using translate.web.Models;

namespace translate.web.Data
{
    public class ApplContext : IdentityDbContext<AppIdentityUser, AppIdentityRole, Guid>
    {
        public ApplContext(DbContextOptions<ApplContext> options) : base(options)
        { }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<ProjectDocument> ProjectDocuments { get; set; }
        public DbSet<ProjectDocumentDictionary> ProjectDocumentDictionarys { get; set; }
        public DbSet<Translation> Translations { get; set; }
        public DbSet<TranslationDictionary> TranslationDictionarys { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppIdentityUser>().ToTable("Users");
            builder.Entity<AppIdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaim");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRole");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogin");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaim");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserToken");

            builder.Entity<ProjectMember>()
                .HasKey(c => new { c.ProjectId, c.EmployeeId });

            builder.Entity<Translation>()
                .HasOne(d => d.Language)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

        }

    }
}
