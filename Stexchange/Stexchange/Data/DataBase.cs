using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stexchange.Data.Models;

namespace Stexchange.Data
{
	public class Database : DbContext
	{
		private IConfiguration Config { get; }

		public Database(IConfiguration config) : base()
		{
			Config = config.GetSection("DbSettings");
		}

		protected override void OnConfiguring(DbContextOptionsBuilder options)
		{
			options.UseMySQL(new MySqlConnectionStringBuilder()
			{
				Server = Config["Server"],
				Port = Config.GetValue<uint>("Port"),
				Database = Config["Database"],
				UserID = Config["Username"],
				Password = Config["Password"]
			}.ConnectionString);
			options.EnableSensitiveDataLogging();
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<UserVerification>(entity =>
			{
				// Put a unique constraint on the Guid column
				entity.HasIndex(uv => uv.Guid)
					.IsUnique();
			});

			modelBuilder.Entity<UserVerification>()
				.HasOne(uv => uv.User)
				.WithOne(u => u.Verification)
				.HasForeignKey<UserVerification>(uv => uv.Id)
				.HasPrincipalKey<User>(u => u.Id);

			modelBuilder.Entity<User>(entity =>
			{
				// Put a unique constraint on the Email column
				entity.HasAlternateKey(u => u.Email);
				// Put a unique constraint on the Username column
				entity.HasAlternateKey(u => u.Username);

				entity.Property(u => u.Postal_Code).IsRequired();
				entity.Property(u => u.Password).IsRequired();
				entity.Property(u => u.IsVerified).HasDefaultValue(0);
			});

			modelBuilder.Entity<Listing>(entity =>
            {
				entity.Property(l => l.Title).IsRequired();
				entity.Property(l => l.Description).IsRequired();
				entity.Property(l => l.NameNl).IsRequired();
				entity.Property(l => l.Visible).HasDefaultValue(1);
				entity.Property(l => l.Renewed).HasDefaultValue(0);
			});

			modelBuilder.Entity<Listing>()
				.HasOne(l => l.Owner)
				.WithMany()
				.HasForeignKey(l => l.UserId)
				.HasPrincipalKey(u => u.Id);

			modelBuilder.Entity<ImageData>()
				.HasOne(id => id.Listing)
				.WithMany(l => l.Pictures)
				.HasForeignKey(id => id.ListingId)
				.HasPrincipalKey(l => l.Id);

			modelBuilder.Entity<FilterListing>()
				.HasKey(fl => new { fl.ListingId, fl.Value });

			modelBuilder.Entity<FilterListing>()
				.HasOne(fl => fl.Listing)
				.WithMany()
				.HasForeignKey(fl => fl.ListingId)
				.HasPrincipalKey(l => l.Id);

			modelBuilder.Entity<FilterListing>()
				.HasOne(fl => fl.Filter)
				.WithMany()
				.HasForeignKey(fl => fl.Value)
				.HasPrincipalKey(f => f.Value);

			modelBuilder.Entity<Chat>()
				.HasAlternateKey(c => new { c.AdId, c.ResponderId });

			modelBuilder.Entity<Chat>()
				.HasOne(c => c.Listing)
				.WithMany()
				.HasForeignKey(c => c.AdId)
				.HasPrincipalKey(l => l.Id);

			modelBuilder.Entity<Chat>()
				.HasOne(c => c.Responder)
				.WithMany()
				.HasForeignKey(c => c.ResponderId)
				.HasPrincipalKey(u => u.Id);

			modelBuilder.Entity<Chat>()
				.HasMany(c => c.Messages)
				.WithOne()
				.HasForeignKey(m => m.ChatId)
				.HasPrincipalKey(c => c.Id);

			modelBuilder.Entity<Message>(entity =>
			{
				entity.Property(m => m.Content).IsRequired();
			});

			modelBuilder.Entity<Message>()
				.HasOne(m => m.Sender)
				.WithMany()
				.HasForeignKey(m => m.SenderId)
				.HasPrincipalKey(u => u.Id);

			modelBuilder.Entity<Block>()
				.HasKey(b => new { b.BlockedId, b.BlockerId });

			modelBuilder.Entity<Block>()
				.HasOne(b => b.Blocked)
				.WithMany()
				.HasForeignKey(b => b.BlockedId)
				.HasPrincipalKey(u => u.Id);

			modelBuilder.Entity<Block>()
				.HasOne(b => b.Blocker)
				.WithMany()
				.HasForeignKey(b => b.BlockerId)
				.HasPrincipalKey(u => u.Id);

			modelBuilder.Entity<Rating>(entity =>
			{
				entity.Property(r => r.Communication).IsRequired();
				entity.Property(r => r.ReviewerId).IsRequired();
				entity.Property(r => r.RevieweeId).IsRequired();
			});

			modelBuilder.Entity<Rating>()
				.HasOne(r => r.Reviewer)
				.WithMany()
				.HasForeignKey(r => r.ReviewerId)
				.HasPrincipalKey(u => u.Id);

			modelBuilder.Entity<Rating>()
				.HasOne(r => r.Reviewee)
				.WithMany()
				.HasForeignKey(r => r.RevieweeId)
				.HasPrincipalKey(u => u.Id);

			modelBuilder.Entity<RatingRequest>(entity =>
			{
				entity.Property(r => r.ReviewerId).IsRequired();
				entity.Property(r => r.RevieweeId).IsRequired();
			});

			modelBuilder.Entity<RatingRequest>()
				.HasOne(r => r.Reviewer)
				.WithMany()
				.HasForeignKey(r => r.ReviewerId)
				.HasPrincipalKey(u => u.Id);

			modelBuilder.Entity<RatingRequest>()
				.HasOne(r => r.Reviewee)
				.WithMany()
				.HasForeignKey(r => r.RevieweeId)
				.HasPrincipalKey(u => u.Id);
		}

		public DbSet<User> Users { get; set; }
		public DbSet<UserVerification> UserVerifications { get; set; }
		public DbSet<Listing> Listings { get; set; }
		public DbSet<ImageData> Images { get; set; }
		public DbSet<Filter> Filters { get; set; }
		public DbSet<FilterListing> FilterListings { get; set; }
		public DbSet<Chat> Chats { get; set; }
		public DbSet<Message> Messages { get; set; }
		public DbSet<Block> Blocks { get; set; }
		public DbSet<Rating> Ratings { get; set; }
		public DbSet<RatingRequest> RatingRequests { get; set; }
	}
}
