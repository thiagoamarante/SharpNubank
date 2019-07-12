using System;
using System.Collections.Generic;
using System.Text;

namespace SharpNubank.Models
{
    public class CreditCardEvent
    {
        #region Fields
        private decimal _Amount;
        #endregion

        public string Id { get; set; }

        public string Href { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        public decimal Amount { get { return _Amount; } set { _Amount = value / 100; } }

        public DateTime Time { get; set; }
    }
}
