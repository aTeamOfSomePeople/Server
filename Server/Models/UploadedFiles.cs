using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class UploadedFiles
    {
        public long Id { get; set; }

        [Required]
        public string Link { get; set; }

        [Required]
        public long FileSize { get; set; }
    }
}