using System;
using System.Collections.Generic;
using System.Text;

namespace SharpNubank.Models
{
    public class AccessToken
    {
        public bool Authenticated { get; set; }

        public string Token { get; set; }

        public string EndPointEvents { get; set; }

        public string EndPointCustomer { get; set; }

        public string EndPointSavingsAccount { get; set; }
    }
}
