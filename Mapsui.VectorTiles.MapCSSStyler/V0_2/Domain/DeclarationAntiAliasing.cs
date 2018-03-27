namespace Mapsui.VectorTiles.MapCSSStyler.v0_2.Domain
{
    /// <summary>
    /// Strongly typed MapCSS v0.2 Declaration class.
    /// </summary>
    public class DeclarationAntiAliasing : Declaration<QualifierAntiAliasingEnum, AntiAliasingEnum>
    {

    }

    /// <summary>
    /// Strongly typed MapCSS v0.2 Declaration class.
    /// </summary>
    public enum QualifierAntiAliasingEnum
    {
        /// <summary>
        /// Anti aliasing option.
        /// </summary>
        AntiAliasing
    }

    /// <summary>
    /// Strongly typed MapCSS v0.2 Declaration class.
    /// </summary>
    public enum AntiAliasingEnum
    {
        /// <summary>
        /// Full option.
        /// </summary>
        Full,
        /// <summary>
        /// Test option.
        /// </summary>
        Text,
        /// <summary>
        /// None option.
        /// </summary>
        None
    }
}
