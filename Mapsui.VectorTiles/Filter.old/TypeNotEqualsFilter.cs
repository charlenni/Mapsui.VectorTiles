namespace Mapsui.VectorTiles.Filter
{
    public class TypeNotEqualsFilter : Filter
    {
        public GeometryType Type { get; }

        public TypeNotEqualsFilter(GeometryType type)
        {
            Type = type;
        }

        public override bool Evaluate(EvaluationContext context)
        {
            return context != null && !context.Feature.GeometryType.Equals(Type);
        }
    }
}
