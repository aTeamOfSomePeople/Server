using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class Users
    {
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Avatar { get; set; }

        [Required, Index("Account", IsUnique = true, Order = 0)]
        public long AccountId { get; set; }

        [Required, Index("Account", IsUnique = true, Order = 1), ForeignKey("Service")]
        public long ServiceId { get; set; }
        public virtual ExternalServices Service { get; set; }

        public string Description { get; set; }
    }   
}