using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MFA_POC.Model
{
    public class Payload
    {
        public Payload() { }

        public int inputCode { get; set; }
        public string AccountId { get; set; }
        public string issuer { get; set; }

    }


}
