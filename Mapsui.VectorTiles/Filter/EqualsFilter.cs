using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles.Filter
{
    public class EqualsFilter : Filter
    {
        public string Key { get; }
        public JValue Value { get; }

        public EqualsFilter(string key, JValue value)
        {
            Key = key;
            Value = value;
        }
        
        public override bool Evaluate(EvaluationContext context)
        {
            return context != null && context.Feature.Tags.ContainsKeyValue(Key, Value);
        }
    }
}
