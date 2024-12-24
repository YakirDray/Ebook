// Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MyEBookLibrary.Models;

namespace MyEBookLibrary.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<User, IdentityRole<int>, int>(options)
    {
        public required DbSet<Book> Books { get; set; }
        public required DbSet<UserBook> UserBooks { get; set; }
        public required DbSet<WaitingListItem> WaitingList { get; set; }
        public required DbSet<BookReview> BookReviews { get; set; }
        public required DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public required DbSet<Borrow> Borrows { get; set; }

        public ApplicationDbContext() : this(new DbContextOptions<ApplicationDbContext>())
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // הגדרת יחסים ומפתחות זרים
            ConfigureRelationships(modelBuilder);

            // הגדרות עבור תכונות decimal
            ConfigureDecimalProperties(modelBuilder);

            // המרת רשימת פורמטים לטקסט
            ConfigureBookFormats(modelBuilder);
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // UserBook relationships
            modelBuilder.Entity<UserBook>()
                .HasOne(ub => ub.User)
                .WithMany(u => u.Books)
                .HasForeignKey(ub => ub.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserBook>()
                .HasOne(ub => ub.Book)
                .WithMany(b => b.UserBooks)
                .HasForeignKey(ub => ub.BookId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // WaitingList relationships
            modelBuilder.Entity<WaitingListItem>()
                .HasOne(w => w.User)
                .WithMany(u => u.WaitingListItems)
                .HasForeignKey(w => w.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WaitingListItem>()
                .HasOne(w => w.Book)
                .WithMany(static b => b.WaitingList)
                .HasForeignKey(w => w.BookId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // BookReview relationships
            modelBuilder.Entity<BookReview>()
                .HasOne(r => r.Reviewer)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.ReviewerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureDecimalProperties(ModelBuilder modelBuilder)
        {
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

            modelBuilder.Entity<CartItem>()
                .Property(c => c.Price)
                .HasColumnType("decimal(18, 2)");


            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18, 2)");
        }

        private void ConfigureBookFormats(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .Property(e => e.AvailableFormats)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(format => ParseBookFormat(format))  // קריאה לפונקציה חיצונית
                          .ToList(),
                    new ValueComparer<List<BookFormat>>(
                        (c1, c2) => (c1 ?? new List<BookFormat>()).SequenceEqual(c2 ?? new List<BookFormat>()),
                        c => c != null ? c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())) : 0,
                        c => c == null ? new List<BookFormat>() : c.ToList()
                    )
                );
        }

        // פונקציה לעקיפת השימוש ב-out ו-?.
        private static BookFormat ParseBookFormat(string format)
        {
            if (Enum.TryParse<BookFormat>(format, out var result))
            {
                return result;
            }
            return BookFormat.PDF;  // ברירת מחדל במקרה של ערך לא חוקי
        }

    }


}
