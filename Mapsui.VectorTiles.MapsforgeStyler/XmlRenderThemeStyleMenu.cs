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
	/// Entry class for automatically building menus from render theme files.
	/// This class holds all the defined layers and allows to retrieve them by name or through iteration.
	/// This class is Serializable to be able to pass an instance of it through the Android Intent
	/// mechanism.
	/// </summary>
	[Serializable]
	public class XmlRenderThemeStyleMenu
	{
		private const long serialVersionUID = 1L;

		private readonly string defaultLanguage;
		private readonly string defaultValue;
		private readonly string id;
		private readonly IDictionary<string, XmlRenderThemeStyleLayer> layers;

		public XmlRenderThemeStyleMenu(string id, string defaultLanguage, string defaultValue)
		{
			this.id = id;
			this.defaultLanguage = defaultLanguage;
			this.defaultValue = defaultValue;
			this.layers = new LinkedHashMap<>();
		}

		public virtual XmlRenderThemeStyleLayer createLayer(string id, bool visible, bool enabled)
		{
			XmlRenderThemeStyleLayer style = new XmlRenderThemeStyleLayer(id, visible, enabled, this.defaultLanguage);
			this.layers[id] = style;
			return style;
		}

		public virtual string DefaultLanguage
		{
			get
			{
				return this.defaultLanguage;
			}
		}

		public virtual string DefaultValue
		{
			get
			{
				return this.defaultValue;
			}
		}

		public virtual string Id
		{
			get
			{
				return this.id;
			}
		}

		public virtual XmlRenderThemeStyleLayer getLayer(string id)
		{
			return this.layers[id];
		}

		public virtual IDictionary<string, XmlRenderThemeStyleLayer> Layers
		{
			get
			{
				return this.layers;
			}
		}
	}

}