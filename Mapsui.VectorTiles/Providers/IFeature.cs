namespace Mapsui.Providers
{
    using System.Collections.Generic;
    using Geometries;
    using Styles;

    public interface IFeature
    {
        IGeometry Geometry { get; set; }
        IDictionary<IStyle, object> RenderedGeometry { get; }
        ICollection<IStyle> Styles { get; }
        object this[string key] { get; set; }
        IEnumerable<string> Fields { get; }
    }
}