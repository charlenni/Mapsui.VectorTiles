namespace Mapsui.VectorTiles.MapCSSStyler.v0_2.Domain
{
    /// <summary>
    /// Strongly typed MapCSS v0.2 Declaration class.
    /// </summary>
    public class DeclarationFontStyle : Declaration<QualifierFontStyleEnum, FontStyleEnum>
    {

    }

    /// <summary>
    /// Strongly typed MapCSS v0.2 Declaration class.
    /// </summary>
    public enum QualifierFontStyleEnum
    {
        FontStyle
    }

    /// <summary>
    /// Strongly typed MapCSS v0.2 Declaration class.
    /// </summary>
    public enum FontStyleEnum
    {
        Italic,
        Normal
    }
}
