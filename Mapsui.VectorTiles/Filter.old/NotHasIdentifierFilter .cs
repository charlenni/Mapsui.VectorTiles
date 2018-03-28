namespace Mapsui.VectorTiles.Filter
{
    public class NotHasIdentifierFilter : Filter
    {
        public NotHasIdentifierFilter()
        {
        }

        public override bool Evaluate(EvaluationContext context)
        {
            return context != null && string.IsNullOrWhiteSpace(context.Feature.Id);
        }
    }
}
