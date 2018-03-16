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
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required, ForeignKey("Chat")]
        public int ChatId { get; set; }
        public virtual Chats Chat { get; set; }

        [Required, ForeignKey("User")]
        public int UserId { get; set; }
        public virtual Users User { get; set; }

        public bool IsReaded { get; set; }
    }
}