using System;
using System.Collections.Generic;
using System.Text;

namespace SharpNubank.Models
{
    public class Response<TData> : Response
    {
        public TData Data { get; set; }
    }

    public class Response
    {
        public bool Success { get; set; }

        public string Message { get; set; }
    }
}
