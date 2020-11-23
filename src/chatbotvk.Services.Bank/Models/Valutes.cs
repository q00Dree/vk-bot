using System;
using System.Collections.Generic;
using System.Text;

namespace chatbotvk.Services.Bank.Models
{
    public class Valutes
    {
        public string Id { get; set; }
        public string NumCode { get; set; }
        public string CharCode { get; set; }
        public int Nominal { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public double Previous { get; set; }
    }
}
