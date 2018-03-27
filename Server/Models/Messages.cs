using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class Messages
    {
        public long Id { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required, ForeignKey("Chat")]
        public long ChatId { get; set; }
        public virtual Chats Chat { get; set; }

        [Required, ForeignKey("User")]
        public long UserId { get; set; }
        public virtual Users User { get; set; }

        public bool IsReaded { get; set; }
    }
}