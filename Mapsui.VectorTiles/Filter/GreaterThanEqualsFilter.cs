using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles.Filter
{
    public class GreaterThanEqualsFilter : BinaryFilter
    {
        public GreaterThanEqualsFilter(string key, JValue value) : base(key, value)
        {
        }

        public override bool Evaluate(EvaluationContext context)
        {
            if (context == null || !context.Feature.Tags.ContainsKey(Key))
                return false;

            if (context.Feature.Tags[Key].Type == JTokenType.Float ||
                context.Feature.Tags[Key].Type == JTokenType.Integer)
                return (float)Value <= (float)context.Feature.Tags[Key];

            return false;
        }
    }
}
