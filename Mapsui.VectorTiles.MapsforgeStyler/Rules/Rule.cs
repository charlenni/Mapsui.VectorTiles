/*
 * Copyright 2014 Hannes Janetzek
 * Copyright 2016 devemux86
 *
 * This file is part of the OpenScienceMap project (http://www.opensciencemap.org).
 * 
 * This program is free software: you can redistribute it and/or modify it under the
 * terms of the GNU Lesser General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY
 * WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
 * PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with
 * this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Mapsui.VectorTiles.Styles;
using System.Collections.Generic;

namespace Mapsui.VectorTiles.MapsforgeStyler.Rules
{
	public class Rule
	{
		public static readonly List<IStyle> EmptyStyle = new List<IStyle>();
		public static readonly Rule[] EmptyRule = new Rule[0];

		public Rule[] SubRules { get; }
		public List<IStyle> Styles { get; }

		public string cat;
		public int Zoom { get; }
		public int Element { get; }
		public bool SelectFirstMatch { get; }
		public bool SelectWhenMatched { get; }

		internal Rule(int element, int zoom, int selector, Rule[] subRules, List<IStyle> styles)
		{
			Element = element;
			Zoom = zoom;

			SubRules = (subRules == null) ? EmptyRule : subRules;
			Styles = (styles == null) ? EmptyStyle : styles;

			SelectFirstMatch = (selector & Selector.FIRST) != 0;
			SelectWhenMatched = (selector & Selector.WHEN_MATCHED) != 0;
		}

		public virtual bool MatchesTags(Tag[] tags)
		{
			return true;
		}

		public virtual bool MatchElement(int type, Tag[] tags, int zoomLevel, IList<IStyle> result)
		{
			if (((Element & type) == 0) || ((Zoom & zoomLevel) == 0) || !MatchesTags(tags))
			{
				return false;
			}

			bool matched = false;

			if (SubRules != EmptyRule)
			{
				if (SelectFirstMatch)
				{
					/* only add first matching rule and when-matched rules if a
					 * previous rule matched */
					foreach (Rule r in SubRules)
					{
						/* continue if matched xor selectWhenMatch */
						if (matched ^ r.SelectWhenMatched)
						{
							continue;
						}

						if (r.MatchElement(type, tags, zoomLevel, result))
						{
							matched = true;
						}
					}
				}
				else
				{
					/* add all rules and when-matched rules iff a previous rule
					 * matched */
					foreach (Rule r in SubRules)
					{
						if (r.SelectWhenMatched && !matched)
						{
							continue;
						}

						if (r.MatchElement(type, tags, zoomLevel, result))
						{
							matched = true;
						}
					}
				}
			}

			if (Styles == EmptyStyle)
				/* matched if styles where added */
			{
				return matched;
			}

			/* add instructions for this rule */
			foreach (IStyle ri in Styles)
			{
				result.Add(ri);
			}

			/* this rule did match */
			return true;
		}

		public virtual void Dispose()
		{
			foreach (IStyle ri in Styles)
			{
				ri.Dispose();
			}

			foreach (Rule subRule in SubRules)
			{
				subRule.Dispose();
			}
		}

		public virtual void ScaleTextSize(float scaleFactor)
		{
			foreach (IStyle ri in Styles)
			{
				ri.ScaleTextSize(scaleFactor);
			}

			foreach (Rule subRule in SubRules)
			{
				subRule.ScaleTextSize(scaleFactor);
			}
		}

		public virtual Rule SetCat(string cat)
		{
			this.cat = cat;
			return this;
		}

		public virtual void updateStyles()
		{
			foreach (IStyle ri in Styles)
			{
				ri.Update();
			}

			foreach (Rule subRule in SubRules)
			{
				subRule.updateStyles();
			}
		}

		public class RuleVisitor
		{
			public virtual void apply(Rule r)
			{
				foreach (Rule subRule in r.SubRules)
				{
					this.apply(subRule);
				}
			}
		}

		public class TextSizeVisitor : RuleVisitor
		{
			internal float scaleFactor = 1;

			public virtual float ScaleFactor
			{
				set
				{
					this.scaleFactor = value;
				}
			}

			public override void apply(Rule r)
			{
				foreach (IStyle ri in r.Styles)
				{
					ri.ScaleTextSize(scaleFactor);
				}
				base.apply(r);
			}
		}

		public class UpdateVisitor : RuleVisitor
		{
			public override void apply(Rule r)
			{
				foreach (IStyle ri in r.Styles)
				{
					ri.Update();
				}
				base.apply(r);
			}
		}

		public virtual void Apply(RuleVisitor v)
		{
			v.Apply(this);
		}

		public static RuleBuilder builder()
		{
			return new RuleBuilder();
		}
	}

}