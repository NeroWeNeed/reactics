using System;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore;

namespace Reactics.UI
{


    public class RectangleMeshFactory : IUIConfigurator
    {

        public bool Equals(IUIConfigurator other)
        {
            return GetType().Equals(other.GetType());
        }

        public void Configure(ref Entity entity, EntityCommandBuffer entityCommandBuffer, World world)
        {

            throw new System.NotImplementedException();
        }

        private EntityQueryMask entityQueryMask;
        private bool queryMaskSet = false;


        public void Configure(Entity entity, EntityCommandBuffer entityCommandBuffer, World world)
        {
            throw new System.NotImplementedException();
        }


    }

    public class TextMeshFactory : IUIConfigurator
    {


        public bool Equals(IUIConfigurator other)
        {
            Debug.Log("Testing Equality on " + other);
            return (other == null && this == null) || (other != null && this != null && this == other);
        }
        private VertexAttributeDescriptor[] vertexDescriptor = new VertexAttributeDescriptor[] {
    new VertexAttributeDescriptor(VertexAttribute.Position,VertexAttributeFormat.Float32,3),
    new VertexAttributeDescriptor(VertexAttribute.TexCoord0,VertexAttributeFormat.Float32,2),
    new VertexAttributeDescriptor(VertexAttribute.Color,VertexAttributeFormat.UNorm8,4),
    new VertexAttributeDescriptor(VertexAttribute.Normal,VertexAttributeFormat.Float32,3)
};




        public void Configure(Entity entity, EntityCommandBuffer entityCommandBuffer, World world)
        {
            UIText text = world.EntityManager.GetSharedComponentData<UIText>(entity);
            UIFont font = world.EntityManager.GetSharedComponentData<UIFont>(entity);
            UITextSettings textSettings = world.EntityManager.GetComponentData<UITextSettings>(entity);
            float horizontalOffset = 0;
            TMP_Character character;
            Mesh mesh = new Mesh();
            mesh.SetVertexBufferParams(text.value.Length * 4, vertexDescriptor);
            NativeArray<VertexData> vertexBuffer = new NativeArray<VertexData>(4, Allocator.Temp);
            int[] indexBuffer = new int[text.value.Length * 6];
            float scale = (textSettings.fontSize / font.value.faceInfo.pointSize * font.value.faceInfo.scale)*(Screen.height/((float)Screen.width))*0.1f;
            for (int i = 0; i < text.value.Length; i++)
            {
                character = font.value.characterLookupTable[text.value[i]];
                VertexData.AddVertexData(character.glyph, horizontalOffset, 0, ref vertexBuffer,scale);
                horizontalOffset += character.glyph.metrics.horizontalAdvance;
                indexBuffer[i * 6] = i * 4;
                indexBuffer[i * 6 + 1] = (i * 4) + 2;
                indexBuffer[i * 6 + 2] = (i * 4) + 1;
                indexBuffer[i * 6 + 3] = (i * 4) + 2;
                indexBuffer[i * 6 + 4] = (i * 4) + 3;
                indexBuffer[i * 6 + 5] = (i * 4) + 1;
                mesh.SetVertexBufferData(vertexBuffer, 0, i * 4, 4);
            }
            
            mesh.SetTriangles(indexBuffer, 0,true);
            mesh.RecalculateBounds();
            Debug.Log(mesh.bounds);
            var old = world.EntityManager.GetComponentData<LocalToScreen>(entity);
            old.extents = new float2(mesh.bounds.extents.x,mesh.bounds.extents.y);
            
            entityCommandBuffer.SetComponent(entity,old);
            entityCommandBuffer.AddComponent(entity, new LocalToWorld());
            entityCommandBuffer.AddSharedComponent(entity, new RenderMesh
            {
                mesh = mesh,
                material = font.value.material,
                subMesh = 0,
                layer = 5,
                castShadows = ShadowCastingMode.Off,
                receiveShadows = false
            });
        }
        [StructLayout(LayoutKind.Explicit, Pack = 8)]
        [Serializable]
        struct VertexData
        {
            [FieldOffset(0)]
            public float3 position;
            [FieldOffset(12)]
            public float3 normal;
            [FieldOffset(24)]
            public Color32 color;
            [FieldOffset(28)]
            public float2 uv;


            public static void AddVertexData(Glyph glyph, float horizontalOffset, int start, ref NativeArray<VertexData> data,float scale)
            {
                
                data[start] = new VertexData
                {
                    position = new float3(horizontalOffset*scale, -(glyph.metrics.height-glyph.metrics.horizontalBearingY)*scale, 0),
                    uv = new float2(glyph.glyphRect.x / 1024f, glyph.glyphRect.y / 1024f),
                    color = Color.white,
                    normal = Vector3.forward
                };
                data[start + 1] = new VertexData
                {
                    position = new float3((horizontalOffset + glyph.metrics.width)*scale, -(glyph.metrics.height-glyph.metrics.horizontalBearingY)*scale, 0),
                    uv = new float2((glyph.glyphRect.x + glyph.glyphRect.width) / 1024f, glyph.glyphRect.y / 1024f),
                    color = Color.white,
                    normal = Vector3.forward
                };
                data[start + 2] = new VertexData
                {
                    position = new float3(horizontalOffset*scale, (glyph.metrics.height-(glyph.metrics.height-glyph.metrics.horizontalBearingY))*scale, 0),
                    uv = new float2(glyph.glyphRect.x / 1024f, (glyph.glyphRect.y + glyph.glyphRect.height) / 1024f),
                    color = Color.white,
                    normal = Vector3.forward
                };
                data[start + 3] = new VertexData
                {
                    position = new float3((horizontalOffset + glyph.metrics.width)*scale, (glyph.metrics.height-(glyph.metrics.height-glyph.metrics.horizontalBearingY))*scale, 0),
                    uv = new float2((glyph.glyphRect.x + glyph.glyphRect.width) / 1024f, (glyph.glyphRect.y + glyph.glyphRect.height) / 1024f),
                    color = Color.white,
                    normal = Vector3.forward
                };
            }


            public override string ToString()
            {
                return position.ToString();
            }
        }
    }
}