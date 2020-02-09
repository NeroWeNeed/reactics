using System;
using System.Runtime.InteropServices;

namespace Reactics.Battle
{

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Tile
    {
        public int elevation;
        public bool inaccessible;
    }


}