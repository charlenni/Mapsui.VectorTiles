namespace Mapsui.VectorTiles.MapsforgeStyler.Rules
{
    internal class PositiveRuleMultiKV : Rule
    {
        internal readonly string[] mKeys;
        internal readonly string[] mValues;

        internal PositiveRuleMultiKV(int element, int zoom, int selector, string[] keys, string[] values, Rule[] subRules, RenderStyle[] styles)
            : base(element, zoom, selector, subRules, styles)
        {
            if (keys.Length == 0)
            {
                mKeys = null;
            }
            else
            {
                mKeys = keys;
            }

            if (values.Length == 0)
            {
                mValues = null;
            }
            else
            {
                mValues = values;
            }
        }

        public override bool MatchesTags(Tag[] tags)
        {
            if (mKeys == null)
            {
                foreach (Tag tag in tags)
                {
                    foreach (string value in mValues)
                    {
                        if (value.Equals(tag.Value))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            foreach (Tag tag in tags)
            {
                foreach (string key in mKeys)
                {
                    if (key.Equals(tag.Key))
                    {
                        if (mValues == null)
                        {
                            return true;
                        }

                        foreach (string value in mValues)
                        {
                            if (value.Equals(tag.Value))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}