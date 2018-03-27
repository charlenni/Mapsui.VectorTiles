namespace Mapsui.VectorTiles.MapboxGLStyler.Filter
{
    public class HasFilter : Filter
    {
        public string Key { get; }

        public HasFilter(string key)
        {
            Key = key;
        }

        public override bool Evaluate(EvaluationContext context)
        {
            return context != null && context.Feature.Tags.ContainsKey(Key);
        }
    }
}
