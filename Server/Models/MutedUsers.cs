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
        public long Id { get; set; }

        [Required, ForeignKey("Users"), Index("Mute", IsUnique = true, Order = 1)]
        public long UserId { get; set; }
        public virtual Users Users { get; set; }

        [Required, ForeignKey("Chats"), Index("Mute", IsUnique = true, Order = 2)]
        public long ChatId { get; set; }
        public virtual Chats Chats { get; set; }

        public DateTime End { get; set; }
    }
}