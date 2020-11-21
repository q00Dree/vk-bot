using System;
using System.Collections.Generic;
using System.Text;

namespace chatbotvk.Services.Bank.Models
{
    public class ValuteResponse
    {
        public DateTime Date { get; set; }
        public DateTime PreviousDate { get; set; }
        public string PreviousURL { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, Valutes> Valute { get; set; }
    }
}
