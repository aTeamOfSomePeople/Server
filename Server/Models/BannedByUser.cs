using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class BannedByUser
    {
        public long Id { get; set; }

        [Required, ForeignKey("User1"), Index("Ban", IsUnique = true, Order = 1)]
        public long BannerId { get; set; }
        public virtual Users User1 { get; set; }

        [Required, ForeignKey("User2"), Index("Ban", IsUnique = true, Order = 2)]
        public long BannedId { get; set; }
        public virtual Users User2 { get; set; }
    }
}