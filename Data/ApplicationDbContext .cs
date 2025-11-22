using CommentService.Api.Models.Comment;
using CommentService.Api.Models.Reaction;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;

namespace CommentService.Api.Data
{
    public class ApplicationDbContext :DbContext
    {
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<Comment> Comments{ get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.HasOne(c => c.ParentComment)
                      .WithMany(c => c.Replies)
                      .HasForeignKey(c => c.ParentCommentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

           

            modelBuilder.Entity<Reaction>(entity =>
            {
                entity.HasOne(cr => cr.Comment)
                      .WithMany(c => c.Reactions)
                      .HasForeignKey(cr => cr.TargetId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


        }
    }
}
