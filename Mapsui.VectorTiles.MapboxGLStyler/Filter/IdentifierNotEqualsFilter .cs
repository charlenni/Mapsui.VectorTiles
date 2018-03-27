namespace Mapsui.VectorTiles.MapboxGLStyler.Filter
{
    public class IdentifierNotEqualsFilter : Filter
    {
        public string Identifier { get; }

        public IdentifierNotEqualsFilter(string identifier)
        {
            Identifier = identifier;
        }

        public override bool Evaluate(EvaluationContext context)
        {
            return context != null && context.Feature.Id != Identifier;
        }
    }
}
