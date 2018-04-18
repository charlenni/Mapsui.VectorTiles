using Mapsui.VectorTiles.MapboxGLStyler.Extensions;
using Mapsui.VectorTiles.MapboxGLStyler.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mapsui.VectorTiles.MapboxGLStyler
{
    public class StoppedBooleanConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(StoppedBoolean) || objectType == typeof(bool);
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Object)
            {
                var stoppedBoolean = new StoppedBoolean { Stops = new List<KeyValuePair<float, bool>>() };

                foreach (var stop in token.SelectToken("stops"))
                {
                    var zoom = (float)stop.First.ToObject<float>();
                    var value = stop.Last.ToObject<bool>();
                    stoppedBoolean.Stops.Add(new KeyValuePair<float, bool>(zoom, value));
                }

                return stoppedBoolean;
            }

            return new StoppedBoolean() { SingleVal = token.Value<bool>() };
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
