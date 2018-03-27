using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Mapsui.VectorTiles.MapboxGLStyler
{
    public class StoppedStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(StoppedString) || objectType == typeof(string);
            //return typeof(StoppedDouble).IsAssignableFrom(objectType) || typeof(int).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Object)
            {
                return token.ToObject<StoppedString>();
            }
            return new StoppedString() { SingleVal = token.Value<string>() };
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
