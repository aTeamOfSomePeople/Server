using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class ExternalAccounts
    {
        [Key, Column(Order = 0)]
        public long UserId { get; set; }

        [Key, ForeignKey("Services"), Column(Order = 1)]
        public long Service { get; set; }
        public virtual ExternalServices Services { get; set; }

        public bool IsDeleted { get; set; }
    }
}