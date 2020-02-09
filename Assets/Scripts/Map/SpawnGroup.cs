using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Reactics.Battle
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct SpawnGroup
    {
        [SerializeField]
        public Point[] points;

    }

}