using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MyEBookLibrary.Models;

namespace MyEBookLibrary.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<UserBook> UserBooks { get; set; } = null!;
        public DbSet<WaitingListItem> WaitingList { get; set; } = null!;
        public DbSet<BookReview> BookReviews { get; set; } = null!;
        public DbSet<ShoppingCart> ShoppingCarts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<UserBook> Borrows { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cart relationships
            modelBuilder.Entity<ShoppingCart>(entity =>
            {
                entity.HasMany(c => c.Items)
                    .WithOne()
                    .HasForeignKey("ShoppingCartId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

           modelBuilder.Entity<UserBook>(entity =>
        {
            entity.ToTable("UserBooks");  // מגדיר את שם הטבלה

            entity.HasOne(ub => ub.User)
                .WithMany(u => u.Books)
                .HasForeignKey(ub => ub.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ub => ub.Book)
                .WithMany(b => b.UserBooks)
                .HasForeignKey(ub => ub.BookId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

            // WaitingList relationships
            modelBuilder.Entity<WaitingListItem>(entity =>
            {
                entity.HasOne(w => w.User)
                    .WithMany(u => u.WaitingListItems)
                    .HasForeignKey(w => w.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(w => w.Book)
                    .WithMany(b => b.WaitingList)
                    .HasForeignKey(w => w.BookId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // BookReview relationships
            modelBuilder.Entity<BookReview>(entity =>
            {
                entity.HasOne(r => r.Reviewer)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.ReviewerId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Book)
                    .WithMany(b => b.Reviews)
                    .HasForeignKey(r => r.BookId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure decimal properties
            modelBuilder.Entity<Book>(entity =>
            {
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Author)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.BuyPrice)
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.BorrowPrice)
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.OriginalPrice)
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.DiscountedPrice)
                    .HasColumnType("decimal(18, 2)");

                entity.HasIndex(e => e.Title);
                entity.HasIndex(e => e.Author);
                entity.HasIndex(e => e.Genre);
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.Property(c => c.Price)
                    .HasColumnType("decimal(18, 2)");

                entity.HasOne(ci => ci.Book)
                    .WithMany()
                    .HasForeignKey(ci => ci.BookId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18, 2)");

            // Configure book formats
            modelBuilder.Entity<Book>()
                .Property(e => e.AvailableFormats)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(format => ParseBookFormat(format))
                          .ToList(),
                    new ValueComparer<List<BookFormat>>(
                        (c1, c2) => (c1 ?? new List<BookFormat>()).SequenceEqual(c2 ?? new List<BookFormat>()),
                        c => c != null ? c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())) : 0,
                        c => c == null ? new List<BookFormat>() : c.ToList()
                    )
                );
        }

        private static BookFormat ParseBookFormat(string format)
        {
            if (Enum.TryParse<BookFormat>(format, out var result))
            {
                return result;
            }
            return BookFormat.PDF;
        }
    }
}