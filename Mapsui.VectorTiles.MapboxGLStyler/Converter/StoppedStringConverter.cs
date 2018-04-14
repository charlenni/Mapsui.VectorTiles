﻿using Mapsui.VectorTiles.MapboxGLStyler.Extensions;
using Mapsui.VectorTiles.MapboxGLStyler.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

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
                var stoppedString = new StoppedString { Stops = new List<KeyValuePair<float, string>>() };

                stoppedString.Base = token.SelectToken("base").ToObject<float>();

                foreach (var stop in token.SelectToken("stops"))
                {
                    var resolution = (float)stop.First.ToObject<float>().ToResolution();
                    var text = stop.Last.ToObject<string>();
                    stoppedString.Stops.Add(new KeyValuePair<float, string>(resolution, text));
                }

                return stoppedString;
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
