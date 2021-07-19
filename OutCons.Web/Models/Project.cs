using System.Collections.Generic;


namespace OutCons.Web.Models
{
    public partial class Project
    {
        public Project()
        {
            TimeLogs = new HashSet<TimeLog>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<TimeLog> TimeLogs { get; set; }
    }
}
