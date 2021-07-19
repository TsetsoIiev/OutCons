using System;

namespace OutCons.Web.Models
{
    public partial class TimeLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public DateTime Date { get; set; }
        public double Hours { get; set; }

        public virtual Project Project { get; set; }
        public virtual User User { get; set; }
    }
}
