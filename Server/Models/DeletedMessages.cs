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
        public long Id { get; set; }

        [Required, ForeignKey("User"), Index("UserMessage", IsUnique = true, Order = 1)]
        public long UserId { get; set; }
        public virtual Users User { get; set; }

        [Required, ForeignKey("Message"), Index("UserMessage", IsUnique = true, Order = 2)]
        public long MessageId { get; set; }
        public virtual Messages Message { get; set; }
    }
}