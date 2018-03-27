namespace Mapsui.VectorTiles.MapboxGLStyler.Filter
{
    public class IdentifierEqualsFilter : Filter
    {
        public string Identifier { get; }

        public IdentifierEqualsFilter(string identifier)
        {
            Identifier = identifier;
        }

        public override bool Evaluate(EvaluationContext context)
        {
            return context != null && context.Feature.Id == Identifier;
        }
    }
}
