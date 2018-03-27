namespace Mapsui.VectorTiles.MapboxGLStyler.Filter
{
    public class NotHasFilter : Filter
    {
        public string Key { get; }

        public NotHasFilter(string key)
        {
            Key = key;
        }

        public override bool Evaluate(EvaluationContext context)
        {
            return context != null && !context.Feature.Tags.ContainsKey(Key);
        }
    }
}
