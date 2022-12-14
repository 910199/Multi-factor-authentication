using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel.DataAnnotations;

namespace MFA_POC.Model
{
    public class User
    {
        public User() { }
        public User(Mail mail)
        {
            this.address = mail.address;
            this.AccountId = mail.AccountId;
            this.mail_otpCode = RandomCode(8);
        }

        public User(Payload payload)
        {
            this.AccountId = payload.AccountId;
            this.secret_key = RandomCode(4) + this.AccountId + RandomCode(4);
        }

        public void SetUser(User user)
        {
            this.secret_key = user.secret_key;
        }
        public void SetUser(Mail mail)
        {
            this.address = mail.address;
            this.mail_otpCode = RandomCode(8);
        }

        [Key]
        public string AccountId { get; set; }
        public string address { get; set; }
        public string mail_otpCode { get; set; }

        public string secret_key { get; set; }

        public bool first_entry { get; set; } = true;
        public bool AuthenticationEnable { get; set; } = false;

        public string RandomCode(int output_len)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string randomed_20chars = "";
            string out_str = "";
            int out_str_len = output_len;
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
