using Mapsui.Styles;
using System.Collections.Generic;

namespace Mapsui.VectorTiles.MapsforgeStyler.Rules
{
    internal class PositiveRuleK : Rule
    {
        internal readonly string mKey;

        internal PositiveRuleK(int element, int zoom, int selector, string key, Rule[] subRules, IStyle[] styles) 
            : base(element, zoom, selector, subRules, styles)
        {
            mKey = key;
        }

        public override bool MatchesTags(IEnumerable<Tag> tags)
        {
            foreach (Tag tag in tags)
            {
                if (mKey.Equals(tag.Key))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
