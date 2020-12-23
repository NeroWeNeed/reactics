using System;
using Reactics.Core.Commons;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;
//using UnityEngine.AddressableAssets;

namespace Reactics.Core.AssetDefinitions {

    public class WorldDefinitionAsset : TextAsset {
        public unsafe World Create() {
            World world;
            fixed (byte* ptr = bytes) {
                var data = new MemoryBinaryReader(ptr);
                var blob = data.Read<WorldDefinitionData>();
                world = new World(blob.Value.name.ToString());
                for (int i = 0; i < blob.Value.entries.Length; i++) {
                    world.CreateSystem(Type.GetType(blob.Value.entries[i].assemblyQualifiedName.ToString()));
                }
            }
            return world;
        }
    }
    [Serializable]
    public struct WorldDefinitionData {
        public BlobString name;
        public BlobArray<Entry> entries;
        public struct Entry {
            public BlobString assemblyQualifiedName;
        }
    }
}