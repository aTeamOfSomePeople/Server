using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API.Models
{
    public class Messages
    {
        public int Id { get; set; }

        //Foreign keys
        public int ChatId { get; set; }
        public int UserId { get; set; }
    }
}