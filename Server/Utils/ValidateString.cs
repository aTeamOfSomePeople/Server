using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Server.Utils
{
    public static class ValidateString
    {
        private static readonly char[] ForbiddenInFileName = { '/', '\\', '?', '%', '*', ':', '|', '\"', '<', '>' };

        public static bool FileName(string fileName)
        {
            return !fileName.Any(e => ForbiddenInFileName.Contains(e));
        }

        public static bool UserName(string userName)
        {
            return true;
        }

        public static bool Login(string login)
        {
            return true;
        }

        public static bool Password(string password)
        {
            return true;
        }
    }
}