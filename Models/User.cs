using Microsoft.AspNetCore.Identity;

namespace MyEBookLibrary.Models
{
    public class User : IdentityUser<int>
    {

        public ICollection<Transaction> Transactions { get; set; } = [];

        public virtual ICollection<UserBook> Books { get; set; } = [];
        public virtual ICollection<BookReview> Reviews { get; set; } = [];
        public virtual ICollection<WaitingListItem> WaitingListItems { get; set; } = [];

        internal decimal Sum(Func<object, object> value)
        {
            throw new NotImplementedException();
        }
    }

}