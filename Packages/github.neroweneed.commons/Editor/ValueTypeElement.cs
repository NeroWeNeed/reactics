using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.Commons.Editor {
    public delegate bool TypedFieldValueConverter<TElementValue, TValue>(TElementValue input, out TValue result);
    [Flags]
    public enum RenderChildrenOptions : byte {
        None = 0,
        Render = 1,
        RenderTerminalLabels = 2,
        RenderContainerLabels = 4,
        RenderTerminals = Render | RenderTerminalLabels,
        RenderAll = Render | RenderTerminalLabels | RenderContainerLabels,
    }
    public static class ValueTypeFieldExtensions {
        internal static Dictionary<Type, Type> drawers = new Dictionary<Type, Type>();

        [InitializeOnLoadMethod]
        private static void CollectTypeFieldDrawers() {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                if (assembly.GetCustomAttribute<ValueTypeFieldDrawerAttribute>() == null)
                    continue;
                foreach (var type in assembly.GetTypes()) {
                    if (type.IsSubclassOf(typeof(ValueTypeFieldDrawer))) {
                        foreach (var attr in type.GetCustomAttributes<ValueTypeFieldDrawerAttribute>()) {
                            if (attr.Type == null) {
                                Debug.LogError($"Type must be specified on TypedFieldDrawer {type.FullName}");
                                continue;
                            }
                            if (drawers.ContainsKey(attr.Type)) {
                                Debug.LogError($"Multiple Type Drawers specified for {type.FullName}");
                                continue;
                            }
                            drawers[attr.Type] = type;
                        }
                    }
                }
            }
        }

        internal static bool TryGetTypeFieldDrawer(this Type type, out ValueTypeFieldDrawer drawer) {

            if (drawers.TryGetValue(type, out Type result)) {
                drawer = Activator.CreateInstance(result) as ValueTypeFieldDrawer;
                return true;
            }
            else {
                drawer = null;
                return false;
            }
        }
        internal static ValueTypeFieldDrawer GetTypeFieldDrawer(this Type type) {
            var result = drawers[type];
            return Activator.CreateInstance(result) as ValueTypeFieldDrawer;
        }

    }
    public abstract class ValueTypeMemoryField : VisualElement {

        public abstract int Offset { get; }
        public abstract int Length { get; }
        public abstract string Path { get; }
        public abstract bool IsDefault { get; }
        public abstract Type Type { get; }

        public void UpdateMemory(object value) => UpdateMemory(Offset, Length, value);

        protected abstract void UpdateMemory(int offset, int length, object value);

        public abstract string CreateLabelText();
        internal unsafe static bool IsDefaultData(IntPtr pointer, int offset, int length) {
            var bytes = new NativeArray<byte>(length, Allocator.Temp, NativeArrayOptions.ClearMemory);
            UnsafeUtility.MemCpy(bytes.GetUnsafePtr<byte>(), (pointer + offset).ToPointer(), (long)length);
            for (int i = 0; i < bytes.Length; i++) {
                if (bytes[i] != 0) {
                    bytes.Dispose();
                    return false;
                }
            }

            bytes.Dispose();
            return true;
        }
    }

    /// <summary>
    /// UI Element for rendering c# value types without serializableproperties. Does not support recursive rendering.
    /// </summary>
    public class ValueTypeElement : ValueTypeMemoryField, IDisposable {
        public const string CLASS = "value-type-element";
        public const string NODE_CLASS = "value-type-element-node";
        public const string TERMINAL_CLASS = "value-type-element-terminal";
        public const string CONTAINER_CLASS = "value-type-element-container";
        public const string LABEL_CLASS = "value-type-element-node-label";
        public const string BUFFER_CLASS = "value-type-element-buffer";

        private IntPtr pointer;
        public IntPtr Pointer { get => pointer; }
        public bool IsCreated { get => pointer != IntPtr.Zero; }

        public override bool IsDefault { get => IsDefaultData(Pointer, 0, Length); }
        public override int Offset { get; } = 0;
        private int length;
        public override int Length { get => length; }
        private Type type;
        public override Type Type { get => type; }

        private string path;
        public override string Path { get => path; }


        public override string CreateLabelText() {
            return "";
        }

        public static ValueTypeElement Create(Type type, RenderChildrenOptions options = RenderChildrenOptions.RenderTerminalLabels) {
            Contract.Ensures(type.IsValueType);
            return new ValueTypeElement(new byte[UnsafeUtility.SizeOf(type)], type, options);
        }
        /*         public static ValueTypeElement Create(Type type, object initial, RenderChildrenOptions options = RenderChildrenOptions.RenderTerminalLabels) {
                    Contract.Ensures(type.IsValueType);
                    Contract.Ensures(initial.GetType() == type);

                    return new ValueTypeElement(initial, type, options);
                } */
        public static ValueTypeElement Create(Type type, byte[] initial, RenderChildrenOptions options = RenderChildrenOptions.RenderTerminalLabels) {
            Contract.Ensures(type.IsValueType);
            Contract.Ensures(UnsafeUtility.SizeOf(type) == initial.Length);
            return new ValueTypeElement(initial, type, options);
        }

        private unsafe ValueTypeElement(byte[] data, Type type, RenderChildrenOptions options) {
            pointer = (IntPtr)UnsafeUtility.Malloc(UnsafeUtility.SizeOf(type), 0, Allocator.Persistent);
            this.length = UnsafeUtility.SizeOf(type);
            this.path = type.FullName;
            Marshal.Copy(data, 0, pointer, data.Length);
            this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/github.neroweneed.commons/Editor/Resources/ValueTypeElement.uss"));
            this.AddToClassList(CLASS);
            this.type = type;
            var typeStack = new Stack<Type>();
            foreach (var fieldInfo in type.GetSerializableFields(f => f.GetCustomAttribute<HideInInspector>() == null)) {
                typeStack.Push(fieldInfo.FieldType);
                ConstructTree(this, fieldInfo, pointer, this.path, 0, typeStack, options);
                typeStack.Pop();
            }
        }

        internal static string BuildLabelText(string parentName, string name) {
            if (string.IsNullOrEmpty(parentName)) {
                return name;
            }
            else {
                return $"{parentName}.{name}";
            }
        }
        internal static Label BuildLabel(string parentName, string name, string tooltipType) {
            var text = BuildLabelText(parentName, name);
            var element = new Label(text)
            {
                tooltip = $"{tooltipType} {text}"
            };
            element.AddToClassList(LABEL_CLASS);
            return element;
        }
        internal static void ConstructTree(ValueTypeMemoryField target, FieldInfo fieldInfo, IntPtr root, string parentPath, int offset, Stack<Type> typeStack, RenderChildrenOptions options, string label = null) {
            if (fieldInfo.GetCustomAttribute<HiddenInValueTypeElement>() != null)
                return;
            var node = new FieldNode(root, parentPath, offset, fieldInfo);
            var fixedBufferAttr = fieldInfo.GetCustomAttribute<FixedBufferAttribute>();
            if (fixedBufferAttr != null) {
                if (fixedBufferAttr.ElementType == typeof(char)) {

                    if ((options & RenderChildrenOptions.RenderTerminalLabels) != 0) {
                        node.Add(BuildLabel(label, fieldInfo.Name, $"fixed char[{fixedBufferAttr.Length}]"));
                    }
                    node.Add(new CharFixedBufferNode(root, parentPath, offset + UnsafeUtility.GetFieldOffset(fieldInfo), fixedBufferAttr.Length));
                }
                else {
                    node.AddToClassList(CONTAINER_CLASS);
                    for (int i = 0; i < fixedBufferAttr.Length; i++) {
                        var value = Marshal.PtrToStructure(root + offset + (i * Marshal.SizeOf(fixedBufferAttr.ElementType)), fixedBufferAttr.ElementType);
                        var elementNode = new FixedBufferNode(parentPath, i, fixedBufferAttr.ElementType, offset);
                        if ((options & RenderChildrenOptions.RenderTerminalLabels) != 0) {
                            elementNode.Add(BuildLabel(label, $"{fieldInfo.Name}[{i}]", fieldInfo.FieldType.Name));
                        }
                        elementNode.Add(CreateTerminal(fixedBufferAttr.ElementType, value));

                        node.Add(elementNode);
                    }
                }
            }
            else {
                if (fieldInfo.FieldType.GetCustomAttribute<HiddenInValueTypeElement>() != null)
                    return;

                var initialValue = Marshal.PtrToStructure(root + offset + UnsafeUtility.GetFieldOffset(fieldInfo), fieldInfo.FieldType);

                var terminal = CreateTerminal(fieldInfo.FieldType, initialValue);
                if (terminal != null) {
                    if ((options & RenderChildrenOptions.RenderTerminalLabels) != 0) {
                        node.Add(BuildLabel(label, fieldInfo.Name, fieldInfo.FieldType.Name));
                    }
                    node.Add(terminal);
                }
                else {
                    if (fieldInfo.FieldType.TryGetTypeFieldDrawer(out ValueTypeFieldDrawer drawer)) {
                        terminal = drawer.CreateElement(fieldInfo.FieldType, initialValue);
                        if (terminal == null) {
                            return;
                        }
                        RegisterDrawerChange(terminal, fieldInfo.FieldType);
                        terminal.AddToClassList(TERMINAL_CLASS);
                        if ((options & RenderChildrenOptions.RenderTerminalLabels) != 0) {
                            node.Add(BuildLabel(label, fieldInfo.Name, fieldInfo.FieldType.Name));
                        }
                        node.Add(terminal);
                    }
                    else if ((options & RenderChildrenOptions.Render) != 0) {

                        if ((options & RenderChildrenOptions.RenderContainerLabels) != 0) {
                            node.Add(BuildLabel(label, fieldInfo.Name, fieldInfo.FieldType.Name));
                        }
                        node.AddToClassList(CONTAINER_CLASS);
                        var outerOffset = UnsafeUtility.GetFieldOffset(fieldInfo);
                        var path = $"{parentPath}.{fieldInfo.Name}";
                        foreach (var nestedFieldInfo in fieldInfo.FieldType.GetSerializableFields(f => f.GetCustomAttribute<HideInInspector>() == null)) {
                            if (typeStack.Contains(nestedFieldInfo.FieldType))
                                continue;
                            typeStack.Push(nestedFieldInfo.FieldType);
                            ConstructTree(node, nestedFieldInfo, root, path, offset + outerOffset, typeStack, options, BuildLabelText(label, fieldInfo.Name));
                            typeStack.Pop();
                        }
                        if (node.childCount == 0)
                            return;
                    }
                }
            }
            target.Add(node);
        }
        internal static void RegisterDrawerChange(VisualElement element, Type type) {

            typeof(ValueTypeElement).GetMethod(nameof(RegisterDrawerChangeGeneric), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(type).Invoke(null, new object[] { element });
        }
        internal static void RegisterDrawerChangeGeneric<T>(VisualElement element) {

            element.RegisterCallback<ChangeEvent<T>>((evt) => (evt.target as VisualElement)?.GetFirstAncestorOfType<ValueTypeMemoryField>()?.UpdateMemory(evt.newValue));
        }
        internal static VisualElement CreateTerminal(Type type, object initialValue) {
            if (type.IsEnum) {
                return CreateEnumElement(type, initialValue);
            }
            else {
                switch (Type.GetTypeCode(type)) {
                    case TypeCode.Boolean:
                        return CreateElement<Toggle, bool>((bool)initialValue);
                    case TypeCode.Char:
                        return CreateElement(new TextField { isDelayed = true, maxLength = 1 }, (string input, out char result) => char.TryParse(input, out result), ((char)initialValue).ToString());
                    case TypeCode.Byte:
                        return CreateElement(new TextField { isDelayed = true }, (string input, out byte result) => byte.TryParse(input, out result), ((byte)initialValue).ToString());
                    case TypeCode.UInt16:
                        return CreateElement(new TextField { isDelayed = true }, (string input, out ushort result) => ushort.TryParse(input, out result), ((ushort)initialValue).ToString());
                    case TypeCode.UInt32:
                        return CreateElement(new TextField { isDelayed = true }, (string input, out uint result) => uint.TryParse(input, out result), ((uint)initialValue).ToString());
                    case TypeCode.UInt64:
                        return CreateElement(new TextField { isDelayed = true }, (string input, out ulong result) => ulong.TryParse(input, out result), ((ulong)initialValue).ToString());
                    case TypeCode.SByte:
                        return CreateElement(new TextField { isDelayed = true }, (string input, out sbyte result) => sbyte.TryParse(input, out result), ((sbyte)initialValue).ToString());
                    case TypeCode.Int16:
                        return CreateElement(new TextField { isDelayed = true }, (string input, out short result) => short.TryParse(input, out result), ((short)initialValue).ToString());
                    case TypeCode.Int32:
                        return CreateElement(new TextField { isDelayed = true }, (string input, out int result) => int.TryParse(input, out result), ((int)initialValue).ToString());
                    case TypeCode.Int64:
                        return CreateElement(new TextField { isDelayed = true }, (string input, out long result) => long.TryParse(input, out result), ((long)initialValue).ToString());
                    case TypeCode.Single:
                        return CreateElement(new TextField { isDelayed = true }, (string input, out float result) => float.TryParse(input, out result), ((float)initialValue).ToString());
                    case TypeCode.Double:
                        return CreateElement(new TextField { isDelayed = true }, (string input, out double result) => double.TryParse(input, out result), ((double)initialValue).ToString());
                    default:
                        return null;
                }
            }
        }
        internal static TElement CreateElement<TElement, TElementValue, TValue>(TElement element, TypedFieldValueConverter<TElementValue, TValue> converter, TElementValue initial) where TElement : VisualElement, INotifyValueChanged<TElementValue> {

            element.RegisterValueChangedCallback((evt) =>
            {
                if (converter.Invoke(evt.newValue, out TValue result)) {
                    (evt.target as VisualElement)?.GetFirstAncestorOfType<ValueTypeMemoryField>()?.UpdateMemory(result);
                }
                else {
                    (evt.target as INotifyValueChanged<TElementValue>)?.SetValueWithoutNotify(evt.previousValue);
                }
            });
            element.SetValueWithoutNotify(initial);
            element.AddToClassList(TERMINAL_CLASS);
            return element;
        }
        internal unsafe static TElement CreateElement<TElement, TValue>(TValue initial) where TElement : VisualElement, INotifyValueChanged<TValue>, new() where TValue : struct {
            var element = new TElement();
            element.RegisterValueChangedCallback((evt) => (evt.target as VisualElement)?.GetFirstAncestorOfType<ValueTypeMemoryField>()?.UpdateMemory(evt.newValue));
            element.RegisterCallback<MemoryRefreshEvent>(evt =>
            {
                if (evt.target is VisualElement selfElement && selfElement is INotifyValueChanged<TValue> self) {
                    var memoryField = selfElement.GetFirstAncestorOfType<ValueTypeMemoryField>();
                    var root = selfElement.GetFirstAncestorOfType<ValueTypeElement>();
                    if (memoryField != null && root != null) {
                        self.SetValueWithoutNotify(Marshal.PtrToStructure<TValue>(root.Pointer + memoryField.Offset));
                    }
                }

            });
            element.SetValueWithoutNotify(initial);
            element.AddToClassList(TERMINAL_CLASS);
            return element;
        }
        internal unsafe static VisualElement CreateEnumElement(Type type, object initial) {
            BaseField<Enum> element;
            Enum initialValue = (Enum)initial;
            if (type.GetCustomAttribute<FlagsAttribute>() != null) {

                element = new EnumFlagsField(initialValue);

            }
            else {
                element = new EnumField(initialValue);
            }

            element.RegisterValueChangedCallback((evt) => (evt.target as VisualElement)?.GetFirstAncestorOfType<ValueTypeMemoryField>()?.UpdateMemory(evt.newValue));
            element.RegisterCallback<MemoryRefreshEvent>(evt =>
            {
                if (evt.target is VisualElement selfElement && selfElement is INotifyValueChanged<Enum> self) {
                    var memoryField = selfElement.GetFirstAncestorOfType<ValueTypeMemoryField>();
                    var root = selfElement.GetFirstAncestorOfType<ValueTypeElement>();
                    if (memoryField != null && root != null) {
                        self.SetValueWithoutNotify((Enum)Marshal.PtrToStructure(root.Pointer + memoryField.Offset, Enum.GetUnderlyingType(type)));
                    }
                }

            });
            element.SetValueWithoutNotify(initialValue);
            element.AddToClassList(TERMINAL_CLASS);
            return element;
        }



        public unsafe void Dispose() {
            if (pointer != IntPtr.Zero) {
                UnsafeUtility.Free(pointer.ToPointer(), Allocator.Persistent);
                pointer = IntPtr.Zero;
            }


        }

        protected override void UpdateMemory(int offset, int length, object value) {
            Marshal.StructureToPtr(value, pointer, true);
            using (MemoryTreeUpdateEvent updateEvent = MemoryTreeUpdateEvent.GetPooled(Pointer, offset, length)) {
                updateEvent.target = this;
                SendEvent(updateEvent);
            }
        }

        ~ValueTypeElement() {
            Dispose();
        }

        /// <summary>
        /// General Node for handling fields.
        /// </summary>
        public class FieldNode : ValueTypeMemoryField {
            private FieldInfo fieldInfo;
            public override Type Type { get => fieldInfo.FieldType; }

            public override int Offset { get; }
            private bool isDefault;
            public override bool IsDefault { get => isDefault; }

            public override int Length { get; }
            public override string Path { get; }

            public FieldNode(IntPtr root, string parentPath, int currentOffset, FieldInfo fieldInfo) {
                this.fieldInfo = fieldInfo;
                this.Path = $"{parentPath}.{fieldInfo.Name}";
                this.Offset = currentOffset + UnsafeUtility.GetFieldOffset(fieldInfo);
                this.Length = UnsafeUtility.SizeOf(fieldInfo.FieldType);
                isDefault = IsDefaultData(root, Offset, Length);
                this.AddToClassList(NODE_CLASS);
            }
            protected override void UpdateMemory(int offset, int length, object value) {
                var root = this.GetFirstAncestorOfType<ValueTypeElement>();
                if (root == null)
                    return;
                Marshal.StructureToPtr(value, root.pointer + offset, true);
                isDefault = IsDefaultData(root.Pointer, offset, length);
                using MemoryTreeUpdateEvent treeUpdateEvent = MemoryTreeUpdateEvent.GetPooled(root.Pointer, offset, length);
                using MemoryNodeUpdateEvent nodeUpdateEvent = MemoryNodeUpdateEvent.GetPooled(root.Pointer, offset, length);
                nodeUpdateEvent.target = this;
                treeUpdateEvent.target = root;
                SendEvent(treeUpdateEvent);
                SendEvent(nodeUpdateEvent);
            }

            public override string CreateLabelText() {
                var parent = this.GetFirstAncestorOfType<ValueTypeMemoryField>()?.CreateLabelText();
                if (string.IsNullOrEmpty(parent)) {
                    return fieldInfo.Name;
                }
                else {
                    return $"{parent}.{fieldInfo.Name}";
                }
            }
        }
        /// <summary>
        /// Specialized Node for handling fixed buffers.
        /// </summary>
        public class FixedBufferNode : ValueTypeMemoryField {
            public int Index { get; }
            public override int Offset { get; }
            public override int Length { get; }
            private bool isDefault;
            public override bool IsDefault { get => isDefault; }
            public override Type Type { get; }
            public override string Path { get; }
            public FixedBufferNode(string path, int index, Type bufferType, int offset) {
                this.Path = $"{path}[{index}]";
                this.Index = index;
                this.Type = bufferType;
                this.Offset = offset + (index * Marshal.SizeOf(bufferType));
                this.Length = Marshal.SizeOf(bufferType);

                this.AddToClassList(NODE_CLASS);
                this.AddToClassList(BUFFER_CLASS);

            }
            protected override void UpdateMemory(int offset, int length, object value) {
                var container = this.GetFirstAncestorOfType<ValueTypeMemoryField>();
                var root = this.GetFirstAncestorOfType<ValueTypeElement>();
                if (container != null && root != null) {
                    Marshal.StructureToPtr(value, root.pointer + offset, true);
                    isDefault = IsDefaultData(root.Pointer, offset, length);
                    using MemoryTreeUpdateEvent treeUpdateEvent = MemoryTreeUpdateEvent.GetPooled(root.Pointer, offset, length);
                    using MemoryNodeUpdateEvent nodeUpdateEvent = MemoryNodeUpdateEvent.GetPooled(root.Pointer, offset, length);
                    nodeUpdateEvent.target = container;
                    treeUpdateEvent.target = root;
                    SendEvent(treeUpdateEvent);
                    SendEvent(nodeUpdateEvent);
                }


            }
            public override string CreateLabelText() {
                var parent = this.GetFirstAncestorOfType<ValueTypeMemoryField>();
                if (parent != null) {
                    return $"{parent.CreateLabelText()}[{Index}]";
                }
                else {
                    return $"[{Index}]";
                }
            }
        }
        /// <summary>
        /// Specialized Node for char buffers.
        /// </summary>
        public class CharFixedBufferNode : ValueTypeMemoryField {
            public Type BufferType { get; private set; }

            public override int Offset { get; }
            private bool isDefault;
            public override bool IsDefault { get => isDefault; }
            public override int Length { get; }

            public override Type Type { get; } = typeof(char);
            private readonly byte[] stringBuffer;
            public override string Path { get; }

            public unsafe CharFixedBufferNode(IntPtr root, string path, int offset, int length) {
                this.Path = path;
                this.Length = length;
                this.Offset = offset;
                this.stringBuffer = new byte[length * 2];
                this.AddToClassList(NODE_CLASS);
                this.AddToClassList(BUFFER_CLASS);
                var element = new TextField { isDelayed = true, maxLength = length };
                element.SetValueWithoutNotify(Encoding.Unicode.GetString((byte*)(root + offset), length * 2));
                element.RegisterValueChangedCallback<string>(evt => UpdateMemory(evt.newValue));
                element.RegisterCallback<MemoryRefreshEvent>(evt =>
                {
                    if (evt.target is VisualElement selfElement && selfElement is INotifyValueChanged<string> self) {
                        var root = selfElement.GetFirstAncestorOfType<ValueTypeElement>();
                        if (root != null) {
                            element.SetValueWithoutNotify(Encoding.Unicode.GetString((byte*)root.Pointer + Offset, Length * 2));
                        }
                    }

                });
                element.AddToClassList(TERMINAL_CLASS);
                this.Add(element);
            }

            protected override void UpdateMemory(int offset, int length, object value) {
                var container = this.GetFirstAncestorOfType<ValueTypeMemoryField>();
                var root = this.GetFirstAncestorOfType<ValueTypeElement>();
                if (container != null && root != null && value is string strValue) {
                    Array.Clear(stringBuffer, 0, stringBuffer.Length);
                    Encoding.Unicode.GetBytes(value as string, 0, strValue.Length > Length ? Length : strValue.Length, stringBuffer, 0);
                    Marshal.Copy(stringBuffer, 0, root.pointer + offset, stringBuffer.Length);
                    isDefault = IsDefaultData(root.Pointer, offset, length * 2);
                    using MemoryTreeUpdateEvent treeUpdateEvent = MemoryTreeUpdateEvent.GetPooled(root.Pointer, offset, length * 2);
                    using MemoryNodeUpdateEvent nodeUpdateEvent = MemoryNodeUpdateEvent.GetPooled(root.Pointer, offset, length * 2);
                    nodeUpdateEvent.target = container;
                    treeUpdateEvent.target = root;
                    SendEvent(treeUpdateEvent);
                    SendEvent(nodeUpdateEvent);
                }

            }
            public override string CreateLabelText() {
                var parent = this.GetFirstAncestorOfType<ValueTypeMemoryField>();
                if (parent != null) {
                    return parent.CreateLabelText();
                }
                else {
                    return "";
                }
            }
        }
    }

    public class MemoryTreeUpdateEvent : EventBase<MemoryTreeUpdateEvent> {
        public IntPtr pointer;

        public int offset;

        public long length;

        public static MemoryTreeUpdateEvent GetPooled(IntPtr pointer, int offset, long length) {
            var evt = EventBase<MemoryTreeUpdateEvent>.GetPooled();
            evt.pointer = pointer;
            evt.offset = offset;
            evt.length = length;
            return evt;
        }
    }
    public class MemoryNodeUpdateEvent : EventBase<MemoryNodeUpdateEvent> {
        public IntPtr pointer;

        public int offset;

        public int length;

        public static MemoryNodeUpdateEvent GetPooled(IntPtr pointer, int offset, int length) {
            var evt = EventBase<MemoryNodeUpdateEvent>.GetPooled();
            evt.pointer = pointer;
            evt.offset = offset;
            evt.length = length;
            return evt;
        }
    }
    public class MemoryRefreshEvent : EventBase<MemoryRefreshEvent> {

    }
}
