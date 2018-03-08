using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class Attachments
    {
        public int Id { get; set; }

        //Foreign key
        public int MessageId { get; set; }

        public string Link { get; set; }
    }
}