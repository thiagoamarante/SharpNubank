using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpNubank.Models
{
    public class CustomerDetails
    {
        [JsonProperty("cpf")]
        public string Cpf { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("printed_name")]
        public string PrintedName { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("preferred_name")]
        public string PreferredName { get; set; }
    }
}
