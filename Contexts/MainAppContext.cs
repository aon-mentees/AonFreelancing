﻿using AonFreelancing.Models;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using static System.Net.WebRequestMethods;

namespace AonFreelancing.Contexts
{
    public class MainAppContext(DbContextOptions<MainAppContext> contextOptions) 
        : IdentityDbContext<User, ApplicationRole, long>(contextOptions)
    {

        public DbSet<Project> Projects { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Otp> Otps { get; set; }
        public DbSet<TempUser> TempUsers { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<TaskEntity> Tasks { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<ProjectLike> ProjectLikes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Rating> Ratings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            
            // For TPT design
            builder.Entity<User>().ToTable("AspNetUsers")
                .HasIndex(u=>u.PhoneNumber).IsUnique();
            builder.Entity<TempUser>().ToTable("TempUser")
                .HasIndex(u=>u.PhoneNumber).IsUnique();
            

            builder.Entity<Freelancer>().ToTable("Freelancers");
            builder.Entity<Client>().ToTable("Clients");
            builder.Entity<SystemUser>().ToTable("SystemUsers");
            builder.Entity<Otp>().ToTable("otps", o => o.HasCheckConstraint("CK_CODE","LEN([Code]) = 6"));

            builder.Entity<Notification>().ToTable("Notifications");
            builder.Entity<LikeNotification>().ToTable("LikeNotifications");
            builder.Entity<SubmitBidNotification>().ToTable("SubmitBidNotifications");



            builder.Entity<Project>().ToTable("Projects", tb => tb.HasCheckConstraint("CK_PRICE_TYPE", $"[PriceType] IN ('{Constants.PROJECT_PRICETYPE_FIXED}', '{Constants.PROJECT_PRICETYPE_PERHOUR}')"));
            builder.Entity<Project>().ToTable("Projects", tb => tb.HasCheckConstraint("CK_QUALIFICATION_NAME", $"[QualificationName] IN ('{Constants.PROJECT_QUALIFICATION_UIUX}', '{Constants.PROJECT_QUALIFICATION_FRONTEND}', '{Constants.PROJECT_QUALIFICATION_MOBILE}', '{Constants.PROJECT_QUALIFICATION_BACKEND}', '{Constants.PROJECT_QUALIFICATION_FULLSTACK}')"));
            builder.Entity<Project>().ToTable("Projects", tb => tb.HasCheckConstraint("CK_PROJECT_STATUS", $"[Status] IN ('{Constants.PROJECT_STATUS_AVAILABLE}', '{Constants.PROJECT_STATUS_CLOSED}')"))
                .Property(p=>p.Status).HasDefaultValue(Constants.PROJECT_STATUS_AVAILABLE);

            builder.Entity<TaskEntity>().ToTable("Tasks", t => t.HasCheckConstraint("CK_TASK_STATUS", $"[Status] IN ('{Constants.TASK_STATUS_DONE}', '{Constants.TASK_STATUS_IN_REVIEW}', '{Constants.TASK_STATUS_IN_PROGRESS}', '{Constants.TASK_STATUS_TO_DO}')"))
              .Property(t => t.Status).HasDefaultValue(Constants.TASK_STATUS_TO_DO);

            builder.Entity<ProjectLike>().HasIndex(pl => new { pl.ProjectId, pl.LikerId }).IsUnique();

            builder.Entity<Rating>().HasIndex(r => r.RaterUserId).IsUnique(false);
            builder.Entity<Rating>().HasIndex(r => r.RatedUserId).IsUnique(false);

            //set up relationships
            builder.Entity<TempUser>().HasOne<Otp>()
                                    .WithOne()
                                    .HasForeignKey<Otp>()
                                    .HasPrincipalKey<TempUser>(nameof(TempUser.PhoneNumber));

            builder.Entity<Bid>()
               .HasOne(b => b.Project)
               .WithMany(p => p.Bids)
               .HasForeignKey(b => b.ProjectId)
               .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Bid>()
                .HasOne(b => b.Freelancer)
                .WithMany(f => f.Bids)
                .HasForeignKey(b => b.FreelancerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Skill>().HasOne<Freelancer>()
                                    .WithMany(f=>f.Skills)
                                    .HasForeignKey(s=>s.UserId)
                                    .HasPrincipalKey(f=>f.Id);

            builder.Entity<ProjectLike>().HasOne<User>()
                                          .WithMany()
                                          .HasForeignKey(pl => pl.LikerId)
                                          .HasPrincipalKey(u => u.Id);
            builder.Entity<ProjectLike>().HasOne<Project>()
                                          .WithMany(p => p.ProjectLikes)
                                          .HasForeignKey(pl => pl.ProjectId)
                                          .OnDelete(DeleteBehavior.NoAction)
                                          .HasPrincipalKey(p => p.Id);
            builder.Entity<Notification>().HasOne<User>()
                                               .WithMany()
                                               .HasForeignKey(n=>n.ReceiverId)
                                               .HasPrincipalKey (u => u.Id);  
            builder.Entity<LikeNotification>().HasOne<Project>()
                                               .WithMany()
                                               .HasForeignKey(ln=>ln.ProjectId)
                                               .HasPrincipalKey (p => p.Id)
                                               .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<LikeNotification>().HasOne<User>()
                                                       .WithMany()
                                                       .HasForeignKey(ln => ln.LikerId)
                                                       .HasPrincipalKey(u => u.Id)
                                                       .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<SubmitBidNotification>().HasOne<Project>()
                                               .WithMany()
                                               .HasForeignKey(sn => sn.ProjectId)
                                               .HasPrincipalKey(p => p.Id)
                                               .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<SubmitBidNotification>().HasOne<Freelancer>()
                                                       .WithMany()
                                                       .HasForeignKey(sn => sn.FreelancerId)
                                                       .HasPrincipalKey(u => u.Id)
                                                       .OnDelete(DeleteBehavior.NoAction);



            builder.Entity<Rating>()
                  .HasOne<User>()
                  .WithMany()
                  .HasForeignKey(r => r.RatedUserId)
                  .HasPrincipalKey(u => u.Id)
                  .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Rating>()
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey(r => r.RaterUserId)
                    .HasPrincipalKey(u => u.Id)
                    .OnDelete(DeleteBehavior.NoAction);

            base.OnModelCreating(builder);
        }
    }
}
