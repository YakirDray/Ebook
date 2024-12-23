
// Models/WaitingListItem.cs

namespace MyEBookLibrary.Models
{
    public class WaitingListItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        public bool IsNotified { get; set; }
        public BookFormat Format { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Book Book { get; set; } = null!;
    }
}