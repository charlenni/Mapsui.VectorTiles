using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapsui.VectorTiles.MapsforgeStyler.Rules
{
    internal class NegativeRule : Rule
    {
        public string[] Keys { get; }
        public string[] Values { get; }

        /* (-) 'exclusive negation' matches when either KEY is not present
         * or KEY is present and any VALUE is NOT present
         *
         * (\) 'except negation' matches when KEY is present
         * none items of VALUE is present (TODO).
         * (can be emulated by <m k="a"><m k=a v="-|b|c">...</m></m>)
         *
         * (~) 'non-exclusive negation' matches when either KEY is not present
         * or KEY is present and any VALUE is present */

        public readonly bool exclusive;

        internal NegativeRule(RuleType type, int element, int zoom, int selector, string[] keys, string[] values, Rule[] subRules, RenderStyle[] styles) 
            : base(element, zoom, selector, subRules, styles)
        {
            this.Keys = keys;
            this.Values = values;
            this.exclusive = type == RuleType.EXCLUDE;
        }

        public override bool MatchesTags(Tag[] tags)
        {
            if (!ContainsKeys(tags))
            {
                return true;
            }

            foreach (Tag tag in tags)
            {
                foreach (string value in Values)
                {
                    if (value.Equals(tag.Value))
                    {
                        return !exclusive;
                    }
                }
            }

            return exclusive;
        }

        internal virtual bool ContainsKeys(IEnumerable<Tag> tags)
        {
            foreach (Tag tag in tags)
            {
                foreach (string key in Keys)
                {
                    if (key.Equals(tag.Key))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
