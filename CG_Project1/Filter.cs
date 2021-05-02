using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CG_lab1KM
{
    public class labFilter
    {
        public float[,] Matrix;
    }
    public class ConvoFilter
    {
        public float[,] Kernel;
        public float Divisor;
        public float Offset;
        public int AnchorX, AnchorY;
    }
    public class BlurFilter : ConvoFilter
    {

    }
    public class GBlurFilter : ConvoFilter
    {

    }
    public class SharpenFilter : ConvoFilter
    {

    }
    public class EdgeFilter : ConvoFilter
    {

    }
    public class EmbossFilter : ConvoFilter
    {

    }
    public class CustomFilter : ConvoFilter
    {

    }
}
