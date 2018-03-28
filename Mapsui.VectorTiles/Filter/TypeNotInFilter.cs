using System.Collections.Generic;

namespace Mapsui.VectorTiles.Filter
{
    public class TypeNotInFilter : Filter
    {
        public IList<GeometryType> Types { get; }

        public TypeNotInFilter(IEnumerable<GeometryType> types)
        {
            Types = new List<GeometryType>();

            foreach (var type in types)
                Types.Add(type);
        }

        public override bool Evaluate(EvaluationContext context)
        {
            if (context == null)
                return true;

            foreach (var type in Types)
            {
                if (context.Feature.GeometryType.Equals(type))
                    return false;
            }

            return true;
        }
    }
}
