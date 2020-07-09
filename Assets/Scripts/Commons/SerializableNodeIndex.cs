using System;
using System.Collections.Generic;

namespace Reactics.Commons
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SerializeNodeIndex : Attribute
    {
        public Type nodeType;

        public SerializeNodeIndex(Type nodeType)
        {
            this.nodeType = nodeType;
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class AliasHandler : Attribute
    {

        public Type type;

        public AliasHandler(Type type)
        {
            this.type = type;
        }
    }
    public static class AliasHandlers
    {
        private static Dictionary<Type, object> aliasHandlers = new Dictionary<Type, object>();
        public static object ToAlias(object original, AliasHandler aliasHandler, object data)
        {
            object handler;
            if (!aliasHandlers.TryGetValue(aliasHandler.type, out handler))
            {
                handler = Activator.CreateInstance(aliasHandler.type);
                aliasHandlers[aliasHandler.type] = handler;
            }
            return ((BaseAliasHandler)handler).ToAlias(original, data);
        }
        public static object ToOriginal(object alias, AliasHandler aliasHandler, object data)
        {
            object handler;
            if (!aliasHandlers.TryGetValue(aliasHandler.type, out handler))
            {
                handler = Activator.CreateInstance(aliasHandler.type);
                aliasHandlers[aliasHandler.type] = handler;
            }
            return ((BaseAliasHandler)handler).ToOriginal(alias, data);
        }
    }
    public abstract class BaseAliasHandler
    {
        public abstract object ToAlias(object original, object data);

        public abstract object ToOriginal(object alias, object data);
    }
    public abstract class BaseAliasHandler<TOriginal, TAlias> : BaseAliasHandler
    {
        public abstract TAlias ToAlias(TOriginal original, object data);

        public abstract TOriginal ToOriginal(TAlias alias, object data);

        public override object ToAlias(object original, object data) => ToAlias((TOriginal)original, data);
        public override object ToOriginal(object alias, object data) => ToOriginal((TAlias)alias, data);
    }

    [Serializable]
    [AliasHandler(typeof(IndexReferenceAliasHandler))]
    public struct IndexReference
    {
        public int index;
    }
    public class IndexReferenceAliasHandler : BaseAliasHandler<IndexReference, NodeReference>
    {
        public override NodeReference ToAlias(IndexReference original, object data)
        {
            return new NodeReference();
        }

        public override IndexReference ToOriginal(NodeReference alias, object data)
        {
            return new IndexReference();
        }
    }
    public struct NodeReference
    {
        public string nodeId;
    }
}