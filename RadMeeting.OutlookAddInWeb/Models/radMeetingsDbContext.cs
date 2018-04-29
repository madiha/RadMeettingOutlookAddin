namespace RedMeeting.OutlookAddInWeb.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class radMeetingsDbContext : DbContext
    {
        public radMeetingsDbContext()
            : base("name=radMeetingsDbContextConnection")
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
