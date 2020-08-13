using System;
using System.Collections.Generic;
using Reactics.Core.Commons;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
namespace Reactics.Core.Map {

    //TODO: Store highlight/collision data in chunk.
    /// <summary>
    /// Component for tracking highlighted tiles
    /// </summary>
    [ChunkSerializable]
    public struct MapHighlightState : IComponentData, IMapHighlightInfo {
        /// <summary>
        /// Bit flag that holds which layers are dirty.
        /// </summary>
        public ushort dirty;

        public ushort Dirty { get => dirty; }
        /// <summary>
        /// Hashmap for keeping track of hits to layers
        /// </summary>
        public UnsafeMultiHashMap<ushort, Point> states;

        public IEnumerator<Point> GetPoints(ushort layer) => states.GetValuesForKey(layer);

        public IEnumerator<Point> GetPoints(MapLayer layer) => GetPoints((ushort)layer);

    }
    /// <summary>
    /// ShaderGraph Property Component for setting highlight color.
    /// </summary>
/*     [Serializable]
    [MaterialProperty("_Color", MaterialPropertyFormat.Float4)]
    public struct MapHighlightColor : IComponentData
    {
        public float4 Value;

        public MapHighlightColor(float4 value)
        {
            Value = value;
        }
        public MapHighlightColor(Color color)
        {
            Value = new float4(color.r, color.g, color.b, color.a);
        }
        public MapHighlightColor(Color32 color)
        {
            Value = new float4(color.r, color.g, color.b, color.a);
        }
    } */
    /// <summary>
    /// Buffer Element to set highlight states of tiles on target map.
    /// </summary>
    public struct HighlightTile : IBufferElementData, IEquatable<HighlightTile>, IEquatable<HighlightSystemTile> {

        public Point point;

        public ushort state;

        public HighlightTile(Point point, ushort state) {
            this.point = point;
            this.state = state;
        }
        public HighlightTile(Point point, MapLayer layer) {
            this.point = point;
            state = (ushort)layer;
        }
        public HighlightTile(Point point, params MapLayer[] layers) {
            this.point = point;
            state = 0;
            for (int i = 0; i < layers.Length; i++) {
                state |= (ushort)layers[i];
            }

        }
        public HighlightTile(Point point, NativeArray<MapLayer> layers) {

            this.point = point;
            state = 0;
            for (int i = 0; i < layers.Length; i++) {
                state |= (ushort)layers[i];
            }

        }
        public bool Equals(HighlightTile other) {
            return point.Equals(other.point) &&
                    state == other.state;
        }

        public bool Equals(HighlightSystemTile other) {
            return point.Equals(other.point) &&
                    state == other.state;
        }

        public override int GetHashCode() {
            int hashCode = -682509521;
            hashCode = hashCode * -1521134295 + point.GetHashCode();
            hashCode = hashCode * -1521134295 + state.GetHashCode();
            return hashCode;
        }

        public static explicit operator HighlightTile(HighlightSystemTile value) => new HighlightTile(value.point, value.state);
    }
    /// <summary>
    /// System Buffer Element to set highlight states of tiles on target map. Used to track when points are removed/added.
    /// </summary>
    public struct HighlightSystemTile : ISystemStateBufferElementData, IEquatable<HighlightSystemTile>, IEquatable<HighlightTile> {

        public Point point;

        public ushort state;

        public HighlightSystemTile(Point point, ushort state) {

            this.point = point;
            this.state = state;
        }
        public HighlightSystemTile(Point point, MapLayer layer) {

            this.point = point;
            state = (ushort)layer;
        }
        public HighlightSystemTile(Point point, params MapLayer[] layers) {

            this.point = point;
            state = 0;
            for (int i = 0; i < layers.Length; i++) {
                state |= (ushort)layers[i];
            }

        }
        public HighlightSystemTile(Point point, NativeArray<MapLayer> layers) {

            this.point = point;
            state = 0;
            for (int i = 0; i < layers.Length; i++) {
                state |= (ushort)layers[i];
            }

        }



        public override int GetHashCode() {
            int hashCode = -977944976;
            hashCode = hashCode * -1521134295 + point.GetHashCode();
            hashCode = hashCode * -1521134295 + state.GetHashCode();
            return hashCode;
        }

        public bool Equals(HighlightTile other) {
            return point.Equals(other.point) &&
                    state == other.state;
        }

        public bool Equals(HighlightSystemTile other) {
            return point.Equals(other.point) &&
                    state == other.state;
        }
        public static explicit operator HighlightSystemTile(HighlightTile value) => new HighlightSystemTile(value.point, value.state);
    }
}