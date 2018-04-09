using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class Chats
    {
        public long Id { get; set; }

        [Required, ForeignKey("User")]
        public long Creator { get; set; }
        public virtual Users User { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public Enums.ChatType Type { get; set; }

        [ForeignKey("UploadedFiles")]
        public long? Avatar { get; set; }
        public virtual UploadedFiles UploadedFiles { get; set; }

        public long MembersCount { get; set; }
    }
}