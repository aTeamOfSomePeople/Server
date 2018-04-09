using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class BannedByChat
    {
        public long Id { get; set; }

        [Required, ForeignKey("Chats"), Index("Ban", IsUnique = true, Order = 1)]
        public long BannerId { get; set; }
        public virtual Chats Chats { get; set; }

        [Required, ForeignKey("Users"), Index("Ban", IsUnique = true, Order = 2)]
        public long BannedId { get; set; }
        public virtual Users Users { get; set; }
    }
}