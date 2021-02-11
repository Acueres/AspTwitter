using Microsoft.EntityFrameworkCore;

using AspTwitter.Models;


namespace AspTwitter.AppData
{
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
            builder.Entity<User>().Property(x => x.Name).IsRequired().IsUnicode().HasMaxLength(128);
            builder.Entity<User>().Property(x => x.Username).IsRequired().IsUnicode().HasMaxLength(64);
            builder.Entity<User>().Property(x => x.Email).IsUnicode().HasMaxLength(128);
            builder.Entity<User>().Property(x => x.PasswordHash).IsRequired().HasMaxLength(128);
            builder.Entity<User>().HasMany(x => x.Entries).WithOne();

            builder.Entity<User>().HasData
            (
                new User
                {
                    Id = 1,
                    Name = "Donald Trump",
                    Username = "donaldTrump",
                    Email = "realDonaldTrump@loser.com",
                    PasswordHash = "covfefe"
                }
            );

            builder.Entity<Entry>().ToTable("Entries");
            builder.Entity<Entry>().HasKey(x => x.Id);
            builder.Entity<Entry>().Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<Entry>().Property(x => x.AuthorId).IsRequired();
            builder.Entity<Entry>().Property(x => x.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Entry>().Property(x => x.Text).IsRequired().IsUnicode().HasMaxLength(256);
            builder.Entity<Entry>().HasOne(x => x.Author).WithMany(x => x.Entries).
                                    HasForeignKey(x => x.AuthorId);

            builder.Entity<Entry>().HasData
            (
                new Entry
                {
                    Id = 1,
                    AuthorId = 1,
                    Text = "Example1"
                },

                new Entry
                {
                    Id = 2,
                    AuthorId = 1,
                    Text = "Example2"
                }
            );
        }
    }
}
