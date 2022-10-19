using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MFA_POC.Model
{
    public class User_qrcode
    {
        public User_qrcode() { }

        public User_qrcode(Payload payload)
        {
            this.userId = payload.userId;         
            this.secret_key = "12jeffery20"+this.userId+SecretRandomCode();
        }

        public string userId { get; set; }
        public string secret_key { get; set; }



        public string SecretRandomCode()
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
