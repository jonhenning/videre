using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Videre.Core.JsonConverters
{
    public class IsoDateConverter : DateTimeConverterBase
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //bool nullable = IsNullableType(objectType);
            //Type t = (nullable) ? Nullable.GetUnderlyingType(objectType) : objectType;

            //if (reader.TokenType == JsonToken.Null)
            //{
            //    if (!nullable)
            //        throw new Exception(String.Format("Cannot convert null value to {0}.", objectType));
            //    return null;
            //}

            if (reader.TokenType != JsonToken.String)
                throw new Exception(String.Format("Unexpected token parsing date. Expected string got {0}.", reader.TokenType));

            var dateString = reader.Value.ToString();
            if (!string.IsNullOrEmpty(dateString))
                return DateTime.ParseExact(dateString, "yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture);
            else
                return null;


        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime)
            {
                writer.WriteValue(((DateTime)value).ToString("yyyy-MM-dd"));
            }
            //else if (value is DateTime?)
            //{
            //    var date = (DateTime?)value;
            //    if (date.HasValue)
            //        writer.WriteValue(date.Value.ToString("yyyy-MM-dd"));
            //    else
            //        writer.WriteValue((string)null);
            //}
            else
            {
                throw new Exception("Expected date object value.");
            }
        }

        //private bool IsNullableType(object obj)
        //{
        //    Type t = obj.GetType();
        //    return t.IsGenericType
        //        && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        //}

    }
}
