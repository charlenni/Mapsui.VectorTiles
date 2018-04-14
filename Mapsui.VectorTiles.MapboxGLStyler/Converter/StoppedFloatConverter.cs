using Mapsui.VectorTiles.MapboxGLStyler.Extensions;
using Mapsui.VectorTiles.MapboxGLStyler.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mapsui.VectorTiles.MapboxGLStyler
{
    public class StoppedFloatConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(StoppedFloat) || objectType == typeof(int);
            //return typeof(StoppedDouble).IsAssignableFrom(objectType) || typeof(int).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Object)
            {
                var stoppedFloat = new StoppedFloat { Stops = new List<KeyValuePair<float, float>>() };

                if (token.SelectToken("base") != null)
                    stoppedFloat.Base = token.SelectToken("base").ToObject<float>();
                else
                    stoppedFloat.Base = 1f;

                foreach (var stop in token.SelectToken("stops"))
                {
                    var resolution = (float)stop.First.ToObject<float>().ToResolution();
                    var value = stop.Last.ToObject<float>();
                    stoppedFloat.Stops.Add(new KeyValuePair<float, float>(resolution, value));
                }

                return stoppedFloat;
            }

            return new StoppedFloat() { SingleVal = token.Value<float>() };
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
