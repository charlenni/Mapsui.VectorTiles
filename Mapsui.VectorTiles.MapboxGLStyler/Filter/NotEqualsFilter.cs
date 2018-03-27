using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles.MapboxGLStyler.Filter
{
    public class NotEqualsFilter : Filter
    {
        public string Key { get; }
        public JValue Value { get; }

        public NotEqualsFilter(string key, JValue value)
        {
            Key = key;
            Value = value;
        }

        public override bool Evaluate(EvaluationContext context)
        {
            return context != null && !context.Feature.Tags.ContainsKeyValue(Key, Value);
        }
    }
}
