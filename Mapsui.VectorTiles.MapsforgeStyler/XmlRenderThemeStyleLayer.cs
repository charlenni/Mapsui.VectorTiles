using System;
using System.Collections.Generic;

/*
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2016 devemux86
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


	/// <summary>
	/// An individual layer in the render theme menu system.
	/// A layer can have translations, categories that will always be enabled when the layer is selected,
	/// as well as optional overlays.
	/// </summary>
	[Serializable]
	public class XmlRenderThemeStyleLayer
	{
		private const long serialVersionUID = 1L;

		private readonly ISet<string> categories;
		private readonly string defaultLanguage;
		private readonly bool enabled;
		private readonly string id;
		private readonly IList<XmlRenderThemeStyleLayer> overlays;
		private readonly IDictionary<string, string> titles;
		private readonly bool visible;

		internal XmlRenderThemeStyleLayer(string id, bool visible, bool enabled, string defaultLanguage)
		{
			this.id = id;
			this.titles = new Dictionary<>();
			this.categories = new LinkedHashSet<>();
			this.visible = visible;
			this.defaultLanguage = defaultLanguage;
			this.enabled = enabled;
			this.overlays = new List<>();
		}

		public virtual void addCategory(string category)
		{
			this.categories.Add(category);
		}

		public virtual void addOverlay(XmlRenderThemeStyleLayer overlay)
		{
			this.overlays.Add(overlay);
		}

		public virtual void addTranslation(string language, string name)
		{
			this.titles[language] = name;
		}

		public virtual ISet<string> Categories
		{
			get
			{
				return this.categories;
			}
		}

		public virtual string Id
		{
			get
			{
				return this.id;
			}
		}

		public virtual IList<XmlRenderThemeStyleLayer> Overlays
		{
			get
			{
				return this.overlays;
			}
		}

		public virtual string getTitle(string language)
		{
			string result = this.titles[language];
			if (string.ReferenceEquals(result, null))
			{
				return this.titles[this.defaultLanguage];
			}
			return result;
		}

		public virtual IDictionary<string, string> Titles
		{
			get
			{
				return this.titles;
			}
		}

		public virtual bool Enabled
		{
			get
			{
				return this.enabled;
			}
		}

		public virtual bool Visible
		{
			get
			{
				return this.visible;
			}
		}
	}

}