using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles.MapboxGLStyler.Filter
{
    public class LessThanEqualsFilter : BinaryFilter
    {
        public LessThanEqualsFilter(string key, JValue value) : base(key, value)
        {
        }

        public override bool Evaluate(EvaluationContext context)
        {
            if (context == null || !context.Feature.Tags.ContainsKey(Key))
                return false;

            if (context.Feature.Tags[Key].Type != JTokenType.Float &&
                context.Feature.Tags[Key].Type != JTokenType.Integer)
                return false;

            return (float)Value <= (float)context.Feature.Tags[Key];
        }
    }
}
