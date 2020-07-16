using System;
using System.Collections.Generic;

namespace Reactics.Editor.Graph {
    public class ObjectGraphNodeSingleRootComparator : IComparer<ObjectGraphNode> {
        public int Compare(ObjectGraphNode x, ObjectGraphNode y) {
            var xInput = x.InputPort.connected;
            var yInput = y.InputPort.connected;
            if (xInput == yInput) {
                return x.viewDataKey.CompareTo(y.viewDataKey);
            }
            else {
                return -(Convert.ToInt32(yInput) - Convert.ToInt32(xInput));
            }
        }
    }
}