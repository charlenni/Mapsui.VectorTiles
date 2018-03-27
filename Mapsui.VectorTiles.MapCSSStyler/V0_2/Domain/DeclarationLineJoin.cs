namespace Mapsui.VectorTiles.MapCSSStyler.v0_2.Domain
{
    /// <summary>
    /// Strongly typed MapCSS v0.2 Declaration class.
    /// </summary>
    public class DeclarationLineJoin : Declaration<QualifierLineJoinEnum, LineJoinEnum>
    {

    }

    /// <summary>
    /// Strongly typed MapCSS v0.2 Declaration class.
    /// </summary>
    public enum QualifierLineJoinEnum
    {
        LineJoin,
        CasingLineJoin
    }

    /// <summary>
    /// Strongly typed MapCSS v0.2 Declaration class.
    /// </summary>
    public enum LineJoinEnum
    {
        Round,
        Miter,
        Bevel
    }
}
