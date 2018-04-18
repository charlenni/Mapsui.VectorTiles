using Mapsui.Styles;
using Mapsui.VectorTiles.MapboxGLStyler.Extensions;
using Mapsui.VectorTiles.MapboxGLStyler.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mapsui.VectorTiles.MapboxGLStyler
{
    public class StoppedColorConverter : JsonConverter
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
                var stoppedColor = new StoppedColor { Stops = new List<KeyValuePair<float, Color>>() };

                if (token.SelectToken("base") != null)
                    stoppedColor.Base = token.SelectToken("base").ToObject<float>();
                else
                    stoppedColor.Base = 1f;

                foreach (var stop in token.SelectToken("stops"))
                {
                    var zoom = (float)stop.First.ToObject<float>();
                    var colorString = stop.Last.ToObject<string>();
                    stoppedColor.Stops.Add(new KeyValuePair<float, Color>(zoom, Color.FromString(colorString)));
                }

                return stoppedColor;
            }

            return new StoppedColor() { SingleVal = Color.FromString(token.Value<string>()) };
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
