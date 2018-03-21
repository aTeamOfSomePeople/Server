using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class MutedUsers
    {
        public int Id { get; set; }

        [Required, ForeignKey("Users"), Index("User", IsUnique = true, Order = 1)]
        public int UserId { get; set; }
        public virtual Users Users { get; set; }

        [Required, ForeignKey("Chats"), Index("User", IsUnique = true, Order = 2)]
        public int ChatId { get; set; }
        public virtual Chats Chats { get; set; }

        public DateTime End { get; set; }
    }
}