﻿namespace Mapsui.VectorTiles.MapCSSStyler.v0_2.Domain
{
    /// <summary>
    /// Strongly typed MapCSS v0.2 Declaration class.
    /// </summary>
    public class DeclarationTextDecoration : Declaration<QualifierTextDecorationEnum, TextDecorationEnum>
    {

    }

    /// <summary>
    /// Strongly typed MapCSS v0.2 Declaration class.
    /// </summary>
    public enum QualifierTextDecorationEnum
    {
        TextDecoration
    }

    /// <summary>
    /// Strongly typed MapCSS v0.2 Declaration class.
    /// </summary>
    public enum TextDecorationEnum
    {
        Underlined,
        None
    }
}
