using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class Attachments
    {
        public long Id { get; set; }

        [Required, ForeignKey("UploadedFiles")]
        public long FileId { get; set; }
        public virtual UploadedFiles UploadedFiles { get; set; }

        [Required, ForeignKey("Message")]
        public long MessageId { get; set; }
        public virtual Messages Message { get; set; }


    }
}