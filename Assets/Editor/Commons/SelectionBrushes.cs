using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NeroWeNeed.Commons;
using Reactics.Core.Commons;
using Reactics.Core.Map;
using Unity.Mathematics;
using UnityEngine;

namespace Reactics.Editor {
    public class SelectionContext<T> {
        private readonly List<T> selected = new List<T>();
        public readonly ReadOnlyCollection<T> Selected;
        private readonly List<Action<SelectionChangedEvent<T>>> listeners;
        public SelectionContext() {
            Selected = selected.AsReadOnly();
            listeners = new List<Action<SelectionChangedEvent<T>>>();
        }
        public void Add(params T[] elements) {
            Add((IEnumerable<T>)elements);
        }
        public void Add(IEnumerable<T> elements) {
            var added = elements.Where((element) =>
            {
                if (!selected.Contains(element)) {
                    selected.Add(element);
                    return true;
                }
                else
                    return false;
            }).ToArray();

            if (added.Length > 0)
                Notify(new SelectionChangedEvent<T>(added, Array.Empty<T>(), Selected));

        }
        public void Remove(params T[] elements) {
            Remove((IEnumerable<T>)elements);
        }
        public void Remove(IEnumerable<T> elements) {
            var removed = elements.Where((element) =>
            {
                if (selected.Remove(element))
                    return true;
                else
                    return false;
            }).ToArray();
            if (removed.Length > 0)
                Notify(new SelectionChangedEvent<T>(Array.Empty<T>(), removed, Selected));
        }
        public void Set(params T[] elements) {
            Set((IEnumerable<T>)elements);
        }
        public void Set(IEnumerable<T> elements) {

            var removed = selected.Where((element) =>
            {
                if (!elements.Contains(element))
                    return true;
                else
                    return false;
            }).ToArray();
            foreach (var item in removed) {
                selected.Remove(item);
            }

            var added = elements.Where((element) =>
            {
                if (!selected.Contains(element)) {
                    selected.Add(element);
                    return true;
                }
                else
                    return false;
            }).ToArray();
            if (removed.Length > 0 || added.Length > 0)
                Notify(new SelectionChangedEvent<T>(added, removed, Selected));
        }
        public void Update(IEnumerable<T> add, IEnumerable<T> remove) {

            var removed = remove.Where((element) => selected.Remove(element)).ToArray();

            var added = add.Where((element) =>
            {
                if (!selected.Contains(element)) {
                    selected.Add(element);
                    return true;
                }
                else
                    return false;
            }).ToArray();
            if (removed.Length > 0 || added.Length > 0)
                Notify(new SelectionChangedEvent<T>(added, removed, Selected));
        }
        public void Clear() {
            if (selected.Count > 0) {

                var old = selected.ToArray();
                selected.Clear();
                Notify(new SelectionChangedEvent<T>(Array.Empty<T>(), old, Selected));
            }
        }
        private void Notify(SelectionChangedEvent<T> changedEvent) {
            foreach (var listener in listeners) {
                listener.Invoke(changedEvent);
            }
        }
        public void AddListener(Action<SelectionChangedEvent<T>> listener) {

            this.listeners.Add(listener);
        }
        public void RemoveListener(Action<SelectionChangedEvent<T>> listener) {

            this.listeners.Remove(listener);
        }

    }
    public struct SelectionChangedEvent<T> {
        public readonly T[] added, removed;

        public readonly ReadOnlyCollection<T> current;

        public SelectionChangedEvent(T[] added, T[] removed, ReadOnlyCollection<T> current) {
            this.added = added;
            this.removed = removed;
            this.current = current;
        }
    }
    public class SelectionManager {
        public readonly SelectionContext<Point> context = new SelectionContext<Point>();

        public SelectionBrush CurrentBrush { get; internal set; }
        public T StartStroke<T>(Point point, bool clearSelection = true) where T : SelectionBrush => StartStroke(point, typeof(T), clearSelection) as T;
        public SelectionBrush StartStroke(Point point, Type type, bool clearSelection = true) {
            if (CurrentBrush != null)
                throw new InvalidOperationException("Cannot start another stroke while a stroke is active.");
            if (!typeof(SelectionBrush).IsAssignableFrom(type))
                throw new ArgumentException("Not a subclass of BaseSelectionBrush");
            if (clearSelection)
                context.Clear();
            return DoStartStroke(point, type, clearSelection);
        }
        public bool TryStartStroke<T>(Point point, out T brush, bool clearSelection = true) where T : SelectionBrush {

            var r = TryStartStroke(point, out SelectionBrush selectionBrush, typeof(T), clearSelection);
            brush = selectionBrush as T;
            return r;
        }
        public bool TryStartStroke(Point point, out SelectionBrush brush, Type type, bool clearSelection = true) {
            if (CurrentBrush != null || !typeof(SelectionBrush).IsAssignableFrom(type)) {
                brush = null;
                return false;

            }
            brush = DoStartStroke(point, type, clearSelection);
            return true;
        }
        public bool TryStartStroke<T>(Point point, bool clearSelection = true) => TryStartStroke(point, typeof(T), clearSelection);
        public bool TryStartStroke(Point point, Type type, bool clearSelection = true) {
            if (CurrentBrush != null || !typeof(SelectionBrush).IsAssignableFrom(type)) {

                return false;
            }
            DoStartStroke(point, type, clearSelection);
            return true;
        }
        private SelectionBrush DoStartStroke(Point point, Type type, bool clearSelection) {
            if (clearSelection)
                context.Clear();
            var brush = Activator.CreateInstance(type) as SelectionBrush;
            brush.Context = context;
            brush.Manager = this;
            CurrentBrush = brush;
            brush.StartStroke(point);
            return brush;
        }
        public void UpdateStroke(Point point) {
            if (CurrentBrush == null)
                throw new InvalidOperationException("No Stroke is active.");
            CurrentBrush.UpdateStroke(point);
        }
        public bool TryUpdateStroke(Point point) {
            if (CurrentBrush == null)
                return false;
            CurrentBrush.UpdateStroke(point);
            return true;
        }
        public void EndStroke() {
            if (CurrentBrush == null)
                throw new InvalidOperationException("No Stroke is active.");
            CurrentBrush.EndStroke();
        }
        public bool TryEndStroke() {
            if (CurrentBrush == null)
                return false;
            CurrentBrush.EndStroke();
            return true;
        }


    }
    public abstract class SelectionBrush : IDisposable {
        public SelectionContext<Point> Context { get; internal set; }

        public SelectionManager Manager { get; internal set; }
        public void StartStroke(Point point) => OnStartStroke(point);

        protected abstract void OnStartStroke(Point point);
        public void UpdateStroke(Point point) => OnUpdateStroke(point);

        protected abstract void OnUpdateStroke(Point point);

        public void EndStroke() {



            Manager.CurrentBrush = null;
            Manager = null;
            Context = null;
            Dispose();
        }



        public abstract void Dispose();
    }

    public class PencilSelectionBrush : SelectionBrush {
        public override void Dispose() { }



        protected override void OnStartStroke(Point point) {
            Context.Add(point);
        }

        protected override void OnUpdateStroke(Point point) {
            Context.Add(point);
        }
    }
    public abstract class ShapeSelectionBrush : SelectionBrush {
        public override void Dispose() { }

        protected Point initial;
        protected List<Point> lastSelection = new List<Point>();
        protected List<Point> selectionBuffer = new List<Point>();

        protected List<Point> added = new List<Point>();
        protected List<Point> removed = new List<Point>();

        protected override void OnStartStroke(Point point) {
            initial = point;
            Context.Add(point);
        }

        protected override void OnUpdateStroke(Point point) {
            MakeRect(initial, point, out ushort left, out ushort top, out ushort right, out ushort bottom);
            selectionBuffer.Clear();
            CollectPoints(left, top, right, bottom, selectionBuffer);
            Difference(lastSelection, selectionBuffer, added, removed);
            lastSelection.Clear();
            lastSelection.AddRange(selectionBuffer);
            Context.Update(added, removed);
        }
        protected void Difference(List<Point> selectionA, List<Point> selectionB, List<Point> addBuffer, List<Point> removeBuffer) {
            removeBuffer.Clear();
            addBuffer.Clear();
            foreach (var item in selectionA) {
                if (!selectionB.Contains(item))
                    removeBuffer.Add(item);
            }
            foreach (var item in selectionB) {
                if (!selectionA.Contains(item))
                    addBuffer.Add(item);
            }





        }
        protected abstract void CollectPoints(ushort left, ushort top, ushort right, ushort bottom, List<Point> output);
        protected void MakeRect(Point pointA, Point pointB, out ushort left, out ushort top, out ushort right, out ushort bottom) {

            if (pointA.x > pointB.x) {
                left = pointB.x;
                right = pointA.x;
            }
            else {
                left = pointA.x;
                right = pointB.x;
            }
            if (pointA.y > pointB.y) {
                top = pointA.y;
                bottom = pointB.y;
            }
            else {
                top = pointB.y;
                bottom = pointA.y;
            }
        }

    }

    public class SolidRectSelectionBrush : ShapeSelectionBrush {
        protected override void CollectPoints(ushort left, ushort top, ushort right, ushort bottom, List<Point> output) {
            for (int y = bottom; y <= top; y++) {
                for (int x = left; x <= right; x++) {
                    selectionBuffer.Add(new Point(x, y));
                }
            }
        }
    }
    public class OutlineRectSelectionBrush : ShapeSelectionBrush {
        protected override void CollectPoints(ushort left, ushort top, ushort right, ushort bottom, List<Point> output) {
            Debug.Log(bottom);
            for (int x = left; x <= right; x++) {
                selectionBuffer.Add(new Point(x, top));
                selectionBuffer.Add(new Point(x, bottom));
            }
            for (int y = bottom; y <= top; y++) {
                selectionBuffer.Add(new Point(left, y));
                selectionBuffer.Add(new Point(right, y));
            }
        }
    }

    public class SolidEllipseSelectionBrush : ShapeSelectionBrush {
        protected override void OnStartStroke(Point point) {
            initial = point;

        }
        protected override void CollectPoints(ushort left, ushort top, ushort right, ushort bottom, List<Point> output) {
            var xRadius = (right - left) / 2;
            var yRadius = (top - bottom) / 2;
            for (int y = bottom; y <= top; y++) {
                for (int x = left; x <= right; x++) {
                    if (MathCommons.WithinEllipse(xRadius, yRadius, x - left - xRadius, y - bottom - yRadius, true))
                        output.Add(new Point(x, y));

                }
            }

        }

    }

    public class OutlineEllipseSelectionBrush : ShapeSelectionBrush {
        protected override void OnStartStroke(Point point) {
            initial = point;

        }
        protected override void CollectPoints(ushort left, ushort top, ushort right, ushort bottom, List<Point> output) {
            /*             var xRadius = (right - left) / 2f;
                        var yRadius = (top - bottom) / 2f;

                        for (int y = bottom; y <= top; y++)
                        {
                            for (int x = left; x <= right; x++)
                            {

                                MathCommons.GetEllipseIntersection(xRadius, yRadius, x - left - xRadius, y - bottom - yRadius, out float xIntersection, out float yIntersection);
                                if (xIntersection == 0 && yIntersection == 0)
                                    continue;
                                if (x == (ushort)(left + xIntersection+xRadius) && y == (ushort)(bottom + yIntersection+yRadius))
                                    output.Add(new Point(x, y));

                            }
                        }
             */
        }

    }

}