using System;
using System.Collections;
using System.Collections.Generic;
using NeroWeNeed.Commons;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NeroWeNeed.UIDots {
    public class UISchema : ScriptableObject, IEnumerable<UISchema.Element> {
        public List<Element> elements = new List<Element>();
        public List<InheritableField> inheritableFields = new List<InheritableField>();


        public IEnumerator<Element> GetEnumerator() {
            return elements.GetEnumerator();
        }
        public BlobAssetReference<CompiledUISchema> Compile(Allocator allocator = Allocator.Temp) {
            var builder = new BlobBuilder(Allocator.Temp);
            ref CompiledUISchema root = ref builder.ConstructRoot<CompiledUISchema>();
            var elements = builder.Allocate(ref root.elements, this.elements.Count);
            var inheritableFields = builder.Allocate(ref root.inheritableFields, this.inheritableFields.Count);
            for (int i = 0; i < this.elements.Count; i++) {

                elements[i] = new CompiledElement
                {
                    layout = BurstCompiler.CompileFunctionPointer(this.elements[i].layoutPass.Value.CreateDelegate(typeof(UILayoutPass)) as UILayoutPass),
                    render = BurstCompiler.CompileFunctionPointer(this.elements[i].renderPass.Value.CreateDelegate(typeof(UIRenderPass)) as UIRenderPass),
                    renderBoxCounter = this.elements[i].renderBoxCounter.IsCreated ? BurstCompiler.CompileFunctionPointer<UIRenderBoxCounter>(this.elements[i].renderBoxCounter.Value.CreateDelegate(typeof(UIRenderBoxCounter)) as UIRenderBoxCounter) : default
                };
            }

            for (int i = 0; i < this.inheritableFields.Count; i++) {
                inheritableFields[i] = this.inheritableFields[i];
            }
            var result = builder.CreateBlobAssetReference<CompiledUISchema>(allocator);
            builder.Dispose();
            return result;

        }
        IEnumerator IEnumerable.GetEnumerator() {
            return elements.GetEnumerator();
        }

        [Serializable]
        public struct Element {
            public string identifier;
            public ulong requiredBlockMask;
            public ulong optionalBlockMask;
            public SerializableMethod layoutPass;
            public SerializableMethod renderPass;
            public SerializableMethod renderBoxCounter;
        }
        [Serializable]
        public struct InheritableField {
            public byte config;
            public int offset;
            public int length;
        }

        public struct CompiledElement {
            public FunctionPointer<UILayoutPass> layout;
            public FunctionPointer<UIRenderPass> render;
            public FunctionPointer<UIRenderBoxCounter> renderBoxCounter;

        }

    }
}