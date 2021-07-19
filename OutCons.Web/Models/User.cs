using System.Collections.Generic;


namespace OutCons.Web.Models
{
    public partial class User
    {
        public User()
        {
            TimeLogs = new HashSet<TimeLog>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public virtual ICollection<TimeLog> TimeLogs { get; set; }
    }
}
