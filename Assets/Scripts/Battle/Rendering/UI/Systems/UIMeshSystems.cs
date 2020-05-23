using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using Reactics.UI;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore;
using Unity.Jobs;
using UnityEngine.AddressableAssets;

namespace Reactics.UI
{

    [UpdateInGroup(typeof(UIMeshSystemGroup))]
    
    public class UIMeshBoxSystem : SystemBase
    {

        public static Material material;
        public static readonly VertexAttributeDescriptor[] vertexAttributeDescriptor = new VertexAttributeDescriptor[] {
            new VertexAttributeDescriptor(VertexAttribute.Position,VertexAttributeFormat.Float32,3),
            new VertexAttributeDescriptor(VertexAttribute.Normal,VertexAttributeFormat.Float32,3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0,VertexAttributeFormat.Float32,2)
        };
        private static readonly SubMeshDescriptor subMeshDescriptor = new SubMeshDescriptor(0, 6, MeshTopology.Triangles);
        private static readonly ushort[] indices = new ushort[] { 0, 2, 1, 2, 3, 1 };
        private UIEntityCommandBufferSystem entityCommandBufferSystem;
        private EntityQuery query;

        protected override void OnCreate()
        {
            entityCommandBufferSystem = World.GetOrCreateSystem<UIEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {

            var entityCount = query.CalculateEntityCount();


            if (entityCount > 0)
            {
                Mesh.MeshDataArray meshes = Mesh.AllocateWritableMeshData(entityCount);

                NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob);
                for (int i = 0; i < meshes.Length; i++)
                {
                    var m = meshes[i];
                    m.SetVertexBufferParams(4, vertexAttributeDescriptor);
                    m.SetIndexBufferParams(6, IndexFormat.UInt16);
                    m.subMeshCount = 1;
                    m.GetIndexData<ushort>().CopyFrom(indices);
                    m.SetSubMesh(0, subMeshDescriptor);
                }
                Entities.WithChangeFilter<UIElementBounds>().WithAll<UIBoxMesh>().ForEach((Entity entity, int entityInQueryIndex, int nativeThreadIndex, ref LocalToScreen lts, in UIElementBounds bounds) =>
                {
                    var mesh = meshes[entities.IndexOf<Entity, Entity>(entity)];

                    var width = bounds.right - bounds.left;
                    var height = bounds.bottom - bounds.top;
                    var vertexBuffer = mesh.GetVertexData<BoxVertexData>();
                    vertexBuffer[0] = new BoxVertexData
                    {
                        vertex = new float3(-width / 2, -height / 2, 0),
                        uv0 = new float2(0, 0),
                        normal = -Vector3.forward
                    };
                    vertexBuffer[1] = new BoxVertexData
                    {
                        vertex = new float3(width / 2, -height / 2, 0),
                        uv0 = new float2(1, 0),
                        normal = -Vector3.forward
                    };
                    vertexBuffer[2] = new BoxVertexData
                    {
                        vertex = new float3(-width / 2, height / 2, 0),
                        uv0 = new float2(0, 1),
                        normal = -Vector3.forward
                    };
                    vertexBuffer[3] = new BoxVertexData
                    {
                        vertex = new float3(width / 2, height / 2, 0),
                        uv0 = new float2(1, 1),
                        normal = -Vector3.forward
                    };
                    lts.extents = new float2(width / 2, height / 2);
                    lts.screenAnchor = lts.localAnchor = UIAnchor.TOP_LEFT;
                }).WithStoreEntityQueryInField(ref query).ScheduleParallel();
                var ecb = entityCommandBufferSystem.CreateCommandBuffer();
                var entityMeshes = new Mesh[entityCount];

                Job.WithCode(() =>
                {
                    Mesh mesh;
                    for (int i = 0; i < entities.Length; i++)
                    {

                        if (!EntityManager.HasComponent<RenderMesh>(entities[i]))
                        {

                            mesh = new Mesh();
                            ecb.AddSharedComponent(entities[i], new RenderMesh
                            {
                                mesh = mesh,
                                subMesh = 0,
                                layer = 5,
                                material = material
                            });
                        }
                        else
                        {
                            var renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(entities[i]);

                            if (renderMesh.mesh == null)
                            {
                                mesh = new Mesh();
                                ecb.SetSharedComponent(entities[i], new RenderMesh
                                {
                                    mesh = mesh,
                                    subMesh = 0,
                                    layer = 5,
                                    material = material
                                });
                            }
                            else
                            {
                                mesh = renderMesh.mesh;
                            }
                        }
                        entityMeshes[i] = mesh;
                    }
                    Mesh.ApplyAndDisposeWritableMeshData(meshes, entityMeshes);
                }).WithoutBurst().WithDeallocateOnJobCompletion(entities).Run();
                entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
            }


        }
        private struct BoxVertexData
        {
            public float3 vertex;
            public float3 normal;
            public float2 uv0;
        }
    }

    [UpdateInGroup(typeof(UIMeshSystemGroup))]
    //[DisableAutoCreation]
    public class UITextMeshBoxSystem : SystemBase
    {
        private static readonly VertexAttributeDescriptor[] vertexAttributeDescriptor = new VertexAttributeDescriptor[] {
            new VertexAttributeDescriptor(VertexAttribute.Position,VertexAttributeFormat.Float32,3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0,VertexAttributeFormat.Float32,2),
            new VertexAttributeDescriptor(VertexAttribute.Color,VertexAttributeFormat.UNorm8,4),
            new VertexAttributeDescriptor(VertexAttribute.Normal,VertexAttributeFormat.Float32,3)
        };
        private static readonly ushort[] indices = new ushort[] { 0, 2, 1, 2, 3, 1 };

        private static readonly SubMeshDescriptor subMeshDescriptor = new SubMeshDescriptor(0, 6, MeshTopology.Triangles);
        private UIEntityCommandBufferSystem entityCommandBufferSystem;
        private UIToLTSSystem ui2ltsSystem;
        private EntityQuery query;

        protected override void OnCreate()
        {
            entityCommandBufferSystem = World.GetOrCreateSystem<UIEntityCommandBufferSystem>();


        }
        protected override void OnUpdate()
        {
            var entityCount = query.CalculateEntityCount();


            if (entityCount > 0)
            {

                Mesh.MeshDataArray meshes = Mesh.AllocateWritableMeshData(entityCount);
                NativeMultiHashMap<Entity, GlyphData> glyphData = new NativeMultiHashMap<Entity, GlyphData>(entityCount, Allocator.TempJob);

                Entities.WithChangeFilter<UIElementBounds>().WithAll<UITextMesh>().ForEach((Entity entity, int entityInQueryIndex, int nativeThreadIndex, ref LocalToScreen lts, in UIElementBounds bounds, in UIText text, in UIFont settings) =>
                {
                    GlyphData.GetGlyphs(text.value, settings, entity, glyphData);
                    //glyphData.Add(entity, new GlyphData(text, settings));
                    //Glyph glyph = new Glyph();
                }).WithStoreEntityQueryInField(ref query).WithoutBurst().Run();

                //Temp until params bug fixed.
                var entities = query.ToEntityArray(Allocator.TempJob);
                for (int i = 0; i < meshes.Length; i++)
                {
                    var glyphCount = glyphData.CountValuesForKey(entities[i]);
                    var mesh = meshes[i];
                    mesh.SetVertexBufferParams(4 * glyphCount, vertexAttributeDescriptor);
                    mesh.SetIndexBufferParams(6, IndexFormat.UInt16);
                    //var indexData = meshes[i].GetIndexData<ushort>();
                    mesh.subMeshCount = 1;
                    mesh.GetIndexData<ushort>().CopyFrom(indices);
                    mesh.SetSubMesh(0, subMeshDescriptor);
                    /*                     for (int j = 0; j < glyphCount; j++)
                                        {
                                            indexData[j * 6] = (ushort) (j * 4);
                                            indexData[j * 6 + 1] = (ushort) (j * 4 + 2);
                                            indexData[j * 6 + 2] = (ushort)(j * 4 + 1);
                                            indexData[j * 6 + 3] = (ushort)(j * 4 + 2);
                                            indexData[j * 6 + 4] = (ushort)(j * 4 + 3);
                                            indexData[j * 6 + 5] = (ushort)(j * 4 + 1);

                                        } */
                }

                var updateTextMesh = new UpdateTextMeshJob
                {
                    UITextType = GetArchetypeChunkSharedComponentType<UIText>(),
                    UITextSettingsType = GetArchetypeChunkSharedComponentType<UIFont>(),
                    EntityType = GetArchetypeChunkEntityType(),
                    MeshData = meshes,
                    UIElementBoundsType = GetArchetypeChunkComponentType<UIElementBounds>(true),
                    LocalToScreenType = GetArchetypeChunkComponentType<LocalToScreen>(),
                    GlyphData = glyphData


                }.ScheduleParallel(query, Dependency);


                var ecb = entityCommandBufferSystem.CreateCommandBuffer();
                var entityMeshes = new Mesh[meshes.Length];
                var x = EntityManager.GetArchetypeChunkSharedComponentType<RenderMesh>();


                //var y = chunk.GetSharedComponentData(x, EntityManager);


                Job.WithCode(() =>
                {
                    Mesh mesh;
                    for (int i = 0; i < entities.Length; i++)
                    {

                        if (!EntityManager.HasComponent<RenderMesh>(entities[i]))
                        {

                            mesh = new Mesh();
                            ecb.AddSharedComponent(entities[i], new RenderMesh
                            {
                                mesh = mesh,
                                subMesh = 0,
                                layer = 5,
                                material = EntityManager.GetSharedComponentData<UIFont>(entities[i]).value.material
                            });
                        }
                        else
                        {
                            var renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(entities[i]);

                            if (renderMesh.mesh == null)
                            {
                                mesh = new Mesh();
                                ecb.SetSharedComponent(entities[i], new RenderMesh
                                {
                                    mesh = mesh,
                                    subMesh = 0,
                                    layer = 5,
                                    material = EntityManager.GetSharedComponentData<UIFont>(entities[i]).value.material
                                });
                            }
                            else
                            {
                                mesh = renderMesh.mesh;
                            }
                        }
                        entityMeshes[i] = mesh;
                    }
Mesh.ApplyAndDisposeWritableMeshData(meshes, entityMeshes);
                }).WithoutBurst().Run();
                updateTextMesh.Complete();
                //entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
                

            }
        }
        public struct RenderMeshData
        {
            int index;

        }

        public struct GlyphData
        {
            public GlyphMetrics metrics;

            public GlyphRect rect;

            public float scale;

            public float horizontalStep;

            public float verticalStep;

            public GlyphData(Glyph glyph, UIFont font)
            {
                metrics = glyph.metrics;
                rect = glyph.glyphRect;
                scale = 12f / font.value.faceInfo.pointSize * font.value.faceInfo.scale;
                horizontalStep = glyph.metrics.horizontalAdvance;
                verticalStep = 0f;
            }
            public static void GetGlyphs(string text, UIFont font, Entity entity, NativeMultiHashMap<Entity, GlyphData> glyphData)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    glyphData.Add(entity, new GlyphData(font.value.characterLookupTable[text[i]].glyph, font));
                }
            }
        }

        private struct GlyphVertexData
        {
            public float3 vertex;

            public float3 normal;
            public Color32 color;
            public float2 uv0;



            public static void AddVertexData(GlyphData glyph, float horizontalOffset, float verticalOffset, int start, NativeArray<GlyphVertexData> data, float xScale, float yScale)
            {


                data[start] = new GlyphVertexData
                {
                    vertex = new float3(horizontalOffset * xScale, -(glyph.metrics.height - glyph.metrics.horizontalBearingY) * yScale, 0),
                    uv0 = new float2(glyph.rect.x / 1024f, glyph.rect.y / 1024f),
                    color = Color.white,
                    normal = Vector3.forward
                };
                data[start + 1] = new GlyphVertexData
                {
                    vertex = new float3((horizontalOffset + glyph.metrics.width) * xScale, -(glyph.metrics.height - glyph.metrics.horizontalBearingY) * yScale, 0),
                    uv0 = new float2((glyph.rect.x + glyph.rect.width) / 1024f, glyph.rect.y / 1024f),
                    color = Color.white,
                    normal = Vector3.forward
                };
                data[start + 2] = new GlyphVertexData
                {
                    vertex = new float3(horizontalOffset * xScale, (glyph.metrics.height - (glyph.metrics.height - glyph.metrics.horizontalBearingY)) * yScale, 0),
                    uv0 = new float2(glyph.rect.x / 1024f, (glyph.rect.y + glyph.rect.height) / 1024f),
                    color = Color.white,
                    normal = Vector3.forward
                };
                data[start + 3] = new GlyphVertexData
                {
                    vertex = new float3((horizontalOffset + glyph.metrics.width) * xScale, (glyph.metrics.height - (glyph.metrics.height - glyph.metrics.horizontalBearingY)) * yScale, 0),
                    uv0 = new float2((glyph.rect.x + glyph.rect.width) / 1024f, (glyph.rect.y + glyph.rect.height) / 1024f),
                    color = Color.white,
                    normal = Vector3.forward
                };
            }

        }

        [BurstCompile]
        public struct UpdateTextMeshJob : IJobChunk
        {
            [ReadOnly]
            public ArchetypeChunkSharedComponentType<UIText> UITextType;

            [ReadOnly]
            public ArchetypeChunkSharedComponentType<UIFont> UITextSettingsType;
            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;
            public Mesh.MeshDataArray MeshData;

            [ReadOnly]
            public ArchetypeChunkComponentType<UIElementBounds> UIElementBoundsType;

            public ArchetypeChunkComponentType<LocalToScreen> LocalToScreenType;

            [ReadOnly]
            public NativeMultiHashMap<Entity, GlyphData> GlyphData;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var entities = chunk.GetNativeArray(EntityType);

                var lts = chunk.GetNativeArray(LocalToScreenType);
                for (int i = 0; i < entities.Length; i++)
                {
                    float width = 0f;
                    float height = 0f;
                    var mesh = MeshData[i];
                    //Doesn't work currently and looks like it will be fixed.
                    //mesh.SetVertexBufferParams(4 * GlyphData.CountValuesForKey(entities[i]), vertexAttributeDescriptor);
                    //mesh.SetIndexBufferParams(6, IndexFormat.UInt16);
                    //mesh.subMeshCount = 1;
                    //mesh.GetIndexData<ushort>().CopyFrom(indices);
                    //mesh.SetSubMesh(0, subMeshDescriptor);
                    var vertexData = mesh.GetVertexData<GlyphVertexData>();
                    var glyphEnumerator = GlyphData.GetValuesForKey(entities[i]);
                    float horizontalOffset = 0f, verticalOffset = 0f;
                    while (glyphEnumerator.MoveNext())
                    {
                        {

                            GlyphVertexData.AddVertexData(glyphEnumerator.Current, horizontalOffset, verticalOffset, 0, vertexData, glyphEnumerator.Current.scale, glyphEnumerator.Current.scale);
                            horizontalOffset += glyphEnumerator.Current.horizontalStep;
                            verticalOffset += glyphEnumerator.Current.verticalStep;
                            width += glyphEnumerator.Current.horizontalStep;
                            height = math.max(height, glyphEnumerator.Current.metrics.height);
                        }

                    }
                    var x = lts[i];
                    x.extents = new float2(width / 2, height / 2);
                    x.screenAnchor = x.localAnchor = UIAnchor.TOP_LEFT;
                    lts[i] = x;
                }

            }
        }
    }


    /*     public class UITextMeshSystem : SystemBase
        {
            private static readonly VertexAttributeDescriptor[] vertexDescriptor = new VertexAttributeDescriptor[] {
            new VertexAttributeDescriptor(VertexAttribute.Position,VertexAttributeFormat.Float32,3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0,VertexAttributeFormat.Float32,2),
            new VertexAttributeDescriptor(VertexAttribute.Color,VertexAttributeFormat.UNorm8,4),
            new VertexAttributeDescriptor(VertexAttribute.Normal,VertexAttributeFormat.Float32,3)
        };
            private static readonly SubMeshDescriptor subMeshDescriptor = new SubMeshDescriptor(0, 6, MeshTopology.Triangles);
            protected override void OnUpdate()
            {
                throw new System.NotImplementedException();
            }
        } */


    /*     [UpdateInGroup(typeof(UIMeshSystemGroup))]
        public class FlutterTextMeshSystem : SystemBase
        {
            private EntityQuery query;

            private static readonly VertexAttributeDescriptor[] vertexDescriptor = new VertexAttributeDescriptor[] {
        new VertexAttributeDescriptor(VertexAttribute.Position,VertexAttributeFormat.Float32,3),
        new VertexAttributeDescriptor(VertexAttribute.TexCoord0,VertexAttributeFormat.Float32,2),
        new VertexAttributeDescriptor(VertexAttribute.Color,VertexAttributeFormat.UNorm8,4),
        new VertexAttributeDescriptor(VertexAttribute.Normal,VertexAttributeFormat.Float32,3)
    };
            private static readonly SubMeshDescriptor subMeshDescriptor = new SubMeshDescriptor(0, 6, MeshTopology.Triangles);
            protected override void OnUpdate()
            {

                var entityCount = query.CalculateEntityCount();
                if (entityCount > 0)
                {
                    Mesh.MeshDataArray meshes = Mesh.AllocateWritableMeshData(entityCount);

                    NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob);
                    for (int i = 0; i < meshes.Length; i++)
                    {
                        var m = meshes[i];
                        var m2 = new Mesh();


                        m.SetVertexBufferParams(4 * GetComponent<UIText>(entities[i]).value., vertexDescriptor);
                        m.SetIndexBufferParams(6, IndexFormat.UInt16);
                        m.subMeshCount = 1;
                        //m.GetIndexData<ushort>().CopyFrom(indices);
                        m.SetSubMesh(0, subMeshDescriptor);
                    }

                    Entities.ForEach((Entity Entity, in UIText text, in FlutterFont font, in UITextSettings textSettings) =>
                    {

                        float horizontalOffset = 0;
                    }).WithStoreEntityQueryInField(ref query);

                }
                 TMP_Character character;
                Mesh mesh = renderMesh.mesh ?? new Mesh();
                mesh.SetVertexBufferParams(text.value.Length * 4, vertexDescriptor);
                NativeArray<TextVertexData> vertexBuffer = new NativeArray<TextVertexData>(4, Allocator.Temp);
                int[] indexBuffer = new int[text.value.Length * 6];
                float scale = (textSettings.fontSize.RealValue / font.value.faceInfo.pointSize * font.value.faceInfo.scale);
                {
                    character = font.value.characterLookupTable[text.value[i]];
                    TextVertexData.AddVertexData(character.glyph, horizontalOffset, 0, ref vertexBuffer, scale, scale);
                    horizontalOffset += character.glyph.metrics.horizontalAdvance;
                    indexBuffer[i * 6] = i * 4;
                    indexBuffer[i * 6 + 1] = (i * 4) + 2;
                    indexBuffer[i * 6 + 2] = (i * 4) + 1;
                    indexBuffer[i * 6 + 3] = (i * 4) + 2;
                    indexBuffer[i * 6 + 4] = (i * 4) + 3;
                    indexBuffer[i * 6 + 5] = (i * 4) + 1;
                    mesh.SetVertexBufferData(vertexBuffer, 0, i * 4, 4);
                }

                mesh.SetTriangles(indexBuffer, 0, true);
                mesh.RecalculateBounds();
                mesh.RecalculateTangents();
                renderMesh.mesh = mesh;
                renderMesh.material = font.value.material;
                Debug.Log(mesh.bounds.extents);
                renderMesh.castShadows = ShadowCastingMode.Off;
                renderMesh.receiveShadows = false; 
            }
        } */
}