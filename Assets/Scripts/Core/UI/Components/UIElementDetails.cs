using System;
using System.Collections.Generic;
using Reactics.Core.Commons;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Reactics.Core.UI {
    public struct UIElementDetails : IComponentData {

        private Layout layout;
        public Layout Layout { get => layout; set => layout = value; }
        private UILength4 padding;
        public UILength4 Padding { get => padding; set => padding = value; }
        private UILength4 borderWidth;
        public UILength4 BorderWidth { get => borderWidth; set => borderWidth = value; }
        private UILength4 margin;
        public UILength4 Margin { get => margin; set => margin = value; }
        private Spacing spacing;
        public Spacing Spacing { get => spacing; set => spacing = value; }
        private Alignment alignSelf;
        public Alignment AlignSelf { get => alignSelf; set => alignSelf = value; }
        private Alignment alignChildren;
        public Alignment AlignChildren { get => alignChildren; set => alignChildren = value; }
        private bool wrap;
        public bool Wrap { get => wrap; set => wrap = value; }
    }

}