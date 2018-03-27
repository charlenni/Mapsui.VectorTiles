using System.Collections.Generic;

namespace Mapsui.VectorTiles.MapboxGLStyler.Filter
{
    public class TypeInFilter : Filter
    {
        public IList<GeometryType> Types { get; }

        public TypeInFilter(IEnumerable<GeometryType> types)
        {
            Types = new List<GeometryType>();

            foreach (var type in types)
                Types.Add(type);
        }

        public override bool Evaluate(EvaluationContext context)
        {
            if (context == null)
                return false;

            foreach (var type in Types)
            {
                if (context.Feature.GeometryType.Equals(type))
                    return true;
            }

            return false;
        }
    }
}
