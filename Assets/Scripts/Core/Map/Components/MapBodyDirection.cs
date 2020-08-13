using System;
using System.Runtime.InteropServices;
using Reactics.Core.Commons;
using Unity.Entities;
using Unity.Transforms;

namespace Reactics.Core.Map {
    [Serializable]

    public enum MapBodyDirection {

        Uninitialized = 0,
        West = 1,
        NorthWest = 2,
        North = 3,
        NorthEast = 4,
        East = 5,
        SouthEast = 6,
        South = 7,
        SouthWest = 8

    }



}