using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace DepartMentStore.Models
{
    public class encryptPassword
    {
        public static string textToEncrypt(string paasWord)
        {
            if(paasWord !=null)
                return Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(paasWord)));
            return paasWord;
        }
    }
}