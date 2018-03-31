using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles.Filter
{
    public class GreaterThanFilter : BinaryFilter
    {
        public GreaterThanFilter(string key, JValue value) : base(key, value)
        {
        }

        public override bool Evaluate(EvaluationContext context)
        {
            if (context == null || !context.Feature.Tags.ContainsKey(Key))
                return false;

            if (context.Feature.Tags[Key].Type == JTokenType.Float ||
                context.Feature.Tags[Key].Type == JTokenType.Integer)
                return (float)context.Feature.Tags[Key] > (float)Value;

            return false;
        }
    }
}
