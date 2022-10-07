using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MFA_POC.Model
{
    public class Mail
    {
        public Mail() { }

        public string userId { get; set; }
        public string address { get; set; }
        public string inputCode { get; set; }
    }
}
