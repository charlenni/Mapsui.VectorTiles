﻿using Mapsui.VectorTiles.MapCSSStyler.v0_2.Eval;

namespace Mapsui.VectorTiles.MapCSSStyler.v0_2.Domain
{
    /// <summary>
    /// Strongly typed MapCSS v0.2 Declaration class.
    /// </summary>
    public class DeclarationInt : Declaration<DeclarationIntEnum, int>
	{
		/// <summary>
		/// Evalues the value in this declaration or returns the regular value when there is no eval function.
		/// </summary>
		/// <param name="tags"></param>
		/// <returns></returns>
		/// <param name="mapCSSObject">Map CSS object.</param>
		public override int Eval (MapCSSObject mapCSSObject)
		{
			if (!string.IsNullOrWhiteSpace (this.EvalFunction)) {
				return EvalInterpreter.Instance.InterpretInt (this.EvalFunction, mapCSSObject);
			}
			return this.Value;
		}
    }

    /// <summary>
    /// Strongly typed MapCSS v0.2 Declaration class.
    /// </summary>
    public enum DeclarationIntEnum
    {
        FillColor,
        Color,
        CasingColor,
        Extrude,
        ExtrudeEdgeColor,
        ExtrudeFaceColor,
        IconWidth,
        IconHeight,
        FontSize,
        TextColor,
        TextOffset,
        MaxWidth,
        TextHaloColor,
        TextHaloRadius,
        TextOffsetY,
        TextSpacing
    }
}