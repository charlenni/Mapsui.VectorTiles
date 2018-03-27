using System.Collections.Generic;

/*
 * Copyright 2014 Hannes Janetzek
 * Copyright 2017 Longri
 * Copyright 2017 devemux86
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
namespace org.oscim.theme
{

	using GeometryType = org.oscim.core.GeometryBuffer.GeometryType;
	using TagSet = org.oscim.core.TagSet;
	using Rule = org.oscim.theme.rule.Rule;
	using Element = org.oscim.theme.rule.Rule.Element;
	using RuleVisitor = org.oscim.theme.rule.Rule.RuleVisitor;
	using RenderStyle = org.oscim.theme.styles.RenderStyle;
	using LRUCache = org.oscim.utils.LRUCache;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;


	public class RenderTheme : IRenderTheme
	{
		internal static readonly Logger log = LoggerFactory.getLogger(typeof(RenderTheme));

		private const int MATCHING_CACHE_SIZE = 512;

		private readonly float mBaseTextSize;
		private readonly int mMapBackground;

		private readonly int mLevels;
		private readonly Rule[] mRules;
		private readonly bool mMapsforgeTheme;

		internal class RenderStyleCache
		{
			private readonly RenderTheme outerInstance;

			internal readonly int matchType;
			internal readonly LRUCache<MatchingCacheKey, RenderStyleItem> cache;
			internal readonly MatchingCacheKey cacheKey;

			/* temporary matching instructions list */
			internal readonly List<RenderStyle> instructionList;

			internal RenderStyleItem prevItem;

			public RenderStyleCache(RenderTheme outerInstance, int type)
			{
				this.outerInstance = outerInstance;
				cache = new LRUCache<MatchingCacheKey, RenderStyleItem>(MATCHING_CACHE_SIZE);
				instructionList = new List<RenderStyle>(4);
				cacheKey = new MatchingCacheKey();
				matchType = type;
			}

			internal virtual RenderStyleItem RenderInstructions
			{
				get
				{
					return cache.get(cacheKey);
				}
			}
		}

		internal class RenderStyleItem
		{
			private readonly RenderTheme outerInstance;

			public RenderStyleItem(RenderTheme outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			internal RenderStyleItem next;
			internal int zoom;
			internal RenderStyle[] list;
			internal MatchingCacheKey key;
		}

		private readonly RenderStyleCache[] mStyleCache;

		public RenderTheme(int mapBackground, float baseTextSize, Rule[] rules, int levels) : this(mapBackground, baseTextSize, rules, levels, false)
		{
		}

		public RenderTheme(int mapBackground, float baseTextSize, Rule[] rules, int levels, bool mapsforgeTheme)
		{
			if (rules == null)
			{
				throw new System.ArgumentException("rules missing");
			}

			mMapBackground = mapBackground;
			mBaseTextSize = baseTextSize;
			mLevels = levels;
			mRules = rules;
			mMapsforgeTheme = mapsforgeTheme;

			mStyleCache = new RenderStyleCache[3];
			mStyleCache[0] = new RenderStyleCache(this, Rule.Element.NODE);
			mStyleCache[1] = new RenderStyleCache(this, Rule.Element.LINE);
			mStyleCache[2] = new RenderStyleCache(this, Rule.Element.POLY);
		}

		public override void dispose()
		{

			for (int i = 0; i < 3; i++)
			{
				mStyleCache[i].cache.clear();
			}

			foreach (Rule rule in mRules)
			{
				rule.dispose();
			}
		}

		public override int Levels
		{
			get
			{
				return mLevels;
			}
		}

		public override int MapBackground
		{
			get
			{
				return mMapBackground;
			}
		}

		internal virtual Rule[] Rules
		{
			get
			{
				return mRules;
			}
		}

		public override bool MapsforgeTheme
		{
			get
			{
				return mMapsforgeTheme;
			}
		}

		//AtomicInteger hitCount = new AtomicInteger(0);
		//AtomicInteger missCount = new AtomicInteger(0);
		//AtomicInteger sameCount = new AtomicInteger(0);

		public override RenderStyle[] matchElement(GeometryType geometryType, TagSet tags, int zoomLevel)
		{

			/* list of items in cache */
			RenderStyleItem ris = null;

			/* the item matching tags and zoomlevel */
			RenderStyleItem ri = null;

			int type = geometryType.nativeInt;
			if (type < 1 || type > 3)
			{
				log.debug("invalid geometry type for RenderTheme " + geometryType.name());
				return null;
			}

			RenderStyleCache cache = mStyleCache[type - 1];

			/* NOTE: maximum zoom level supported is 32 */
			int zoomMask = 1 << zoomLevel;

			lock (cache)
			{

				if ((cache.prevItem == null) || (cache.prevItem.zoom & zoomMask) == 0)
				{
					/* previous instructions zoom does not match */
					cache.cacheKey.set(tags, null);
				}
				else
				{
					/* compare if tags match previous instructions */
					if (cache.cacheKey.set(tags, cache.prevItem.key))
					{
						ri = cache.prevItem;
						//log.debug(hitCount + "/" + sameCount.incrementAndGet()
						//        + "/" + missCount + "same hit " + tags);
					}
				}

				if (ri == null)
				{
					/* get instruction for current cacheKey */
					ris = cache.RenderInstructions;

					for (ri = ris; ri != null; ri = ri.next)
					{
						if ((ri.zoom & zoomMask) != 0)
						{
							/* cache hit */

							//log.debug(hitCount.incrementAndGet()
							//       + "/" + sameCount + "/" + missCount
							//       + " cache hit " + tags);
							break;
						}
					}
				}

				if (ri == null)
				{
					/* cache miss */
					//missCount.incrementAndGet();

					IList<RenderStyle> matches = cache.instructionList;
					matches.Clear();

					foreach (Rule rule in mRules)
					{
						rule.matchElement(cache.matchType, cache.cacheKey.mTags, zoomMask, matches);
					}

					int size = matches.Count;
					if (size > 1)
					{
						for (int i = 0; i < size - 1; i++)
						{
							RenderStyle r = matches[i];
							for (int j = i + 1; j < size; j++)
							{
								if (matches[j] == r)
								{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
									log.debug("fix duplicate instruction! " + Arrays.deepToString(cache.cacheKey.mTags) + " zoom:" + zoomLevel + " " + r.GetType().FullName);
									matches.RemoveAt(j--);
									size--;
								}
							}
						}
					}
					/* check if same instructions are used in another level */
					for (ri = ris; ri != null; ri = ri.next)
					{
						if (size == 0)
						{
							if (ri.list != null)
							{
								continue;
							}

							/* both matchinglists are empty */
							break;
						}

						if (ri.list == null)
						{
							continue;
						}

						if (ri.list.Length != size)
						{
							continue;
						}

						int i = 0;
						foreach (RenderStyle r in ri.list)
						{
							if (r != matches[i])
							{
								break;
							}
							i++;
						}
						if (i == size)
							/* both matching lists contain the same items */
						{
							break;
						}
					}

					if (ri != null)
					{
						/* we found a same matchting list on another zoomlevel add
						 * this zoom level to the existing RenderInstructionItem. */
						ri.zoom |= zoomMask;

						//log.debug(zoomLevel + " same instructions " + size + " "
						//                + Arrays.deepToString(tags));
					}
					else
					{
						//log.debug(zoomLevel + " new instructions " + size + " "
						//                + Arrays.deepToString(tags));

						ri = new RenderStyleItem(this);
						ri.zoom = zoomMask;

						if (size > 0)
						{
							ri.list = new RenderStyle[size];
							matches.toArray(ri.list);
						}

						/* attach this list to the one found for MatchingKey */
						if (ris != null)
						{
							ri.next = ris.next;
							ri.key = ris.key;
							ris.next = ri;
						}
						else
						{
							ri.key = new MatchingCacheKey(cache.cacheKey);
							cache.cache.put(ri.key, ri);
						}
					}
				}
				cache.prevItem = ri;
			}
			return ri.list;
		}

		public override void scaleTextSize(float scaleFactor)
		{
			foreach (Rule rule in mRules)
			{
				rule.scaleTextSize(scaleFactor * mBaseTextSize);
			}
		}

		public override void updateStyles()
		{
			foreach (Rule rule in mRules)
			{
				rule.updateStyles();
			}
		}

		public virtual void traverseRules(Rule.RuleVisitor visitor)
		{
			foreach (Rule rule in mRules)
			{
				rule.apply(visitor);
			}
		}

	}

}