using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyEBookLibrary.Models;

namespace MyEBookLibrary.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context,
            UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            context.Database.EnsureCreated();

            // יצירת תפקידים אם לא קיימים
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }

            // יצירת משתמש מנהל אם לא קיים
            var adminEmail = "admin@ebooklibrary.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new User
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // זריעת ספרים
            await SeedBooksAsync(context);
        }

        private static async Task SeedBooksAsync(ApplicationDbContext context)
        {
            var books = GetDefaultBooks();
            int addedBooks = 0;
            int existingBooks = 0;

            foreach (var book in books)
            {
                var existingBook = await context.Books
                    .FirstOrDefaultAsync(b => b.Title == book.Title);

                if (existingBook == null)
                {
                    context.Books.Add(book);
                    Console.WriteLine($"Adding new book: {book.Title}");
                    addedBooks++;
                }
                else
                {
                    Console.WriteLine($"Book already exists: {book.Title}");
                    existingBooks++;
                }
            }

            if (addedBooks > 0)
            {
                await context.SaveChangesAsync();
                Console.WriteLine($"\nSummary:");
                Console.WriteLine($"Added {addedBooks} new books");
                Console.WriteLine($"Found {existingBooks} existing books");
            }
            else
            {
                Console.WriteLine("No new books needed to be added");
            }
        }

        private static List<Book> GetDefaultBooks()
        {
            return new List<Book>
            {
            new Book
            {
                Title = "הארי פוטר ואבן החכמים",
                Author = "ג'יי. קיי. רולינג",
                Genre = "פנטזיה",
                Description = "הספר הראשון בסדרת הארי פוטר, המספר את סיפורו של נער צעיר המגלה שהוא קוסם ומתחיל את לימודיו בבית הספר הוגוורטס לכישוף ולקוסמות.",
                Publisher = "ידיעות ספרים",
                YearOfPublication = 1997,
                BuyPrice = 49.90m,
                BorrowPrice = 9.90m,
                IsBorrowable = true,
                AgeRestriction = "8+",
                AvailableCopies = 3,
                AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB },
                CoverImageUrl = "/images/books/healthy-cooking.jpg"
            },
                    new Book
                    {
                        Title = "בישול בריא לכל המשפחה",
                        Author = "שף רותם ליברמן",
                        Genre = "בישול",
                        Description = "ספר מתכונים בריאים וקלים להכנה, מתאים לכל המשפחה.",
                        Publisher = "פן",
                        YearOfPublication = 2024,
                        BuyPrice = 149.90m,
                        BorrowPrice = 24.90m,
                        IsBorrowable = true,
                        AgeRestriction = "8+",
                        AvailableCopies = 3,
                        AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB },
                        CoverImageUrl = "/images/books/healthy-cooking.jpg"
                    },
                    new Book
                    {
                        Title = "מבוא לפסיכולוגיה",
                        Author = "פרופ' דן ענבר",
                        Genre = "פסיכולוגיה",
                        Description = "ספר יסוד בפסיכולוגיה מודרנית, מתאים לסטודנטים ולמתעניינים.",
                        Publisher = "האוניברסיטה הפתוחה",
                        YearOfPublication = 2022,
                        BuyPrice = 159.90m,
                        BorrowPrice = 29.90m,
                        IsBorrowable = true,
                        AgeRestriction = "18+",
                        AvailableCopies = 3,
                        AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB },
                        CoverImageUrl = "/images/books/psychology.jpg"
                    },
                    new Book
                    {
                        Title = "סיפורים קצרים מהעולם",
                        Author = "מחברים שונים",
                        Genre = "סיפורים קצרים",
                        Description = "אסופה של סיפורים קצרים נבחרים מרחבי העולם.",
                        Publisher = "כרמל",
                        YearOfPublication = 2023,
                        BuyPrice = 69.90m,
                        BorrowPrice = 12.90m,
                        IsBorrowable = true,
                        AgeRestriction = "12+",
                        AvailableCopies = 3,
                        AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB, BookFormat.MOBI },
                        CoverImageUrl = "/images/books/short-stories.jpg"
                    },
                    new Book
                    {
                        Title = "שירת הים",
                        Author = "רחל המשוררת",
                        Genre = "שירה",
                        Description = "אסופת שירים על הים, האהבה והטבע.",
                        Publisher = "הקיבוץ המאוחד",
                        YearOfPublication = 2020,
                        BuyPrice = 78.90m,
                        BorrowPrice = 14.90m,
                        IsBorrowable = true,
                        AgeRestriction = "12+",
                        AvailableCopies = 3,
                        AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB },
                        CoverImageUrl = "/images/books/sea-poetry.jpg"
                    },
                    new Book
                    {
                        Title = "יסודות הכלכלה",
                        Author = "פרופ' אבי ישראלי",
                        Genre = "כלכלה",
                        Description = "ספר יסוד בכלכלה המסביר מושגים בסיסיים ומתקדמים.",
                        Publisher = "אקדמון",
                        YearOfPublication = 2024,
                        BuyPrice = 169.90m,
                        BorrowPrice = 0m,
                        IsBorrowable = false,
                        AgeRestriction = "16+",
                        AvailableCopies = 0,
                        AvailableFormats = new List<BookFormat> { BookFormat.PDF },
                        CoverImageUrl = "/images/books/economics.jpg"
                    },
                    new Book
                    {
                        Title = "הרפתקאות בגינה",
                        Author = "תמר גרין",
                        Genre = "ילדים",
                        Description = "ספר ילדים מקסים על הרפתקאות בגינה הקסומה.",
                        Publisher = "כנרת",
                        YearOfPublication = 2024,
                        BuyPrice = 54.90m,
                        BorrowPrice = 9.90m,
                        IsBorrowable = true,
                        AgeRestriction = "4+",
                        AvailableCopies = 3,
                        AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB },
                        CoverImageUrl = "/images/books/garden-adventures.jpg"
                    },
                    new Book
                    {
                        Title = "אמנות הצילום",
                        Author = "יוסי רוט",
                        Genre = "אמנות",
                        Description = "מדריך מקיף לצילום דיגיטלי מקצועי.",
                        Publisher = "פוקוס",
                        YearOfPublication = 2023,
                        BuyPrice = 199.90m,
                        BorrowPrice = 0m,
                        IsBorrowable = false,
                        AgeRestriction = "14+",
                        AvailableCopies = 0,
                        AvailableFormats = new List<BookFormat> { BookFormat.PDF },
                        CoverImageUrl = "/images/books/photography.jpg"
                    },
                    new Book
                    {
                        Title = "מדיטציה למתחילים",
                        Author = "מיכל שלום",
                        Genre = "סגנון חיים",
                        Description = "מדריך מעשי למתחילים בתרגול מדיטציה.",
                        Publisher = "פראג",
                        YearOfPublication = 2024,
                        BuyPrice = 89.90m,
                        BorrowPrice = 16.90m,
                        IsBorrowable = true,
                        AgeRestriction = "12+",
                        AvailableCopies = 3,
                        AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB },
                        CoverImageUrl = "/images/books/meditation.jpg"
                    },
                    new Book
                    {
                        Title = "מסע בזמן: תולדות המדע",
                        Author = "ד\"ר שרה לוי",
                        Genre = "מדע",
                        Description = "סקירה מקיפה של התפתחות המדע לאורך ההיסטוריה.",
                        Publisher = "מטר",
                        YearOfPublication = 2023,
                        BuyPrice = 145.90m,
                        BorrowPrice = 24.90m,
                        IsBorrowable = true,
                        AgeRestriction = "14+",
                        AvailableCopies = 3,
                        AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB },
                        CoverImageUrl = "/images/books/science-history.jpg"
                    },
                        new Book
        {
            Title = "1984",
            Author = "ג'ורג' אורוול",
            Genre = "מדע בדיוני",
            Description = "רומן דיסטופי המתאר חברה טוטליטרית שבה הממשלה שולטת במחשבות ובמעשי אזרחיה.",
            Publisher = "עם עובד",
            YearOfPublication = 1949,
            BuyPrice = 39.90m,
            BorrowPrice = 7.90m,
            IsBorrowable = true,
            AgeRestriction = "16+",
            AvailableCopies = 3,
            AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB },
            CoverImageUrl = "/images/books/1984.jpg"
        },
        new Book
        {
            Title = "שר הטבעות: אחוות הטבעת",
            Author = "ג'.ר.ר. טולקין",
            Genre = "פנטזיה",
            Description = "החלק הראשון בטרילוגית שר הטבעות, המספר על מסעו של פרודו להשמיד את טבעת הכוח.",
            Publisher = "זמורה ביתן",
            YearOfPublication = 1954,
            BuyPrice = 54.90m,
            BorrowPrice = 10.90m,
            IsBorrowable = true,
            AgeRestriction = "12+",
            AvailableCopies = 3,
            AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB, BookFormat.MOBI },
            CoverImageUrl = "/images/books/lotr-1.jpg"
        },
        new Book
        {
            Title = "מאה שנים של בדידות",
            Author = "גבריאל גרסיה מארקס",
            Genre = "ספרות יפה",
            Description = "סאגה משפחתית המשלבת ריאליזם מאגי, המספרת את סיפורה של משפחת בואנדיה לאורך שבעה דורות.",
            Publisher = "כנרת",
            YearOfPublication = 1967,
            BuyPrice = 44.90m,
            BorrowPrice = 8.90m,
            IsBorrowable = true,
            AgeRestriction = "16+",
            AvailableCopies = 3,
            AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB },
            CoverImageUrl = "/images/books/hundred-years.jpg"
        },
        new Book
        {
            Title = "רומן רוסי",
            Author = "מאיר שלו",
            Genre = "ספרות עברית",
            Description = "רומן ישראלי על משפחה וזכרונות, המשלב הומור ודרמה.",
            Publisher = "עם עובד",
            YearOfPublication = 1988,
            BuyPrice = 42.90m,
            BorrowPrice = 8.90m,
            IsBorrowable = true,
            AgeRestriction = "12+",
            AvailableCopies = 3,
            AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB },
            CoverImageUrl = "/images/books/russian-novel.jpg"
        },
        new Book
        {
            Title = "המשחק של אנדר",
            Author = "אורסון סקוט קארד",
            Genre = "מדע בדיוני",
            Description = "סיפור על ילד מחונן המתאמן בבית ספר צבאי חללי למלחמה בחייזרים.",
            Publisher = "אופוס",
            YearOfPublication = 1985,
            BuyPrice = 49.90m,
            BorrowPrice = 9.90m,
            IsBorrowable = true,
            AgeRestriction = "12+",
            AvailableCopies = 3,
            AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB },
            CoverImageUrl = "/images/books/enders-game.jpg"
        },
        new Book
        {
            Title = "קיצור תולדות האנושות",
            Author = "יובל נח הררי",
            Genre = "היסטוריה",
            Description = "מסע מרתק דרך ההיסטוריה האנושית, מראשית האדם ועד ימינו.",
            Publisher = "דביר",
            YearOfPublication = 2011,
            BuyPrice = 98.00m,
            BorrowPrice = 0m,
            IsBorrowable = false,
            AgeRestriction = "16+",
            AvailableCopies = 0,
            AvailableFormats = new List<BookFormat> { BookFormat.PDF },
            CoverImageUrl = "/images/books/sapiens.jpg"
        },
        new Book
        {
            Title = "הצל של הרוח",
            Author = "קרלוס רואיס סאפון",
            Genre = "מתח",
            Description = "מותחן גותי המתרחש בברצלונה של שנות הארבעים.",
            Publisher = "כנרת",
            YearOfPublication = 2001,
            BuyPrice = 49.90m,
            BorrowPrice = 9.90m,
            IsBorrowable = true,
            AgeRestriction = "16+",
            AvailableCopies = 3,
            AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB },
            CoverImageUrl = "/images/books/shadow.jpg"
        },
        new Book
        {
            Title = "מלחמה ושלום",
            Author = "לב טולסטוי",
            Genre = "ספרות קלאסית",
            Description = "אפוס היסטורי המתאר את החברה הרוסית בתקופת מלחמות נפוליאון.",
            Publisher = "הקיבוץ המאוחד",
            YearOfPublication = 1869,
            BuyPrice = 89.90m,
            BorrowPrice = 15.90m,
            IsBorrowable = true,
            AgeRestriction = "16+",
            AvailableCopies = 3,
            AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB, BookFormat.MOBI },
            CoverImageUrl = "/images/books/war-peace.jpg"
        },
        new Book
        {
            Title = "אמנות המלחמה",
            Author = "סון דזה",
            Genre = "פילוסופיה",
            Description = "ספר אסטרטגיה צבאית עתיק המשמש עד היום כמדריך למנהיגות ואסטרטגיה.",
            Publisher = "מטר",
            YearOfPublication = 2020,
            BuyPrice = 79.90m,
            BorrowPrice = 0m,
            IsBorrowable = false,
            AgeRestriction = "16+",
            AvailableCopies = 0,
            AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB },
            CoverImageUrl = "/images/books/art-of-war.jpg"
        },
        new Book
        {
            Title = "הנסיך הקטן",
            Author = "אנטואן דה סנט-אכזופרי",
            Genre = "ילדים",
            Description = "סיפור פילוסופי על נסיך קטן המבקר בכוכבים שונים ולומד על אהבה וחברות.",
            Publisher = "כנרת",
            YearOfPublication = 1943,
            BuyPrice = 34.90m,
            BorrowPrice = 6.90m,
            IsBorrowable = true,
            AgeRestriction = "8+",
            AvailableCopies = 3,
            AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB, BookFormat.MOBI },
            CoverImageUrl = "/images/books/little-prince.jpg"
        },
        new Book
        {
            Title = "מורה נבוכים",
            Author = "הרמב\"ם",
            Genre = "פילוסופיה",
            Description = "ספר יסוד בפילוסופיה היהודית המשלב בין אמונה להגיון.",
            Publisher = "מאגנס",
            YearOfPublication = 2019,
            BuyPrice = 120.00m,
            BorrowPrice = 0m,
            IsBorrowable = false,
            AgeRestriction = "18+",
            AvailableCopies = 0,
            AvailableFormats = new List<BookFormat> { BookFormat.PDF },
            CoverImageUrl = "/images/books/guide.jpg"
        },
        new Book
        {
            Title = "סיפור על אהבה וחושך",
            Author = "עמוס עוז",
            Genre = "ביוגרפיה",
            Description = "רומן אוטוביוגרפי על ילדות בירושלים של תקופת המנדט והקמת המדינה.",
            Publisher = "כתר",
            YearOfPublication = 2002,
            BuyPrice = 54.90m,
            BorrowPrice = 10.90m,
            IsBorrowable = true,
            AgeRestriction = "16+",
            AvailableCopies = 3,
            AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB },
            CoverImageUrl = "/images/books/oz-love.jpg"
        },
        new Book
        {
            Title = "אלף לילה ולילה",
            Author = "עממי",
            Genre = "אגדות",
            Description = "אוסף סיפורי פולקלור קלאסיים מהמזרח התיכון.",
            Publisher = "ידיעות ספרים",
            YearOfPublication = 2015,
            BuyPrice = 129.90m,
            BorrowPrice = 19.90m,
            IsBorrowable = true,
            AgeRestriction = "12+",
            AvailableCopies = 3,
            AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB },
            CoverImageUrl = "/images/books/arabian-nights.jpg"
        },
        new Book
        {
            Title = "עיצוב מודרני",
            Author = "ג'ון סמית",
            Genre = "אמנות ועיצוב",
            Description = "מדריך מקיף לעיצוב מודרני, כולל תיאוריה והדגמות מעשיות.",
            Publisher = "מודן",
            YearOfPublication = 2023,
            BuyPrice = 199.90m,
            BorrowPrice = 0m,
            IsBorrowable = false,
            AgeRestriction = "12+",
            AvailableCopies = 0,
            AvailableFormats = new List<BookFormat> { BookFormat.PDF },
            CoverImageUrl = "/images/books/modern-design.jpg"
        },
        new Book
        {
            Title = "בישול בריא לכל המשפחה",
            Author = "שף רותם ליברמן",
            Genre = "בישול",
            Description = "ספר מתכונים בריאים וקלים להכנה, מתאים לכל המשפחה.",
            Publisher = "פן",
            YearOfPublication = 2024,
            BuyPrice = 149.90m,
            BorrowPrice = 24.90m,
            IsBorrowable = true,
            AgeRestriction = "8+",
            AvailableCopies = 3,
            AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB },
            CoverImageUrl = "/images/books/healthy-cooking.jpg"
        },
        new Book
        {
            Title = "מבוא לפסיכולוגיה",
            Author = "פרופ' דן ענבר",
            Genre = "פסיכולוגיה",
            Description = "ספר יסוד בפסיכולוגיה מודרנית, מתאים לסטודנטים ולמתעניינים.",
            Publisher = "האוניברסיטה הפתוחה",
            YearOfPublication = 2022,
            BuyPrice = 159.90m,
            BorrowPrice = 29.90m,
            IsBorrowable = true,
            AgeRestriction = "18+",
            AvailableCopies = 3,
            AvailableFormats = new List<BookFormat> { BookFormat.PDF, BookFormat.EPUB },
            CoverImageUrl = "/images/books/psychology.jpg"
        },
            };
        }
    }
}
