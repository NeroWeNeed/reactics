using System.Reflection;

namespace NeroWeNeed.Commons.Editor {
    public struct TypeTreeNode {
        public string fieldName;
        public int offset;
        public int length;
        public TypeTreeNode[] children;
        public static void Create(Type type, BindingFlags flags) {



        }

    }

}