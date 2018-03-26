using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Server.Utils
{
    public class ErrorMessage
    {
        public int errorCode { get; set; }
        public string description { get; set; }
        public ErrorMessage(int errorCode, string desctiption)
        {
            this.errorCode = errorCode;
            this.description = description;
        }
    }
}