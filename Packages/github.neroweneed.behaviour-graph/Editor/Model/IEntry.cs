using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NeroWeNeed.BehaviourGraph.Editor.Model {
    public interface IEntry : IComparable<IEntry> {
        public string Id { get; set; }
        public Rect Layout { get; set; }
        public int Priority { get; }
        public Node CreateNode(BehaviourGraphView graphView, BehaviourGraphSettings settings);


    }
}