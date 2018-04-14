using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles.MapboxGLFormat
{
    using System.Collections.Generic;
    using System.Linq;

    public static class TagsParser
    {
        public static TagsCollection Parse(List<string> keys, List<Tile.Value> values, List<uint> tags)
        {
            var result = new TagsCollection();
            var odds = tags.GetOdds().ToList();
            var evens = tags.GetEvens().ToList();

            for (var i = 0; i < evens.Count; i++)
            {
                var key = keys[(int)evens[i]];
                var val = values[(int)odds[i]];
                var valObject = GetAttr(val);
                result.Add(key, new JValue(valObject));
            }
            return result;
        }

        private static object GetAttr(Tile.Value value)
        {
            object res = null;

            if (value.HasBoolValue)
            {
                res = value.BoolValue;
            }
            else if (value.HasDoubleValue)
            {
                res = value.DoubleValue;
            }
            else if (value.HasFloatValue)
            {
                res = value.FloatValue;
            }
            else if (value.HasIntValue)
            {
                res = value.IntValue;
            }
            else if (value.HasStringValue)
            {
                res = value.StringValue;
            }
            else if (value.HasSIntValue)
            {
                res = value.SintValue;
            }
            else if (value.HasUIntValue)
            {
                res = value.UintValue;
            }
            return res;
        }
    }
}