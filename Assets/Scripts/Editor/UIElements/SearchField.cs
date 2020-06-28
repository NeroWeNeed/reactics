using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor
{

    public abstract class SearchField<TObject> : BindableElement, INotifyValueChanged<TObject>
    {

        private const string USS_GUID = "f0274f1bbd6a3fe4a9756c4a1335c8c9";

        private VisualElement container;
        private Label labelElement;
        private Button searchButton;

        private Button cancelButton;
        private TextField textField;
        private SearchFieldResults searchFieldResults;

        public virtual string NoValueDisplayText { get => string.Empty; }
        private TObject _value;
        public TObject value
        {
            get => _value; set
            {
                if (!EqualityComparer<TObject>.Default.Equals(value, _value))
                {
                    if (panel != null)
                    {
                        using (ChangeEvent<TObject> evt = ChangeEvent<TObject>.GetPooled(_value, value))
                        {
                            evt.target = this;
                            SetValueWithoutNotify(value);
                            SendEvent(evt);
                        }
                    }
                    else
                    {
                        SetValueWithoutNotify(value);
                    }
                }
            }
        }
        public string label
        {
            get => labelElement.text;
            set
            {
                if (labelElement.text != value)
                {
                    labelElement.text = value;
                    if (string.IsNullOrEmpty(value))
                    {
                        labelElement.visible = false;
                        labelElement.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        labelElement.visible = true;
                        labelElement.style.display = DisplayStyle.Flex;
                    }
                }
            }
        }


        private CancellationTokenSource searchTaskTokenSource;
        private ISearchResult<TObject> _selectedResult;
        private ISearchResult<TObject> selectedResult
        {
            get => _selectedResult; set
            {
                if (!EqualityComparer<ISearchResult<TObject>>.Default.Equals(value, _selectedResult))
                {
                    _selectedResult = value;
                    if (EqualityComparer<ISearchResult<TObject>>.Default.Equals(_selectedResult, default))
                    {
                        this.value = default;
                    }
                    else
                    {
                        this.value = _selectedResult.LoadValue();
                    }

                }
            }
        }
        private void SetDisplayText()
        {
            if (selectedResult == null)
            {
                textField.SetValueWithoutNotify(NoValueDisplayText);
            }
            else
            {
                textField.SetValueWithoutNotify(selectedResult.DisplayText);
            }
        }
        public SearchField() : base()
        {
            Init(null);
        }

        public SearchField(string label) : base()
        {
            Init(label);
        }
        private void Init(string label)
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(USS_GUID));
            this.styleSheets.Add(styleSheet);
            container = new VisualElement();
            labelElement = new Label()
            {
                name = "search-label"
            };
            this.label = label;
            searchButton = new Button
            {
                name = "unity-search"
            };
            searchButton.AddToClassList("unity-text-element");
            searchButton.AddToClassList("unity-button");
            searchButton.AddToClassList("unity-search-field-base__search-button");
            cancelButton = new Button
            {
                name = "unity-cancel"
            };
            cancelButton.AddToClassList("unity-text-element");
            cancelButton.AddToClassList("unity-button");
            cancelButton.AddToClassList("unity-search-field-base__cancel-button");
            cancelButton.AddToClassList("unity-search-field-base__cancel-button--off");
            cancelButton.clicked += OnCancelButton;
            textField = new TextField
            {
                name = "search-textfield",
                value = NoValueDisplayText
            };
            textField.AddToClassList("unity-base-field");
            textField.AddToClassList("unity-base-text-field");
            textField.AddToClassList("unity-text-field");
            textField.RegisterValueChangedCallback(OnTextChanged);
            var textInputBase = textField.Q<VisualElement>("unity-text-input");
            textInputBase.AddToClassList("unity-text-field__input");
            searchFieldResults = new SearchFieldResults(this);
            this.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            this.Add(labelElement);
            this.Add(container);
            this.container.Add(searchButton);
            this.container.Add(textField);
            this.container.Add(cancelButton);
            this.container.AddToClassList("unity-search-field-base");
            this.container.name = "search-field-container";
            this.container.RegisterCallback<FocusInEvent>(OnFocusGained);
            this.container.RegisterCallback<FocusOutEvent>(OnFocusLost);
            this.name = "search-field";

        }
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (searchFieldResults != null)
            {
                searchFieldResults.transform.position = new Vector3(this.worldBound.xMin, this.worldBound.yMax - 2, 0);
                searchFieldResults.UpdateLayout(evt.newRect);
            }
        }
        private void OnFocusGained(FocusInEvent evt)
        {
            searchFieldResults.visible = true;
            if (!EqualityComparer<ISearchResult<TObject>>.Default.Equals(selectedResult, default))
            {
                UpdateSearchResults(selectedResult.DisplayText, string.Empty);
            }
            else
            {
                UpdateSearchResults(null, string.Empty);
                textField.value = string.Empty;
            }
        }
        private void OnFocusLost(FocusOutEvent evt)
        {
            searchFieldResults.visible = false;
            searchFieldResults.ClearSearchResults();
            SetDisplayText();

        }
        private void UpdateCancelButton()
        {
            if (EqualityComparer<TObject>.Default.Equals(value, default))
            {
                if (!cancelButton.ClassListContains("unity-search-field-base__cancel-button--off"))
                    cancelButton.AddToClassList("unity-search-field-base__cancel-button--off");
            }
            else
            {
                if (cancelButton.ClassListContains("unity-search-field-base__cancel-button--off"))
                    cancelButton.RemoveFromClassList("unity-search-field-base__cancel-button--off");
            }
        }
        private async void OnTextChanged(ChangeEvent<string> evt)
        {

            if (string.IsNullOrEmpty(evt.newValue))
            {

                if (searchFieldResults.panel != null)
                {
                    searchFieldResults.RemoveFromHierarchy();
                    //this.panel.visualTree.Remove(searchFieldResults);
                }
            }
            else
            {

                if (searchFieldResults.panel == null)
                {

                    this.panel.visualTree.Add(searchFieldResults);
                    searchFieldResults.style.position = Position.Absolute;
                    searchFieldResults.BringToFront();
                }
                var newQuery = string.Copy(evt.newValue);
                var oldQuery = string.Copy(evt.previousValue);

                UpdateSearchResults(newQuery, oldQuery);

            }
        }
        private async void UpdateSearchResults(string newQuery, string oldQuery)
        {
            var context = TaskScheduler.FromCurrentSynchronizationContext();
            var tokenSource = new CancellationTokenSource();

            if (searchTaskTokenSource != null)
            {

                searchTaskTokenSource.Cancel();
            }
            searchTaskTokenSource = tokenSource;
            try
            {
                await Task.Run(() => CollectSearch(newQuery, oldQuery), tokenSource.Token).ContinueWith((results) =>
                 {
                     this.searchFieldResults.ClearSearchResults();
                     foreach (var result in results.Result)
                     {
                         searchFieldResults.AddSearchResult(result);
                     }

                 }, tokenSource.Token, TaskContinuationOptions.NotOnCanceled | TaskContinuationOptions.NotOnFaulted, context);


            }
            catch (TaskCanceledException e)
            {

            }
            finally
            {
                if (searchTaskTokenSource == tokenSource)
                {
                    searchTaskTokenSource = null;
                }
                tokenSource.Dispose();
            }
        }
        private IEnumerable<ISearchResult<TObject>> CollectSearch(string newQuery, string oldQuery)
        {
            List<ISearchResult<TObject>> results = new List<ISearchResult<TObject>>();

            foreach (var item in OnSearch(newQuery, oldQuery, 100))
            {
                results.Add(item);
            }
            return results;
        }
        private void OnCancelButton()
        {
            selectedResult = null;

            UpdateCancelButton();

        }

        public void SetValueWithoutNotify(TObject newValue)
        {
            _value = newValue;
            _selectedResult = CreateFromObject(newValue);
            SetDisplayText();
            UpdateCancelButton();
        }

        public abstract IEnumerable<ISearchResult<TObject>> OnSearch(string newQuery, string oldQuery, int max = -1);



        protected abstract ISearchResult<TObject> CreateFromObject(TObject obj);


        protected class SearchFieldResults : VisualElement
        {
            protected ScrollView scrollView;
            private SearchField<TObject> searchField;
            private const string USS_GUID = "faa35cf9bb7d7fd4ea68fcb946b7c4b5";

            private const float HEIGHT = 200;

            public SearchFieldResults(SearchField<TObject> searchField)
            {
                this.searchField = searchField;
                this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(USS_GUID)));


                Init();
            }
            public void UpdateLayout(Rect searchFieldRect)
            {
                this.style.width = searchFieldRect.width;
                scrollView.contentViewport.style.width = searchFieldRect.width;
                scrollView.contentContainer.style.width = searchFieldRect.width;
            }
            private void Init()
            {
                this.name = "search-field-results";
                scrollView = new ScrollView();
                this.Add(scrollView);
                this.style.position = Position.Absolute;
                this.scrollView.style.flexShrink = 0;
                this.style.overflow = Overflow.Hidden;
                this.style.maxHeight = HEIGHT;
                scrollView.contentContainer.style.flexShrink = 0;
                scrollView.contentContainer.style.flexGrow = 1;
                scrollView.contentViewport.style.flexGrow = 0;
                scrollView.contentViewport.style.flexShrink = 0;
                scrollView.contentViewport.style.maxHeight = this.style.maxHeight;
                scrollView.contentViewport.style.width = this.style.width;
                scrollView.showVertical = false;

            }
            public void ClearSearchResults()
            {
                this.scrollView.contentContainer.Clear();
            }
            public void AddSearchResult(ISearchResult<TObject> searchResult)
            {
                var element = searchResult.CreateElement();
                element.AddToClassList("search-result");
                element.style.textOverflow = TextOverflow.Ellipsis;
                //element.style.width = this.style.width;
                element.style.overflow = Overflow.Hidden;
                element.style.whiteSpace = WhiteSpace.NoWrap;
                element.userData = searchResult;
                this.scrollView.contentContainer.Add(element);
                element.RegisterCallback<MouseDownEvent>(OnSearchResultSelect);
            }
            private void OnSearchResultSelect(MouseDownEvent evt)
            {

                if (evt.target is VisualElement visualElement && visualElement.FindAncestorUserData() is ISearchResult<TObject> searchResult)
                {
                    searchField.selectedResult = searchResult;

                }
            }
        }
    }

    public interface ISearchResult<TObject>
    {
        VisualElement CreateElement();
        TObject LoadValue();
        string DisplayText { get; }
    }
}