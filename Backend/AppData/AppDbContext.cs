using Microsoft.EntityFrameworkCore;

using System;

using AspTwitter.Models;


namespace AspTwitter.AppData
{
    public enum MaxLength
    {
        Entry = 256,
        Name = 128,
        Username = 64,
        Email = 128,
        About = 160,
        Password = 128,
        Comment = 128
    }

    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Entry> Entries { get; set; }
        public DbSet<Relationship> Relationships { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Following> Followers { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.UseIdentityAlwaysColumns();

            builder.Entity<User>().ToTable("Users");
            builder.Entity<User>().HasKey(x => x.Id);
            builder.Entity<User>().Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<User>().Property(x => x.Name).IsRequired().IsUnicode().HasMaxLength((int)MaxLength.Name);
            builder.Entity<User>().Property(x => x.Username).IsRequired().IsUnicode().HasMaxLength((int)MaxLength.Username);
            builder.Entity<User>().Property(x => x.Email).IsUnicode().HasMaxLength((int)MaxLength.Email);
            builder.Entity<User>().Property(x => x.About).IsUnicode().HasMaxLength((int)MaxLength.About);
            builder.Entity<User>().Property(x => x.PasswordHash).IsRequired();
            builder.Entity<User>().Property(x => x.DateJoined).IsRequired();
            builder.Entity<User>().Property(x => x.FollowerCount).HasDefaultValue(0);
            builder.Entity<User>().Property(x => x.FollowingCount).HasDefaultValue(0);
            builder.Entity<User>().HasMany(x => x.Entries).WithOne();
            builder.Entity<User>().HasMany(x => x.Relationships).WithOne();
            builder.Entity<User>().HasMany(x => x.Comments).WithOne();
            builder.Entity<User>().HasMany(x => x.Followers).WithOne(x => x.User);
            builder.Entity<User>().HasMany(x => x.Following).WithOne(x => x.Follower);

            builder.Entity<Entry>().ToTable("Entries");
            builder.Entity<Entry>().HasKey(x => x.Id);
            builder.Entity<Entry>().Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Entity<Entry>().Property(x => x.AuthorId).IsRequired();
            builder.Entity<Entry>().Property(x => x.Timestamp).IsRequired();
            builder.Entity<Entry>().Property(x => x.LikeCount).HasDefaultValue(0);
            builder.Entity<Entry>().Property(x => x.RetweetCount).HasDefaultValue(0);
            builder.Entity<Entry>().Property(x => x.CommentCount).HasDefaultValue(0);
            builder.Entity<Entry>().Property(x => x.Text).IsRequired().IsUnicode().HasMaxLength((int)MaxLength.Entry);
            builder.Entity<Entry>().HasOne(x => x.Author).WithMany(x => x.Entries).HasForeignKey(x => x.AuthorId);
            builder.Entity<Entry>().HasMany(x => x.Relationships).WithOne();
            builder.Entity<Entry>().HasMany(x => x.Comments).WithOne();

            builder.Entity<Relationship>().ToTable("Relationships");
            builder.Entity<Relationship>().HasKey(x => x.Id);
            builder.Entity<Relationship>().Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<Relationship>().Property(x => x.Timestamp).IsRequired();
            builder.Entity<Relationship>().Property(x => x.Type).IsRequired();
            builder.Entity<Relationship>().Property(x => x.UserId).IsRequired();
            builder.Entity<Relationship>().Property(x => x.EntryId).IsRequired();
            builder.Entity<Relationship>().HasOne(x => x.User).WithMany(x => x.Relationships).HasForeignKey(x => x.UserId);
            builder.Entity<Relationship>().HasOne(x => x.Entry).WithMany(x => x.Relationships).HasForeignKey(x => x.EntryId);

            builder.Entity<Comment>().ToTable("Comments");
            builder.Entity<Comment>().HasKey(x => x.Id);
            builder.Entity<Comment>().Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<Comment>().Property(x => x.AuthorId).IsRequired();
            builder.Entity<Comment>().Property(x => x.Timestamp).IsRequired();
            builder.Entity<Comment>().Property(x => x.Text).IsRequired().IsUnicode().HasMaxLength((int)MaxLength.Comment);
            builder.Entity<Comment>().HasOne(x => x.Author).WithMany(x => x.Comments).HasForeignKey(x => x.AuthorId);
            builder.Entity<Comment>().HasOne(x => x.Parent).WithMany(x => x.Comments).HasForeignKey(x => x.ParentId);

            builder.Entity<Following>().ToTable("Followers");
            builder.Entity<Following>().HasKey(x => x.Id);
            builder.Entity<Following>().Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<Following>().Property(x => x.UserId).IsRequired();
            builder.Entity<Following>().Property(x => x.FollowerId).IsRequired();
            builder.Entity<Following>().HasOne(x => x.User).WithMany(x => x.Followers).HasForeignKey(x => x.UserId);
            builder.Entity<Following>().HasOne(x => x.Follower).WithMany(x => x.Following).HasForeignKey(x => x.FollowerId);

            builder.Entity<User>().HasData(new User
            {
                Id = 1,
                Name = "Admin",
                Username = "admin",
                PasswordHash = "null",
                DateJoined = DateTime.UtcNow
            });
        }
    }
}
