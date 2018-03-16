using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class DeletedMessages
    {
        public int Id { get; set; }

        [Required, ForeignKey("User"), Index("UserMessage", IsUnique = true, Order = 1)]
        public int UserId { get; set; }
        public virtual Users User { get; set; }

        [Required, ForeignKey("Message"), Index("UserMessage", IsUnique = true, Order = 2)]
        public int MessageId { get; set; }
        public virtual Messages Message { get; set; }
    }
}