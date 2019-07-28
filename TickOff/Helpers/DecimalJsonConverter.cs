using Newtonsoft.Json;
using System;
using System.Globalization;

namespace TickerPlant
{
	internal class DecimalJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(double);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteRawValue(((double)value).ToString("F2", CultureInfo.InvariantCulture));
		}
	}
}
