namespace Mapsui.VectorTiles.MapboxGLStyler.Filter
{
    public class TypeEqualsFilter : Filter
    {
        public GeometryType Type { get; }

        public TypeEqualsFilter(GeometryType type)
        {
            Type = type;
        }

        public override bool Evaluate(EvaluationContext context)
        {
            return context != null && context.Feature.GeometryType.Equals(Type);
        }
    }
}
