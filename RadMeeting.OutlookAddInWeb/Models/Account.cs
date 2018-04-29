namespace RedMeeting.OutlookAddInWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Account")]
    public partial class Account
    {
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string EmailId { get; set; }

        [Required]
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public int? ExpiresIn { get; set; }

        public DateTime? LastModified { get; set; }
    }
}
