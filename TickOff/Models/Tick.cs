using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace TickOff.Models
{
	public class Tick : StockSymbolBase
	{
		[IgnoreDataMember]
		public decimal Ask { get; set; }
		[DataMember(Name = "Ask")]
		public decimal RoundedAsk => Math.Round(Ask, 4);

		[IgnoreDataMember]
		public decimal Bid { get; set; }
		[DataMember(Name = "Bid")]
		public decimal RoundedBid => Math.Round(Bid, 4);

		[IgnoreDataMember]
		public decimal LastPrice { get; set; }
		[DataMember(Name = "LastPrice")]
		public decimal RoundedLastPrice => Math.Round(LastPrice, 4);

		public int AskSize { get; set; }

		public int BidSize { get; set; }

		public long Volume { get; set; }

		[IgnoreDataMember]
		public decimal High { get; set; }
		[DataMember(Name = "High")]
		public decimal RoundedHigh => Math.Round(High, 4);

		[IgnoreDataMember]
		public decimal Low { get; set; }
		[DataMember(Name = "Low")]
		public decimal RoundedLow => Math.Round(Low, 4);

		[IgnoreDataMember]
		public decimal Open { get; set; }
		[DataMember(Name = "Open")]
		public decimal RoundedOpen => Math.Round(Open, 4);

		[IgnoreDataMember]
		public decimal PercentChange { get; set; }
		[DataMember(Name = "PercentChange")]
		public decimal RoundedPercentChange => Math.Round(PercentChange, 4);

		[IgnoreDataMember]
		public DateTime TimeStamp { get; set; }
		[DataMember(Name = "TimeStamp")]
		public string ConvertedTimeStamp => TimeStamp.ToString("o", CultureInfo.InvariantCulture);
	}
}
