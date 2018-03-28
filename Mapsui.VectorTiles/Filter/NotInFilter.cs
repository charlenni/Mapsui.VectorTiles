using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles.Filter
{
    public class NotInFilter : Filter
    {
        public string Key { get; }
        public JArray Values { get; }

        public NotInFilter(string key, IEnumerable<JValue> values)
        {
            Key = key;
            Values = new JArray();

            foreach (var value in values)
                Values.Add(value);
        }

        public override bool Evaluate(EvaluationContext context)
        {
            if (context == null || !context.Feature.Tags.ContainsKey(Key) || context.Feature.Tags[Key] == null)
                return true;

            foreach (var value in Values)
            {
                if (context.Feature.Tags[Key].Equals(value))
                    return false;
            }

            return true;
        }
    }
}
