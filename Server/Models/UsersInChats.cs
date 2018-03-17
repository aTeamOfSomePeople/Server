using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class UsersInChats
    {
        public int Id { get; set; }

        [Required, ForeignKey("Chat"), Index("ChatUser", IsUnique = true, Order = 1)]
        public int ChatId { get; set; }
        public virtual Chats Chat {get; set;}

        [Required, ForeignKey("User"), Index("ChatUser", IsUnique = true, Order = 2)]
        public int UserId { get; set; }
        public virtual Users User { get; set; }

        public int UnreadedMessages { get; set; }
        
        public bool CanWrite { get; set; }
    }
}