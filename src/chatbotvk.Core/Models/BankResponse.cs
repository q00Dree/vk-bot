using System;
using System.Collections.Generic;
using System.Text;

namespace chatbotvk.Core.Models
{
    /// <summary>
    /// Возможно стоило либо дублировать объекты ValuteResponse и Valutes на уровне логики.
    /// TODO: Подумать над рефакторингом.
    /// </summary>
    public class BankResponse
    {
        /// <summary>
        /// Дата запроса курсов, поле "Date"
        /// </summary>
        public DateTime CurrentRateDate { get; set; }
        /// <summary>
        /// Последнее обновление курсов, поле "PreviousDate"
        /// </summary>
        public DateTime LastRateUpdateDate { get; set; }
        /// <summary>
        /// Актуальные котировки, поле "Key" - сокращение название валюты.(Key) / "Value" - рубли.(Value)
        /// </summary>
        public Dictionary<string, double> ActualValue { get; set; }
        /// <summary>
        /// Предыдущие котировки, поле "Key" - сокращение название валюты.(Key) / "Value" - рубли.(Value)
        /// </summary>
        public Dictionary<string, double> PreviousValue { get; set; }
        public BankResponse()
        {
            ActualValue = new Dictionary<string, double>();
            PreviousValue = new Dictionary<string, double>();
        }
    }
}
