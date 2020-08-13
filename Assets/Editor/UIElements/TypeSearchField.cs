using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Core.Editor {
    public class TypeSearchField : SearchField<Type> {

        private readonly HashSet<Func<Type, bool>> typeFilters = new HashSet<Func<Type, bool>>();
        private List<Type> typeCandidates = null;

        public TypeSearchField() : base() {

        }

        public TypeSearchField(string label) : base(label) {

        }
        public override string NoValueDisplayText { get => "None (Type)"; }

        public bool AddTypeFilter(Func<Type, bool> filter) {
            if (typeFilters.Add(filter)) {

                if (typeCandidates == null) {
                    typeCandidates = new List<Type>();
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                        foreach (var type in assembly.GetTypes()) {
                            if (IsValid(type))
                                typeCandidates.Add(type);

                        }
                    }
                }
                else {
                    for (int i = 0; i < typeCandidates.Count; i++) {
                        if (!IsValid(typeCandidates[i])) {
                            typeCandidates.RemoveAt(i);
                            i--;
                        }
                    }
                }
                return true;
            }
            else
                return false;
        }
        public bool RemoveTypeFilter(Func<Type, bool> filter) {
            if (typeFilters.Remove(filter)) {
                if (typeFilters.Count == 0) {
                    typeCandidates = null;
                }
                else {
                    typeCandidates.Clear();
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                        foreach (var type in assembly.GetTypes()) {
                            if (IsValid(type))
                                typeCandidates.Add(type);

                        }
                    }
                }
                return true;
            }
            else {
                return false;
            }
        }
        public bool ContainsTypeFilter(Func<Type, bool> filter) {
            return typeFilters.Contains(filter);
        }
        public bool IsValid(Type type) {
            foreach (var filter in typeFilters) {
                if (!filter(type))
                    return false;
            }
            return true;
        }

        public override IEnumerable<ISearchResult<Type>> OnSearch(string newQuery, string oldQuery, int max) {
            if (string.IsNullOrEmpty(newQuery))
                yield break;
            int count = 0;
            if (typeCandidates == null) {

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                    foreach (var type in assembly.GetTypes()) {
                        if (max > 0 && count >= max)
                            yield break;
                        if (type.Name.StartsWith(newQuery) || type.FullName.StartsWith(newQuery)) {
                            count++;
                            yield return new SearchResult(type);

                        }
                    }
                }
            }
            else {
                foreach (var type in typeCandidates) {
                    if (max > 0 && count >= max)
                        yield break;
                    if (type.Name.StartsWith(newQuery) || type.FullName.StartsWith(newQuery)) {
                        count++;
                        yield return new SearchResult(type);
                    }

                }

            }


        }

        protected override ISearchResult<Type> CreateFromObject(Type obj) {
            if (obj == null)
                return null;
            else
                return new SearchResult(obj);
        }

        public struct SearchResult : ISearchResult<Type> {
            private Type type;

            public string DisplayText => type.FullName;

            public SearchResult(Type type) {
                this.type = type;
            }
            public Type LoadValue() => type;

            public SearchResultElement<Type> CreateElement() {
                return new TypeLabel(this, type.FullName);
            }

        }
        public class TypeLabel : SearchResultElement<Type> {
            public override ISearchResult<Type> Value { get; set; }
            private Label labelElement;
            public string Label { get => labelElement?.text; set => labelElement.text = value; }
            public TypeLabel(string label = null) : base() {
                Init(label);
            }

            public TypeLabel(ISearchResult<Type> value, string label = null) {
                Init(label);
                Value = value;

            }

            private void Init(string text = null) {
                labelElement = new Label(text);
                this.Add(labelElement);
            }
        }
    }
}