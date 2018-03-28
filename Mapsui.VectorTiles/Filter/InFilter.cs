using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles.Filter
{
    public class InFilter : Filter
    {
        public string Key { get; }
        public IList<JValue> Values { get; }

        public InFilter(string key, IEnumerable<JValue> values)
        {
            Key = key;
            Values = new List<JValue>();

            foreach(var value in values)
                Values.Add(value);
        }

        public override bool Evaluate(EvaluationContext context)
        {
            if (context == null || !context.Feature.Tags.ContainsKey(Key) || context.Feature.Tags[Key] == null)
                return false;

            foreach (var value in Values)
            {
                if (context.Feature.Tags[Key].Equals(value))
                    return true;
            }

            return false;
        }
    }
}
