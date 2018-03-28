namespace Mapsui.VectorTiles.Filter
{
    public class HasIdentifierFilter : Filter
    {
        public HasIdentifierFilter()
        {
        }

        public override bool Evaluate(EvaluationContext context)
        {
            return context != null && !string.IsNullOrWhiteSpace(context.Feature.Id);
        }
    }
}
