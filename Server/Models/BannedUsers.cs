using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class BannedUsers
    {
        public int Id { get; set; }

        [Required, ForeignKey("User1"), Index("User", IsUnique = true, Order = 1)]
        public int BannerId { get; set; }
        public virtual Users User1 { get; set; }

        [Required, ForeignKey("User2"), Index("User", IsUnique = true, Order = 2)]
        public int BannedId { get; set; }
        public virtual Users User2 { get; set; }
    }
}