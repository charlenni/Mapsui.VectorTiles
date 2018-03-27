namespace Mapsui.VectorTiles.MapCSSStyler.v0_2.Domain
{
    /// <summary>
    /// Strongly typed MapCSS v0.2 Declaration class.
    /// </summary>
    public class DeclarationTextTransform : Declaration<QualifierTextTransformEnum, TextTransformEnum>
    {

    }

    /// <summary>
    /// Strongly typed MapCSS v0.2 Declaration class.
    /// </summary>
    public enum QualifierTextTransformEnum
    {
        TextTransform
    }

    /// <summary>
    /// Strongly typed MapCSS v0.2 Declaration class.
    /// </summary>
    public enum TextTransformEnum
    {
        Uppercase,
        Lowercase,
        Capitalize,
        None
    }
}