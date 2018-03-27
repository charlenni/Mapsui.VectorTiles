using System;

namespace Mapsui.VectorTiles.MapCSSStyler.v0_2.Eval
{
    public class EvalInterpreterException : Exception
    {
        public EvalInterpreterException(string message)
            : base(message)
        {

        }

        public EvalInterpreterException(string message, params object[] args)
            : base(string.Format(message, args))
        {

        }
    }
}
