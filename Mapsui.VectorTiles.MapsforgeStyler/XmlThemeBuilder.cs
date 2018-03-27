using System;
using System.Collections.Generic;

/*
 * Copyright 2010, 2011, 2012 mapsforge.org
 * Copyright 2013 Hannes Janetzek
 * Copyright 2016-2018 devemux86
 * Copyright 2016-2017 Longri
 * Copyright 2016 Andrey Novikov
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

	using CanvasAdapter = org.oscim.backend.CanvasAdapter;
	using XMLReaderAdapter = org.oscim.backend.XMLReaderAdapter;
	using Bitmap = org.oscim.backend.canvas.Bitmap;
	using Canvas = org.oscim.backend.canvas.Canvas;
	using Color = org.oscim.backend.canvas.Color;
	using Cap = org.oscim.backend.canvas.Paint.Cap;
	using FontFamily = org.oscim.backend.canvas.Paint.FontFamily;
	using FontStyle = org.oscim.backend.canvas.Paint.FontStyle;
	using TextureAtlas = org.oscim.renderer.atlas.TextureAtlas;
	using Rect = org.oscim.renderer.atlas.TextureAtlas.Rect;
	using TextureRegion = org.oscim.renderer.atlas.TextureRegion;
	using TextureItem = org.oscim.renderer.bucket.TextureItem;
	using ThemeException = org.oscim.theme.IRenderTheme.ThemeException;
	using Rule = org.oscim.theme.rule.Rule;
	using Closed = org.oscim.theme.rule.Rule.Closed;
	using Selector = org.oscim.theme.rule.Rule.Selector;
	using RuleBuilder = org.oscim.theme.rule.RuleBuilder;
	using AreaStyle = org.oscim.theme.styles.AreaStyle;
	using AreaBuilder = org.oscim.theme.styles.AreaStyle.AreaBuilder;
	using CircleStyle = org.oscim.theme.styles.CircleStyle;
	using CircleBuilder = org.oscim.theme.styles.CircleStyle.CircleBuilder;
	using ExtrusionStyle = org.oscim.theme.styles.ExtrusionStyle;
	using ExtrusionBuilder = org.oscim.theme.styles.ExtrusionStyle.ExtrusionBuilder;
	using LineStyle = org.oscim.theme.styles.LineStyle;
	using LineBuilder = org.oscim.theme.styles.LineStyle.LineBuilder;
	using RenderStyle = org.oscim.theme.styles.RenderStyle;
	using SymbolStyle = org.oscim.theme.styles.SymbolStyle;
	using SymbolBuilder = org.oscim.theme.styles.SymbolStyle.SymbolBuilder;
	using TextStyle = org.oscim.theme.styles.TextStyle;
	using TextBuilder = org.oscim.theme.styles.TextStyle.TextBuilder;
	using Utils = org.oscim.utils.Utils;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;
	using Attributes = org.xml.sax.Attributes;
	using SAXException = org.xml.sax.SAXException;
	using SAXParseException = org.xml.sax.SAXParseException;
	using DefaultHandler = org.xml.sax.helpers.DefaultHandler;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static bool.Parse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static float.Parse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static int.Parse;

	public class XmlThemeBuilder : DefaultHandler
	{

		private static readonly Logger log = LoggerFactory.getLogger(typeof(XmlThemeBuilder));

		private const int RENDER_THEME_VERSION = 1;

		private enum Element
		{
			RENDER_THEME,
			RENDERING_INSTRUCTION,
			RULE,
			STYLE,
			ATLAS,
			RENDERING_STYLE
		}

		private const string ELEMENT_NAME_RENDER_THEME = "rendertheme";
		private const string ELEMENT_NAME_STYLE_MENU = "stylemenu";
		private const string ELEMENT_NAME_MATCH = "m";
		private const string UNEXPECTED_ELEMENT = "unexpected element: ";

		private const string LINE_STYLE = "L";
		private const string OUTLINE_STYLE = "O";
		private const string AREA_STYLE = "A";

		/// <param name="theme"> an input theme containing valid render theme XML data. </param>
		/// <returns> a new RenderTheme which is created by parsing the XML data from the input theme. </returns>
		/// <exception cref="ThemeException"> if an error occurs while parsing the render theme XML. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static IRenderTheme read(ThemeFile theme) throws org.oscim.theme.IRenderTheme.ThemeException
		public static IRenderTheme read(ThemeFile theme)
		{
			return read(theme, null);
		}

		/// <param name="theme">         an input theme containing valid render theme XML data. </param>
		/// <param name="themeCallback"> the theme callback. </param>
		/// <returns> a new RenderTheme which is created by parsing the XML data from the input theme. </returns>
		/// <exception cref="ThemeException"> if an error occurs while parsing the render theme XML. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static IRenderTheme read(ThemeFile theme, ThemeCallback themeCallback) throws org.oscim.theme.IRenderTheme.ThemeException
		public static IRenderTheme read(ThemeFile theme, ThemeCallback themeCallback)
		{
			XmlThemeBuilder renderThemeHandler = new XmlThemeBuilder(theme, themeCallback);

			try
			{
				(new XMLReaderAdapter()).parse(renderThemeHandler, theme.RenderThemeAsStream);
			}
			catch (Exception e)
			{
				throw new ThemeException(e.Message);
			}

			return renderThemeHandler.mRenderTheme;
		}

		/// <summary>
		/// Logs the given information about an unknown XML attribute.
		/// </summary>
		/// <param name="element">        the XML element name. </param>
		/// <param name="name">           the XML attribute name. </param>
		/// <param name="value">          the XML attribute value. </param>
		/// <param name="attributeIndex"> the XML attribute index position. </param>
		private static void logUnknownAttribute(string element, string name, string value, int attributeIndex)
		{
			log.debug("unknown attribute in element {} () : {} = {}", element, attributeIndex, name, value);
		}

		private readonly List<RuleBuilder> mRulesList = new List<RuleBuilder>();
		private readonly Stack<Element> mElementStack = new Stack<Element>();
		private readonly Stack<RuleBuilder> mRuleStack = new Stack<RuleBuilder>();
		private readonly Dictionary<string, RenderStyle> mStyles = new Dictionary<string, RenderStyle>(10);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.HashMap<String, org.oscim.theme.styles.TextStyle.TextBuilder<?>> mTextStyles = new java.util.HashMap<>(10);
		private readonly Dictionary<string, TextStyle.TextBuilder<object>> mTextStyles = new Dictionary<string, TextStyle.TextBuilder<object>>(10);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.oscim.theme.styles.AreaStyle.AreaBuilder<?> mAreaBuilder = org.oscim.theme.styles.AreaStyle.builder();
		private readonly AreaStyle.AreaBuilder<object> mAreaBuilder = AreaStyle.builder();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.oscim.theme.styles.CircleStyle.CircleBuilder<?> mCircleBuilder = org.oscim.theme.styles.CircleStyle.builder();
		private readonly CircleStyle.CircleBuilder<object> mCircleBuilder = CircleStyle.builder();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.oscim.theme.styles.ExtrusionStyle.ExtrusionBuilder<?> mExtrusionBuilder = org.oscim.theme.styles.ExtrusionStyle.builder();
		private readonly ExtrusionStyle.ExtrusionBuilder<object> mExtrusionBuilder = ExtrusionStyle.builder();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.oscim.theme.styles.LineStyle.LineBuilder<?> mLineBuilder = org.oscim.theme.styles.LineStyle.builder();
		private readonly LineStyle.LineBuilder<object> mLineBuilder = LineStyle.builder();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.oscim.theme.styles.SymbolStyle.SymbolBuilder<?> mSymbolBuilder = org.oscim.theme.styles.SymbolStyle.builder();
		private readonly SymbolStyle.SymbolBuilder<object> mSymbolBuilder = SymbolStyle.builder();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.oscim.theme.styles.TextStyle.TextBuilder<?> mTextBuilder = org.oscim.theme.styles.TextStyle.builder();
		private readonly TextStyle.TextBuilder<object> mTextBuilder = TextStyle.builder();

		private RuleBuilder mCurrentRule;
		private TextureAtlas mTextureAtlas;

		internal int mLevels = 0;
		internal int mMapBackground = unchecked((int)0xffffffff);
		private float mStrokeScale = 1;
		internal float mTextScale = 1;

		internal readonly ThemeFile mTheme;
		private readonly ThemeCallback mThemeCallback;
		internal RenderTheme mRenderTheme;

		private readonly float mScale, mScale2;

		private ISet<string> mCategories;
		private XmlRenderThemeStyleLayer mCurrentLayer;
		private XmlRenderThemeStyleMenu mRenderThemeStyleMenu;

		public XmlThemeBuilder(ThemeFile theme) : this(theme, null)
		{
		}

		public XmlThemeBuilder(ThemeFile theme, ThemeCallback themeCallback)
		{
			mTheme = theme;
			mThemeCallback = themeCallback;
			mScale = CanvasAdapter.Scale;
			mScale2 = CanvasAdapter.Scale * 0.5f;
		}

		public override void endDocument()
		{
			Rule[] rules = new Rule[mRulesList.Count];
			for (int i = 0, n = rules.Length; i < n; i++)
			{
				rules[i] = mRulesList[i].onComplete(null);
			}

			mRenderTheme = createTheme(rules);

			mRulesList.Clear();
			mStyles.Clear();
			mRuleStack.Clear();
			mElementStack.Clear();

			mTextureAtlas = null;
		}

		internal virtual RenderTheme createTheme(Rule[] rules)
		{
			return new RenderTheme(mMapBackground, mTextScale, rules, mLevels);
		}

		public override void endElement(string uri, string localName, string qName)
		{
			mElementStack.Pop();

			if (ELEMENT_NAME_MATCH.Equals(localName))
			{
				mRuleStack.Pop();
				if (mRuleStack.Count == 0)
				{
					if (isVisible(mCurrentRule))
					{
						mRulesList.Add(mCurrentRule);
					}
				}
				else
				{
					mCurrentRule = mRuleStack.Peek();
				}
			}
			else if (ELEMENT_NAME_STYLE_MENU.Equals(localName))
			{
				// when we are finished parsing the menu part of the file, we can get the
				// categories to render from the initiator. This allows the creating action
				// to select which of the menu options to choose
				if (null != mTheme.MenuCallback)
				{
					// if there is no callback, there is no menu, so the categories will be null
					mCategories = mTheme.MenuCallback.getCategories(mRenderThemeStyleMenu);
				}
			}
		}

		public override void error(SAXParseException exception)
		{
			log.debug(exception.Message);
		}

		public override void warning(SAXParseException exception)
		{
			log.debug(exception.Message);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void startElement(String uri, String localName, String qName, org.xml.sax.Attributes attributes) throws org.oscim.theme.IRenderTheme.ThemeException
		public override void startElement(string uri, string localName, string qName, Attributes attributes)
		{
			try
			{
				if (ELEMENT_NAME_RENDER_THEME.Equals(localName))
				{
					checkState(localName, Element.RENDER_THEME);
					createRenderTheme(localName, attributes);

				}
				else if (ELEMENT_NAME_MATCH.Equals(localName))
				{
					checkState(localName, Element.RULE);
					RuleBuilder rule = createRule(localName, attributes);
					if (mRuleStack.Count > 0 && isVisible(rule))
					{
						mCurrentRule.addSubRule(rule);
					}
					mCurrentRule = rule;
					mRuleStack.Push(mCurrentRule);

				}
				else if ("style-text".Equals(localName))
				{
					checkState(localName, Element.STYLE);
					handleTextElement(localName, attributes, true, false);

				}
				else if ("style-area".Equals(localName))
				{
					checkState(localName, Element.STYLE);
					handleAreaElement(localName, attributes, true);

				}
				else if ("style-line".Equals(localName))
				{
					checkState(localName, Element.STYLE);
					handleLineElement(localName, attributes, true, false);

				}
				else if ("outline-layer".Equals(localName))
				{
					checkState(localName, Element.RENDERING_INSTRUCTION);
					LineStyle line = createLine(null, localName, attributes, mLevels++, true, false);
					mStyles[OUTLINE_STYLE + line.style] = line;

				}
				else if ("area".Equals(localName))
				{
					checkState(localName, Element.RENDERING_INSTRUCTION);
					handleAreaElement(localName, attributes, false);

				}
				else if ("caption".Equals(localName))
				{
					checkState(localName, Element.RENDERING_INSTRUCTION);
					handleTextElement(localName, attributes, false, true);

				}
				else if ("circle".Equals(localName))
				{
					checkState(localName, Element.RENDERING_INSTRUCTION);
					CircleStyle circle = createCircle(localName, attributes, mLevels++);
					if (isVisible(circle))
					{
						mCurrentRule.addStyle(circle);
					}

				}
				else if ("line".Equals(localName))
				{
					checkState(localName, Element.RENDERING_INSTRUCTION);
					handleLineElement(localName, attributes, false, false);

				}
				else if ("text".Equals(localName) || "pathText".Equals(localName))
				{
					checkState(localName, Element.RENDERING_INSTRUCTION);
					handleTextElement(localName, attributes, false, false);

				}
				else if ("symbol".Equals(localName))
				{
					checkState(localName, Element.RENDERING_INSTRUCTION);
					SymbolStyle symbol = createSymbol(localName, attributes);
					if (symbol != null && isVisible(symbol))
					{
						mCurrentRule.addStyle(symbol);
					}

				}
				else if ("outline".Equals(localName))
				{
					checkState(localName, Element.RENDERING_INSTRUCTION);
					LineStyle outline = createOutline(attributes.getValue("use"), attributes);
					if (outline != null && isVisible(outline))
					{
						mCurrentRule.addStyle(outline);
					}

				}
				else if ("extrusion".Equals(localName))
				{
					checkState(localName, Element.RENDERING_INSTRUCTION);
					ExtrusionStyle extrusion = createExtrusion(localName, attributes, mLevels++);
					if (isVisible(extrusion))
					{
						mCurrentRule.addStyle(extrusion);
					}

				}
				else if ("lineSymbol".Equals(localName))
				{
					checkState(localName, Element.RENDERING_INSTRUCTION);
					handleLineElement(localName, attributes, false, true);

				}
				else if ("atlas".Equals(localName))
				{
					checkState(localName, Element.ATLAS);
					createAtlas(localName, attributes);

				}
				else if ("rect".Equals(localName))
				{
					checkState(localName, Element.ATLAS);
					createTextureRegion(localName, attributes);

				}
				else if ("cat".Equals(localName))
				{
					checkState(qName, Element.RENDERING_STYLE);
					mCurrentLayer.addCategory(getStringAttribute(attributes, "id"));

				}
				else if ("layer".Equals(localName))
				{
					// render theme menu layer
					checkState(qName, Element.RENDERING_STYLE);
					bool enabled = false;
					if (!string.ReferenceEquals(getStringAttribute(attributes, "enabled"), null))
					{
						enabled = Convert.ToBoolean(getStringAttribute(attributes, "enabled"));
					}
					bool visible = Convert.ToBoolean(getStringAttribute(attributes, "visible"));
					mCurrentLayer = mRenderThemeStyleMenu.createLayer(getStringAttribute(attributes, "id"), visible, enabled);
					string parent = getStringAttribute(attributes, "parent");
					if (null != parent)
					{
						XmlRenderThemeStyleLayer parentEntry = mRenderThemeStyleMenu.getLayer(parent);
						if (null != parentEntry)
						{
							foreach (string cat in parentEntry.Categories)
							{
								mCurrentLayer.addCategory(cat);
							}
							foreach (XmlRenderThemeStyleLayer overlay in parentEntry.Overlays)
							{
								mCurrentLayer.addOverlay(overlay);
							}
						}
					}

				}
				else if ("name".Equals(localName))
				{
					// render theme menu name
					checkState(qName, Element.RENDERING_STYLE);
					mCurrentLayer.addTranslation(getStringAttribute(attributes, "lang"), getStringAttribute(attributes, "value"));

				}
				else if ("overlay".Equals(localName))
				{
					// render theme menu overlay
					checkState(qName, Element.RENDERING_STYLE);
					XmlRenderThemeStyleLayer overlay = mRenderThemeStyleMenu.getLayer(getStringAttribute(attributes, "id"));
					if (overlay != null)
					{
						mCurrentLayer.addOverlay(overlay);
					}

				}
				else if ("stylemenu".Equals(localName))
				{
					checkState(qName, Element.RENDERING_STYLE);
					mRenderThemeStyleMenu = new XmlRenderThemeStyleMenu(getStringAttribute(attributes, "id"), getStringAttribute(attributes, "defaultlang"), getStringAttribute(attributes, "defaultvalue"));

				}
				else
				{
					log.error("unknown element: {}", localName);
					throw new SAXException("unknown element: " + localName);
				}
			}
			catch (SAXException e)
			{
				throw new ThemeException(e.Message);
			}
			catch (IOException e)
			{
				throw new ThemeException(e.Message);
			}
		}

		private RuleBuilder createRule(string localName, Attributes attributes)
		{
			string cat = null;
			int element = Rule.Element.ANY;
			int closed = Rule.Closed.ANY;
			string keys = null;
			string values = null;
			sbyte zoomMin = 0;
			sbyte zoomMax = sbyte.MaxValue;
			int selector = 0;

			for (int i = 0; i < attributes.Length; i++)
			{
				string name = attributes.getLocalName(i);
				string value = attributes.getValue(i);

				if ("e".Equals(name))
				{
					string val = value.ToUpper(Locale.ENGLISH);
					if ("WAY".Equals(val))
					{
						element = Rule.Element.WAY;
					}
					else if ("NODE".Equals(val))
					{
						element = Rule.Element.NODE;
					}
				}
				else if ("k".Equals(name))
				{
					keys = value;
				}
				else if ("v".Equals(name))
				{
					values = value;
				}
				else if ("cat".Equals(name))
				{
					cat = value;
				}
				else if ("closed".Equals(name))
				{
					string val = value.ToUpper(Locale.ENGLISH);
					if ("YES".Equals(val))
					{
						closed = Rule.Closed.YES;
					}
					else if ("NO".Equals(val))
					{
						closed = Rule.Closed.NO;
					}
				}
				else if ("zoom-min".Equals(name))
				{
					zoomMin = sbyte.Parse(value);
				}
				else if ("zoom-max".Equals(name))
				{
					zoomMax = sbyte.Parse(value);
				}
				else if ("select".Equals(name))
				{
					if ("first".Equals(value))
					{
						selector |= Rule.Selector.FIRST;
					}
					if ("when-matched".Equals(value))
					{
						selector |= Rule.Selector.WHEN_MATCHED;
					}
				}
				else
				{
					logUnknownAttribute(localName, name, value, i);
				}
			}

			if (closed == Rule.Closed.YES)
			{
				element = Rule.Element.POLY;
			}
			else if (closed == Rule.Closed.NO)
			{
				element = Rule.Element.LINE;
			}

			validateNonNegative("zoom-min", zoomMin);
			validateNonNegative("zoom-max", zoomMax);
			if (zoomMin > zoomMax)
			{
				throw new ThemeException("zoom-min must be less or equal zoom-max: " + zoomMin);
			}

			RuleBuilder b = RuleBuilder.create(keys, values);
			b.cat(cat);
			b.zoom(zoomMin, zoomMax);
			b.element(element);
			b.select(selector);
			return b;
		}

		private TextureRegion getAtlasRegion(string src)
		{
			if (mTextureAtlas == null)
			{
				return null;
			}

			TextureRegion texture = mTextureAtlas.getTextureRegion(src);

			if (texture == null)
			{
				log.debug("missing texture atlas item '" + src + "'");
			}

			return texture;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void handleLineElement(String localName, org.xml.sax.Attributes attributes, boolean isStyle, boolean hasSymbol) throws org.xml.sax.SAXException
		private void handleLineElement(string localName, Attributes attributes, bool isStyle, bool hasSymbol)
		{

			string use = attributes.getValue("use");
			LineStyle style = null;

			if (!string.ReferenceEquals(use, null))
			{
				style = (LineStyle) mStyles[LINE_STYLE + use];
				if (style == null)
				{
					log.debug("missing line style 'use': " + use);
					return;
				}
			}

			LineStyle line = createLine(style, localName, attributes, mLevels++, false, hasSymbol);

			if (isStyle)
			{
				mStyles[LINE_STYLE + line.style] = line;
			}
			else
			{
				if (isVisible(line))
				{
					mCurrentRule.addStyle(line);
					/* Note 'outline' will not be inherited, it's just a
					 * shortcut to add the outline RenderInstruction. */
					string outlineValue = attributes.getValue("outline");
					if (!string.ReferenceEquals(outlineValue, null))
					{
						LineStyle outline = createOutline(outlineValue, attributes);
						if (outline != null)
						{
							mCurrentRule.addStyle(outline);
						}
					}
				}
			}
		}

		/// <param name="line">      optional: line style defaults </param>
		/// <param name="level">     the drawing level of this instruction. </param>
		/// <param name="isOutline"> is outline layer </param>
		/// <returns> a new Line with the given rendering attributes. </returns>
		private LineStyle createLine(LineStyle line, string elementName, Attributes attributes, int level, bool isOutline, bool hasSymbol)
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.oscim.theme.styles.LineStyle.LineBuilder<?> b = mLineBuilder.set(line);
			LineStyle.LineBuilder<object> b = mLineBuilder.set(line);
			b.isOutline(isOutline);
			b.level(level);
			b.themeCallback(mThemeCallback);
			string src = null;

			for (int i = 0; i < attributes.Length; i++)
			{
				string name = attributes.getLocalName(i);
				string value = attributes.getValue(i);

				if ("id".Equals(name))
				{
					b.style = value;
				}

				else if ("cat".Equals(name))
				{
					b.cat(value);
				}

				else if ("src".Equals(name))
				{
					src = value;
				}

				else if ("use".Equals(name))
				{
					; // ignore
				}

				else if ("outline".Equals(name))
				{
					; // ignore
				}

				else if ("stroke".Equals(name))
				{
					b.color(value);
				}

				else if ("width".Equals(name) || "stroke-width".Equals(name))
				{
					b.strokeWidth = parseFloat(value) * mScale2 * mStrokeScale;
					if (line == null)
					{
						if (!isOutline)
						{
							validateNonNegative("width", b.strokeWidth);
						}
					}
					else
					{
						/* use stroke width relative to 'line' */
						b.strokeWidth += line.width;
						if (b.strokeWidth <= 0)
						{
							b.strokeWidth = 1;
						}
					}
				}
				else if ("cap".Equals(name) || "stroke-linecap".Equals(name))
				{
					b.cap = Cap.valueOf(value.ToUpper(Locale.ENGLISH));
				}

				else if ("fix".Equals(name))
				{
					b.@fixed = parseBoolean(value);
				}

				else if ("stipple".Equals(name))
				{
					b.stipple = Math.Round(parseInt(value) * mScale2 * mStrokeScale);
				}

				else if ("stipple-stroke".Equals(name))
				{
					b.stippleColor(value);
				}

				else if ("stipple-width".Equals(name))
				{
					b.stippleWidth = parseFloat(value);
				}

				else if ("fade".Equals(name))
				{
					b.fadeScale = int.Parse(value);
				}

				else if ("min".Equals(name))
				{
					; //min = Float.parseFloat(value);
				}

				else if ("blur".Equals(name))
				{
					b.blur = parseFloat(value);
				}

				else if ("style".Equals(name))
				{
					; // ignore
				}

				else if ("dasharray".Equals(name) || "stroke-dasharray".Equals(name))
				{
					b.dashArray = parseFloatArray(value);
					for (int j = 0; j < b.dashArray.length; ++j)
					{
						b.dashArray[j] = b.dashArray[j] * mScale * mStrokeScale;
					}

				}
				else if ("symbol-width".Equals(name))
				{
					b.symbolWidth = (int)(int.Parse(value) * mScale);
				}

				else if ("symbol-height".Equals(name))
				{
					b.symbolHeight = (int)(int.Parse(value) * mScale);
				}

				else if ("symbol-percent".Equals(name))
				{
					b.symbolPercent = int.Parse(value);
				}

				else if ("symbol-scaling".Equals(name))
				{
					; // no-op
				}

				else if ("repeat-start".Equals(name))
				{
					b.repeatStart = float.Parse(value) * mScale;
				}

				else if ("repeat-gap".Equals(name))
				{
					b.repeatGap = float.Parse(value) * mScale;
				}

				else
				{
					logUnknownAttribute(elementName, name, value, i);
				}
			}

			if (b.dashArray != null)
			{
				// Stroke dash array
				if (b.dashArray.length % 2 != 0)
				{
					// Odd number of entries is duplicated
					float[] newDashArray = new float[b.dashArray.length * 2];
					Array.Copy(b.dashArray, 0, newDashArray, 0, b.dashArray.length);
					Array.Copy(b.dashArray, 0, newDashArray, b.dashArray.length, b.dashArray.length);
					b.dashArray = newDashArray;
				}
				int width = 0;
				int height = (int)(b.strokeWidth);
				if (height < 1)
				{
					height = 1;
				}
				foreach (float f in b.dashArray)
				{
					if (f < 1)
					{
						f = 1;
					}
					width += (int)f;
				}
				Bitmap bitmap = CanvasAdapter.newBitmap(width, height, 0);
				Canvas canvas = CanvasAdapter.newCanvas();
				canvas.Bitmap = bitmap;
				int x = 0;
				bool transparent = false;
				foreach (float f in b.dashArray)
				{
					if (f < 1)
					{
						f = 1;
					}
					canvas.fillRectangle(x, 0, f, height, transparent ? Color.TRANSPARENT : Color.WHITE);
					x += (int)f;
					transparent = !transparent;
				}
				b.texture = new TextureItem(Utils.potBitmap(bitmap));
				b.texture.mipmap = true;
				b.randomOffset = false;
				b.stipple = width;
				b.stippleWidth = 1;
				b.stippleColor = b.fillColor;
			}
			else
			{
				b.texture = Utils.loadTexture(mTheme.RelativePathPrefix, src, b.symbolWidth, b.symbolHeight, b.symbolPercent);

				if (hasSymbol)
				{
					// Line symbol
					int width = (int)(b.texture.width + b.repeatGap);
					int height = b.texture.height;
					Bitmap bitmap = CanvasAdapter.newBitmap(width, height, 0);
					Canvas canvas = CanvasAdapter.newCanvas();
					canvas.Bitmap = bitmap;
					canvas.drawBitmap(b.texture.bitmap, b.repeatStart, 0);
					b.texture = new TextureItem(Utils.potBitmap(bitmap));
					b.texture.mipmap = true;
					b.@fixed = true;
					b.randomOffset = false;
					b.stipple = width;
					b.stippleWidth = 1;
					b.strokeWidth = height * 0.5f;
					b.stippleColor = Color.WHITE;
				}
			}

			return b.build();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void handleAreaElement(String localName, org.xml.sax.Attributes attributes, boolean isStyle) throws org.xml.sax.SAXException
		private void handleAreaElement(string localName, Attributes attributes, bool isStyle)
		{

			string use = attributes.getValue("use");
			AreaStyle style = null;

			if (!string.ReferenceEquals(use, null))
			{
				style = (AreaStyle) mStyles[AREA_STYLE + use];
				if (style == null)
				{
					log.debug("missing area style 'use': " + use);
					return;
				}
			}

			AreaStyle area = createArea(style, localName, attributes, mLevels++);

			if (isStyle)
			{
				mStyles[AREA_STYLE + area.style] = area;
			}
			else
			{
				if (isVisible(area))
				{
					mCurrentRule.addStyle(area);
				}
			}
		}

		/// <returns> a new Area with the given rendering attributes. </returns>
		private AreaStyle createArea(AreaStyle area, string elementName, Attributes attributes, int level)
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.oscim.theme.styles.AreaStyle.AreaBuilder<?> b = mAreaBuilder.set(area);
			AreaStyle.AreaBuilder<object> b = mAreaBuilder.set(area);
			b.level(level);
			b.themeCallback(mThemeCallback);
			string src = null;

			for (int i = 0; i < attributes.Length; i++)
			{
				string name = attributes.getLocalName(i);
				string value = attributes.getValue(i);

				if ("id".Equals(name))
				{
					b.style = value;
				}

				else if ("cat".Equals(name))
				{
					b.cat(value);
				}

				else if ("use".Equals(name))
				{
					; // ignore
				}

				else if ("src".Equals(name))
				{
					src = value;
				}

				else if ("fill".Equals(name))
				{
					b.color(value);
				}

				else if ("stroke".Equals(name))
				{
					b.strokeColor(value);
				}

				else if ("stroke-width".Equals(name))
				{
					float strokeWidth = float.Parse(value);
					validateNonNegative("stroke-width", strokeWidth);
					b.strokeWidth = strokeWidth * mScale * mStrokeScale;

				}
				else if ("fade".Equals(name))
				{
					b.fadeScale = int.Parse(value);
				}

				else if ("blend".Equals(name))
				{
					b.blendScale = int.Parse(value);
				}

				else if ("blend-fill".Equals(name))
				{
					b.blendColor(value);
				}

				else if ("mesh".Equals(name))
				{
					b.mesh(bool.Parse(value));
				}

				else if ("symbol-width".Equals(name))
				{
					b.symbolWidth = (int)(int.Parse(value) * mScale);
				}

				else if ("symbol-height".Equals(name))
				{
					b.symbolHeight = (int)(int.Parse(value) * mScale);
				}

				else if ("symbol-percent".Equals(name))
				{
					b.symbolPercent = int.Parse(value);
				}

				else if ("symbol-scaling".Equals(name))
				{
					; // no-op
				}

				else
				{
					logUnknownAttribute(elementName, name, value, i);
				}
			}

			b.texture = Utils.loadTexture(mTheme.RelativePathPrefix, src, b.symbolWidth, b.symbolHeight, b.symbolPercent);

			return b.build();
		}

		private LineStyle createOutline(string style, Attributes attributes)
		{
			if (!string.ReferenceEquals(style, null))
			{
				LineStyle line = (LineStyle) mStyles[OUTLINE_STYLE + style];
				if (line != null && line.outline)
				{
					string cat = null;

					for (int i = 0; i < attributes.Length; i++)
					{
						string name = attributes.getLocalName(i);
						string value = attributes.getValue(i);

						if ("cat".Equals(name))
						{
							cat = value;
							break;
						}
					}

					return line.setCat(cat);
				}
			}
			log.debug("BUG not an outline style: " + style);
			return null;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void createAtlas(String elementName, org.xml.sax.Attributes attributes) throws java.io.IOException
		private void createAtlas(string elementName, Attributes attributes)
		{
			string img = null;

			for (int i = 0; i < attributes.Length; i++)
			{
				string name = attributes.getLocalName(i);
				string value = attributes.getValue(i);

				if ("img".Equals(name))
				{
					img = value;
				}
				else
				{
					logUnknownAttribute(elementName, name, value, i);
				}
			}
			validateExists("img", img, elementName);

			Bitmap bitmap = CanvasAdapter.getBitmapAsset(mTheme.RelativePathPrefix, img);
			if (bitmap != null)
			{
				mTextureAtlas = new TextureAtlas(bitmap);
			}
		}

		private void createTextureRegion(string elementName, Attributes attributes)
		{
			if (mTextureAtlas == null)
			{
				return;
			}

			string regionName = null;
			TextureAtlas.Rect r = null;

			for (int i = 0, n = attributes.Length; i < n; i++)
			{
				string name = attributes.getLocalName(i);
				string value = attributes.getValue(i);

				if ("id".Equals(name))
				{
					regionName = value;
				}
				else if ("pos".Equals(name))
				{
					string[] pos = value.Split(" ", true);
					if (pos.Length == 4)
					{
						r = new TextureAtlas.Rect(int.Parse(pos[0]), int.Parse(pos[1]), int.Parse(pos[2]), int.Parse(pos[3]));
					}
				}
				else
				{
					logUnknownAttribute(elementName, name, value, i);
				}
			}
			validateExists("id", regionName, elementName);
			validateExists("pos", r, elementName);

			mTextureAtlas.addTextureRegion(regionName.intern(), r);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void checkElement(String elementName, Element element) throws org.xml.sax.SAXException
		private void checkElement(string elementName, Element element)
		{
			Element parentElement;
			switch (element)
			{
				case org.oscim.theme.XmlThemeBuilder.Element.RENDER_THEME:
					if (mElementStack.Count > 0)
					{
						throw new SAXException(UNEXPECTED_ELEMENT + elementName);
					}
					return;

				case org.oscim.theme.XmlThemeBuilder.Element.RULE:
					parentElement = mElementStack.Peek();
					if (parentElement != Element.RENDER_THEME && parentElement != Element.RULE)
					{
						throw new SAXException(UNEXPECTED_ELEMENT + elementName);
					}
					return;

				case org.oscim.theme.XmlThemeBuilder.Element.STYLE:
					parentElement = mElementStack.Peek();
					if (parentElement != Element.RENDER_THEME)
					{
						throw new SAXException(UNEXPECTED_ELEMENT + elementName);
					}
					return;

				case org.oscim.theme.XmlThemeBuilder.Element.RENDERING_INSTRUCTION:
					if (mElementStack.Peek() != Element.RULE)
					{
						throw new SAXException(UNEXPECTED_ELEMENT + elementName);
					}
					return;

				case org.oscim.theme.XmlThemeBuilder.Element.ATLAS:
					parentElement = mElementStack.Peek();
					// FIXME
					if (parentElement != Element.RENDER_THEME && parentElement != Element.ATLAS)
					{
						throw new SAXException(UNEXPECTED_ELEMENT + elementName);
					}
					return;

				case org.oscim.theme.XmlThemeBuilder.Element.RENDERING_STYLE:
					return;
			}

			throw new SAXException("unknown enum value: " + element);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void checkState(String elementName, Element element) throws org.xml.sax.SAXException
		private void checkState(string elementName, Element element)
		{
			checkElement(elementName, element);
			mElementStack.Push(element);
		}

		private void createRenderTheme(string elementName, Attributes attributes)
		{
			int? version = null;
			int mapBackground = Color.WHITE;
			float baseStrokeWidth = 1;
			float baseTextScale = 1;

			for (int i = 0; i < attributes.Length; ++i)
			{
				string name = attributes.getLocalName(i);
				string value = attributes.getValue(i);

				if ("schemaLocation".Equals(name))
				{
					continue;
				}

				if ("version".Equals(name))
				{
					version = int.Parse(value);
				}

				else if ("map-background".Equals(name))
				{
					mapBackground = Color.parseColor(value);
					if (mThemeCallback != null)
					{
						mapBackground = mThemeCallback.getColor(mapBackground);
					}

				}
				else if ("base-stroke-width".Equals(name))
				{
					baseStrokeWidth = float.Parse(value);
				}

				else if ("base-text-scale".Equals(name) || "base-text-size".Equals(name))
				{
					baseTextScale = float.Parse(value);
				}

				else
				{
					logUnknownAttribute(elementName, name, value, i);
				}

			}

			validateExists("version", version, elementName);

			if (version > RENDER_THEME_VERSION)
			{
				throw new ThemeException("invalid render theme version:" + version);
			}

			validateNonNegative("base-stroke-width", baseStrokeWidth);
			validateNonNegative("base-text-scale", baseTextScale);

			mMapBackground = mapBackground;
			mStrokeScale = baseStrokeWidth;
			mTextScale = baseTextScale;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void handleTextElement(String localName, org.xml.sax.Attributes attributes, boolean isStyle, boolean isCaption) throws org.xml.sax.SAXException
		private void handleTextElement(string localName, Attributes attributes, bool isStyle, bool isCaption)
		{

			string style = attributes.getValue("use");
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.oscim.theme.styles.TextStyle.TextBuilder<?> pt = null;
			TextStyle.TextBuilder<object> pt = null;

			if (!string.ReferenceEquals(style, null))
			{
				pt = mTextStyles[style];
				if (pt == null)
				{
					log.debug("missing text style: " + style);
					return;
				}
			}

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.oscim.theme.styles.TextStyle.TextBuilder<?> b = createText(localName, attributes, isCaption, pt);
			TextStyle.TextBuilder<object> b = createText(localName, attributes, isCaption, pt);
			if (isStyle)
			{
				log.debug("put style {}", b.style);
				mTextStyles[b.style] = TextStyle.builder().from(b);
			}
			else
			{
				TextStyle text = b.buildInternal();
				if (isVisible(text))
				{
					mCurrentRule.addStyle(text);
				}
			}
		}

		/// <param name="caption"> ... </param>
		/// <returns> a new Text with the given rendering attributes. </returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.oscim.theme.styles.TextStyle.TextBuilder<?> createText(String elementName, org.xml.sax.Attributes attributes, boolean caption, org.oscim.theme.styles.TextStyle.TextBuilder<?> style)
		private TextStyle.TextBuilder<object> createText<T1>(string elementName, Attributes attributes, bool caption, TextStyle.TextBuilder<T1> style)
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.oscim.theme.styles.TextStyle.TextBuilder<?> b;
			TextStyle.TextBuilder<object> b;
			if (style == null)
			{
				b = mTextBuilder.reset();
				b.caption = caption;
			}
			else
			{
				b = mTextBuilder.from(style);
			}
			b.themeCallback(mThemeCallback);
			string symbol = null;

			for (int i = 0; i < attributes.Length; i++)
			{
				string name = attributes.getLocalName(i);
				string value = attributes.getValue(i);

				if ("id".Equals(name))
				{
					b.style = value;
				}

				else if ("cat".Equals(name))
				{
					b.cat(value);
				}

				else if ("k".Equals(name))
				{
					b.textKey = value.intern();
				}

				else if ("font-family".Equals(name))
				{
					b.fontFamily = FontFamily.valueOf(value.ToUpper(Locale.ENGLISH));
				}

				else if ("style".Equals(name) || "font-style".Equals(name))
				{
					b.fontStyle = FontStyle.valueOf(value.ToUpper(Locale.ENGLISH));
				}

				else if ("size".Equals(name) || "font-size".Equals(name))
				{
					b.fontSize = float.Parse(value);
				}

				else if ("fill".Equals(name))
				{
					b.fillColor = Color.parseColor(value);
				}

				else if ("stroke".Equals(name))
				{
					b.strokeColor = Color.parseColor(value);
				}

				else if ("stroke-width".Equals(name))
				{
					b.strokeWidth = float.Parse(value) * mScale;
				}

				else if ("caption".Equals(name))
				{
					b.caption = bool.Parse(value);
				}

				else if ("priority".Equals(name))
				{
					b.priority = int.Parse(value);
				}

				else if ("area-size".Equals(name))
				{
					b.areaSize = float.Parse(value);
				}

				else if ("dy".Equals(name))
				{
					// NB: minus..
					b.dy = -float.Parse(value) * mScale;
				}

				else if ("symbol".Equals(name))
				{
					symbol = value;
				}

				else if ("use".Equals(name))
				{
					; // ignore
				}

				else if ("symbol-width".Equals(name))
				{
					b.symbolWidth = (int)(int.Parse(value) * mScale);
				}

				else if ("symbol-height".Equals(name))
				{
					b.symbolHeight = (int)(int.Parse(value) * mScale);
				}

				else if ("symbol-percent".Equals(name))
				{
					b.symbolPercent = int.Parse(value);
				}

				else if ("symbol-scaling".Equals(name))
				{
					; // no-op
				}

				else
				{
					logUnknownAttribute(elementName, name, value, i);
				}
			}

			validateExists("k", b.textKey, elementName);
			validateNonNegative("size", b.fontSize);
			validateNonNegative("stroke-width", b.strokeWidth);

			if (!string.ReferenceEquals(symbol, null) && symbol.Length > 0)
			{
				string lowValue = symbol.ToLower(Locale.ENGLISH);
				if (lowValue.EndsWith(".png", StringComparison.Ordinal) || lowValue.EndsWith(".svg", StringComparison.Ordinal))
				{
					try
					{
						b.bitmap = CanvasAdapter.getBitmapAsset(mTheme.RelativePathPrefix, symbol, b.symbolWidth, b.symbolHeight, b.symbolPercent);
					}
					catch (Exception e)
					{
						log.error("{}: {}", symbol, e.Message);
					}
				}
				else
				{
					b.texture = getAtlasRegion(symbol);
				}
			}

			return b;
		}

		/// <param name="level"> the drawing level of this instruction. </param>
		/// <returns> a new Circle with the given rendering attributes. </returns>
		private CircleStyle createCircle(string elementName, Attributes attributes, int level)
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.oscim.theme.styles.CircleStyle.CircleBuilder<?> b = mCircleBuilder.reset();
			CircleStyle.CircleBuilder<object> b = mCircleBuilder.reset();
			b.level(level);
			b.themeCallback(mThemeCallback);

			for (int i = 0; i < attributes.Length; i++)
			{
				string name = attributes.getLocalName(i);
				string value = attributes.getValue(i);

				if ("r".Equals(name) || "radius".Equals(name))
				{
					b.radius(float.Parse(value) * mScale * mStrokeScale);
				}

				else if ("cat".Equals(name))
				{
					b.cat(value);
				}

				else if ("scale-radius".Equals(name))
				{
					b.scaleRadius(bool.Parse(value));
				}

				else if ("fill".Equals(name))
				{
					b.color(Color.parseColor(value));
				}

				else if ("stroke".Equals(name))
				{
					b.strokeColor(Color.parseColor(value));
				}

				else if ("stroke-width".Equals(name))
				{
					b.strokeWidth(float.Parse(value) * mScale * mStrokeScale);
				}

				else
				{
					logUnknownAttribute(elementName, name, value, i);
				}
			}

			validateExists("radius", b.radius, elementName);
			validateNonNegative("radius", b.radius);
			validateNonNegative("stroke-width", b.strokeWidth);

			return b.build();
		}

		/// <returns> a new Symbol with the given rendering attributes. </returns>
		private SymbolStyle createSymbol(string elementName, Attributes attributes)
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.oscim.theme.styles.SymbolStyle.SymbolBuilder<?> b = mSymbolBuilder.reset();
			SymbolStyle.SymbolBuilder<object> b = mSymbolBuilder.reset();
			string src = null;

			for (int i = 0; i < attributes.Length; i++)
			{
				string name = attributes.getLocalName(i);
				string value = attributes.getValue(i);

				if ("src".Equals(name))
				{
					src = value;
				}

				else if ("cat".Equals(name))
				{
					b.cat(value);
				}

				else if ("symbol-width".Equals(name))
				{
					b.symbolWidth = (int)(int.Parse(value) * mScale);
				}

				else if ("symbol-height".Equals(name))
				{
					b.symbolHeight = (int)(int.Parse(value) * mScale);
				}

				else if ("symbol-percent".Equals(name))
				{
					b.symbolPercent = int.Parse(value);
				}

				else if ("symbol-scaling".Equals(name))
				{
					; // no-op
				}

				else if ("repeat".Equals(name))
				{
					b.repeat(bool.Parse(value));
				}

				else if ("repeat-start".Equals(name))
				{
					b.repeatStart = float.Parse(value) * mScale;
				}

				else if ("repeat-gap".Equals(name))
				{
					b.repeatGap = float.Parse(value) * mScale;
				}

				else
				{
					logUnknownAttribute(elementName, name, value, i);
				}
			}

			validateExists("src", src, elementName);

			string lowSrc = src.ToLower(Locale.ENGLISH);
			if (lowSrc.EndsWith(".png", StringComparison.Ordinal) || lowSrc.EndsWith(".svg", StringComparison.Ordinal))
			{
				try
				{
					Bitmap bitmap = CanvasAdapter.getBitmapAsset(mTheme.RelativePathPrefix, src, b.symbolWidth, b.symbolHeight, b.symbolPercent);
					if (bitmap != null)
					{
						return buildSymbol(b, src, bitmap);
					}
				}
				catch (Exception e)
				{
					log.error("{}: {}", src, e.Message);
				}
				return null;
			}
			return b.texture(getAtlasRegion(src)).build();
		}

		internal virtual SymbolStyle buildSymbol<T1>(SymbolStyle.SymbolBuilder<T1> b, string src, Bitmap bitmap)
		{
			return b.bitmap(bitmap).build();
		}

		private ExtrusionStyle createExtrusion(string elementName, Attributes attributes, int level)
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.oscim.theme.styles.ExtrusionStyle.ExtrusionBuilder<?> b = mExtrusionBuilder.reset();
			ExtrusionStyle.ExtrusionBuilder<object> b = mExtrusionBuilder.reset();
			b.level(level);
			b.themeCallback(mThemeCallback);

			for (int i = 0; i < attributes.Length; ++i)
			{
				string name = attributes.getLocalName(i);
				string value = attributes.getValue(i);

				if ("cat".Equals(name))
				{
					b.cat(value);
				}

				else if ("side-color".Equals(name))
				{
					b.colorSide(Color.parseColor(value));
				}

				else if ("top-color".Equals(name))
				{
					b.colorTop(Color.parseColor(value));
				}

				else if ("line-color".Equals(name))
				{
					b.colorLine(Color.parseColor(value));
				}

				else if ("default-height".Equals(name))
				{
					b.defaultHeight(int.Parse(value));
				}

				else
				{
					logUnknownAttribute(elementName, name, value, i);
				}
			}

			return b.build();
		}

		private string getStringAttribute(Attributes attributes, string name)
		{
			for (int i = 0; i < attributes.Length; ++i)
			{
				if (attributes.getLocalName(i).Equals(name))
				{
					return attributes.getValue(i);
				}
			}
			return null;
		}

		/// <summary>
		/// A style is visible if categories is not set or the style has no category
		/// or the categories contain the style's category.
		/// </summary>
		private bool isVisible(RenderStyle renderStyle)
		{
			return mCategories == null || renderStyle.cat == null || mCategories.Contains(renderStyle.cat);
		}

		/// <summary>
		/// A rule is visible if categories is not set or the rule has no category
		/// or the categories contain the rule's category.
		/// </summary>
		private bool isVisible(RuleBuilder rule)
		{
			return mCategories == null || rule.cat == null || mCategories.Contains(rule.cat);
		}

		private static float[] parseFloatArray(string dashString)
		{
			string[] dashEntries = dashString.Split(",", true);
			float[] dashIntervals = new float[dashEntries.Length];
			for (int i = 0; i < dashEntries.Length; ++i)
			{
				dashIntervals[i] = float.Parse(dashEntries[i]);
			}
			return dashIntervals;
		}

		private static void validateNonNegative(string name, float value)
		{
			if (value < 0)
			{
				throw new ThemeException(name + " must not be negative: " + value);
			}
		}

		private static void validateExists(string name, object obj, string elementName)
		{
			if (obj == null)
			{
				throw new ThemeException("missing attribute " + name + " for element: " + elementName);
			}
		}
	}

}