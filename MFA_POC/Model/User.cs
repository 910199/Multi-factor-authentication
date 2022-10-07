using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace MFA_POC.Model
{
    public class User
    {
        public User() { }
        public User(Mail mail)
        {
            this.address = mail.address;
            this.userId = mail.userId;
            this.otpCode = RandomCode();
        }

        public string userId { get; set; }
        public string address { get; set; }
        public string otpCode { get; set; }
        public MemoryStream stream { get; set; }

        public string RandomCode()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string randomed_20chars = "";
            string out_str = "";
            int out_str_len = 8;
            var random = new Random();

            for (int i = 0; i < 20; i++)
            {
                randomed_20chars += chars[random.Next(chars.Length)];
            }
            for (int i = 0; i < out_str_len; i++)
            {
                out_str += randomed_20chars[random.Next(randomed_20chars.Length)];
            }
            return out_str;

        }

        
    }
}
