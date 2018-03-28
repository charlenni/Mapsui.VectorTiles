using System.Collections.Generic;

namespace Mapsui.VectorTiles.Filter
{
    public class IdentifierNotInFilter : Filter
    {
        public IList<string> Identifiers { get; }

        public IdentifierNotInFilter(IEnumerable<string> identifiers)
        {
            Identifiers = new List<string>();

            foreach (var identifier in identifiers)
                Identifiers.Add(identifier);
        }

        public override bool Evaluate(EvaluationContext context)
        {
            if (context == null)
                return true;

            foreach (var identifier in Identifiers)
            {
                if (context.Feature.Id == identifier)
                    return false;
            }

            return true;
        }
    }
}
