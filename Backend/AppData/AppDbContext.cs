﻿using Microsoft.EntityFrameworkCore;

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
        Password = 128
    }

    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Entry> Entries { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().ToTable("Users");
            builder.Entity<User>().HasKey(x => x.Id);
            builder.Entity<User>().Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<User>().Property(x => x.Name).IsRequired().IsUnicode().HasMaxLength((int)MaxLength.Name);
            builder.Entity<User>().Property(x => x.Username).IsRequired().IsUnicode().HasMaxLength((int)MaxLength.Username);
            builder.Entity<User>().Property(x => x.Email).IsUnicode().HasMaxLength((int)MaxLength.Email);
            builder.Entity<User>().Property(x => x.About).IsUnicode().HasMaxLength((int)MaxLength.About);
            builder.Entity<User>().Property(x => x.PasswordHash).IsRequired();
            builder.Entity<User>().HasMany(x => x.Entries).WithOne();

            builder.Entity<Entry>().ToTable("Entries");
            builder.Entity<Entry>().HasKey(x => x.Id);
            builder.Entity<Entry>().Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<Entry>().Property(x => x.AuthorId).IsRequired();
            builder.Entity<Entry>().Property(x => x.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Entry>().Property(x => x.Text).IsRequired().IsUnicode().HasMaxLength((int)MaxLength.Entry);
            builder.Entity<Entry>().HasOne(x => x.Author).WithMany(x => x.Entries).HasForeignKey(x => x.AuthorId);
        }
    }
}
