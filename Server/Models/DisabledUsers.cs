using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class DisabledUsers
    {
        public int Id { get; set; }

        [Required, ForeignKey("User1"), Index("User", IsUnique = true, Order = 1)]
        public int Banner { get; set; }
        public virtual Users User1 { get; set; }

        [Required, ForeignKey("User2"), Index("User", IsUnique = true, Order = 2)]
        public int Banned { get; set; }
        public virtual Users User2 { get; set; }

        [Required]
        public DateTime End { get; set; }
    }
}