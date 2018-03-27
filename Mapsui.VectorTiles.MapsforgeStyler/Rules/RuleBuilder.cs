using Mapsui.VectorTiles.Styles;
using System;
using System.Collections.Generic;
using System.Linq;

/*
 * Copyright 2014 Hannes Janetzek
 * Copyright 2016-2017 devemux86
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
namespace Mapsui.VectorTiles.MapsforgeStyler.Rules
{
	public class RuleBuilder
	{
		private static readonly string[] EmptyKV = new string[] {};

		public string cat;
		internal int zoom;
		internal int element;
		internal int selector;
		internal RuleType type;

		internal string[] keys;
		internal string[] values;

		internal List<IStyle> styles = new List<IStyle>(4);
		internal List<RuleBuilder> subRules = new List<RuleBuilder>(4);

		private const string StringNegation = "~";
		private const string StringExclusive = "-";
		private static readonly char[] Separator = { '|' };

		//private static final String STRING_WILDCARD = "*";

		public RuleBuilder(RuleType type, int element, int zoom, int selector, string[] keys, string[] values)
		{
			this.type = type;
			this.element = element;
			this.zoom = zoom;
			this.selector = selector;
			this.keys = keys;
			this.values = values;
		}

		public RuleBuilder(RuleType type, string[] keys, string[] values)
		{
			this.element = Element.ANY;
			this.zoom = ~0;
			this.type = type;
			this.keys = keys;
			this.values = values;
		}

		public RuleBuilder()
		{
			this.type = RuleType.POSITIVE;
			this.element = Element.ANY;
			this.zoom = ~0;
			this.keys = EmptyKV;
			this.values = EmptyKV;
		}

		public static RuleBuilder Create(string k, string v)
		{
			string[] keys = EmptyKV;
			string[] values = EmptyKV;
			RuleType type = RuleType.POSITIVE;

			if (!string.IsNullOrEmpty(v))
			{
				var valueList = v.Trim().Split(Separator).ToList();

				if (valueList.Remove(StringNegation))
				{
					type = RuleType.NEGATIVE;
					values = valueList.ToArray();
				}
				else if (valueList.Remove(StringExclusive))
				{
					type = RuleType.EXCLUDE;
					values = valueList.ToArray();
				}
				else
				{
					values = valueList.ToArray();
				}
			}

			if (!string.IsNullOrEmpty(k))
			{
				keys = k.Split(Separator);
			}

			if (type != RuleType.POSITIVE)
			{
				if (keys == null || keys.Length == 0)
				{
					throw new ArgumentException("negative rule requires key");
				}
			}

			return new RuleBuilder(type, keys, values);
		}

		public virtual RuleBuilder SetZoom(sbyte zoomMin, sbyte zoomMax)
		{
			// zoom-level bitmask
			zoom = 0;

			for (int z = zoomMin; z <= zoomMax && z < 32; z++)
			{
				zoom |= (1 << z);
			}

			return this;
		}

		public virtual Rule onComplete(int[] level)
		{

			RenderStyle[] styles = null;
			Rule[] rules = null;

			if (styleBuilder != null)
			{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.oscim.theme.styles.RenderStyle.StyleBuilder<?> style : styleBuilder)
				foreach (RenderStyle.StyleBuilder<object> style in styleBuilder)
				{
					renderStyles.Add(style.level(level[0]).build());
					level[0] += 2;
				}
			}

			if (renderStyles.Count > 0)
			{
				styles = new RenderStyle[renderStyles.Count];
				renderStyles.toArray(styles);
			}

			if (subRules.Count > 0)
			{
				rules = new Rule[subRules.Count];
				for (int i = 0; i < rules.Length; i++)
				{
					rules[i] = subRules[i].onComplete(level);
				}
			}

			int numKeys = keys.Length;
			int numVals = values.Length;

			if (numKeys == 0 && numVals == 0)
			{
				return (new Rule(element, zoom, selector, rules, styles)).SetCat(cat);
			}

			if (type != RuleType.POSITIVE)
			{
				return (new NegativeRule(type, element, zoom, selector, keys, values, rules, styles)).setCat(cat);
			}

			if (numKeys == 1 && numVals == 0)
			{
				return (new PositiveRuleK(element, zoom, selector, keys[0], rules, styles)).setCat(cat);
			}

			if (numKeys == 0 && numVals == 1)
			{
				return (new PositiveRuleV(element, zoom, selector, values[0], rules, styles)).setCat(cat);
			}

			if (numKeys == 1 && numVals == 1)
			{
				return (new PositiveRuleKV(element, zoom, selector, keys[0], values[0], rules, styles)).setCat(cat);
			}

			return (new PositiveRuleMultiKV(element, zoom, selector, keys, values, rules, styles)).setCat(cat);
		}

		public virtual RuleBuilder AddStyle(IStyle style)
		{
			styles.Add(style);
			return this;
		}

		public virtual RuleBuilder AddSubRule(RuleBuilder rule)
		{
			subRules.Add(rule);
			return this;
		}

		public virtual RuleBuilder SetStyle(params IStyle[] styles)
		{
			styleBuilder = styles;
			return this;
		}

		public virtual RuleBuilder SetRules(params RuleBuilder[] rules)
		{
			Collections.addAll(subRules, rules);
			return this;
		}

		public virtual RuleBuilder SetSelector(int selector)
		{
			this.selector = selector;
			return this;
		}

		public virtual RuleBuilder SetZoom(int zoom)
		{
			this.zoom = zoom;
			return this;
		}

		public virtual RuleBuilder SetElement(int element)
		{
			this.element = element;
			return this;
		}

		public virtual RuleBuilder SetCat(string cat)
		{
			this.cat = cat;
			return this;
		}
	}
}