using System;
using System.Runtime.InteropServices;
using Reactics.Util;

namespace Reactics.Battle
{

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Tile
    {
        public int elevation;
        public BlittableBool inaccessible;
        public bool Accessible() => !inaccessible;
    }


}