using System.Linq;
using System.Reflection;
using System;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace Reactics.Editor
{

    public class TypeSelector : VisualElement, INotifyValueChanged<Type>
    {

        private readonly Dictionary<string, Type> types = new Dictionary<string, Type>();

        private TextField searchFieldElement;
        private Label labelElement;

        private VisualElement suggestionsElement;

        private StyleSheet styleSheet;
        private Func<Type, bool> typeValidator;
        private int maxResults = 32;

        public TypeSelector() : base()
        {
            Initialize();
        }
        public TypeSelector(Type initial) : base()
        {
            value = initial;
            Initialize();
        }
        public TypeSelector(Type initial = null, string label = null, Func<Type, bool> validator = null) : base()
        {
            value = initial;
            typeValidator = validator;
            this.label = label;
            Initialize();
        }
        public TypeSelector(string label = null, Func<Type, bool> validator = null) : base()
        {

            typeValidator = validator;
            this.label = label;
            Initialize();
        }
        public TypeSelector(Type initial = null, Func<Type, bool> validator = null) : base()
        {
            value = initial;
            typeValidator = validator;

            Initialize();
        }
        public TypeSelector(Func<Type, bool> validator) : base()
        {
            typeValidator = validator;
            Initialize();
        }
        private void Initialize()
        {
            searchFieldElement = new TextField();
            suggestionsElement = new VisualElement();

            suggestionsElement.AddToClassList("reactics-type-suggestions");
            searchFieldElement.AddToClassList("reactics-type-search");
            this.AddToClassList("reactics-type-selector");
            this.AddToClassList("unity-base-field");
            this.AddToClassList("unity-base-text-field");
            this.AddToClassList("unity-text-field");
            styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Resources/Editor/TypeSelector.uss");
            types.Clear();
            if (typeValidator != null)
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (typeValidator.Invoke(type))
                            types[type.FullName] = type;
                    }

                }
            else
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        types[type.FullName] = type;
                    }
                }
            searchFieldElement.RegisterValueChangedCallback(change =>
            {
                if (TrySetValue(change.newValue))
                {
                    suggestionsElement.Clear();
                    suggestionsElement.visible = false;
                }
                else
                {
                    suggestionsElement.Clear();
                    if (change.newValue != null && change.newValue != String.Empty)
                    {

                        foreach (var type in types)
                        {
                            if (type.Key.StartsWith(change.newValue))
                            {
                                suggestionsElement.Add(new Suggestion(type.Value, this));


                            }
                            if (suggestionsElement.childCount > maxResults)
                                break;
                        }
                    }

                    suggestionsElement.visible = suggestionsElement.childCount != 0;
                }


            });
            searchFieldElement.RegisterCallback<FocusInEvent>(focus =>
            {
                VisualElement root = this;
                while (root.parent != null)
                    root = root.parent;
                suggestionsElement.RemoveFromHierarchy();
                foreach (var child in root.Children())
                {
                    if (child.name.StartsWith("rootVisualContainer"))
                    {
                        child.Add(suggestionsElement);
                        break;
                    }
                }
                UpdateSuggestionsBoxLayout(suggestionsElement);
            });
            searchFieldElement.RegisterCallback<GeometryChangedEvent>(geometry =>
            {
                UpdateSuggestionsBoxLayout(suggestionsElement);
            });
            searchFieldElement.RegisterCallback<FocusOutEvent>(focus =>
            {
                suggestionsElement.visible = false;
            });

            suggestionsElement.style.position = Position.Absolute;


            suggestionsElement.styleSheets.Add(styleSheet);
            this.styleSheets.Add(styleSheet);
            this.Add(searchFieldElement);
        }

        private void UpdateSuggestionsBoxLayout(VisualElement suggestions)
        {

            suggestions.transform.position = new Vector3(searchFieldElement.worldBound.x, searchFieldElement.worldBound.y - 5, 0);
            suggestions.style.width = searchFieldElement.worldBound.width;
            if (_value == null)
            {
                Debug.Log(value);
                suggestions.visible = suggestions.childCount != 0;
                suggestions.BringToFront();
            }
        }
        public bool IsValid() => _value != null;
        public void SetValueWithoutNotify(Type newValue)
        {

            _value = newValue;
            searchFieldElement.SetValueWithoutNotify(newValue.FullName);

        }
        

        [SerializeField]
        private Type _value;


        private string _label;

        public string label
        {
            get => _label;
            set
            {
                if (value == null || value == string.Empty)
                {
                    if (labelElement != null)
                    {

                        labelElement.style.display = DisplayStyle.None;
                    }
                }
                else
                {
                    if (labelElement == null)
                    {
                        labelElement = new Label();
                        this.Add(labelElement);
                        labelElement.AddToClassList("reactics-type-label");
                        labelElement.AddToClassList("unity-base-text-field__label");
                        labelElement.AddToClassList("unity-base-field__label");
                        labelElement.AddToClassList("unity-text-field__label");
                        labelElement.SendToBack();
                    }
                    labelElement.text = value;
                    labelElement.style.display = DisplayStyle.Flex;
                }

            }
        }
        public Type value
        {
            get => _value;
            set
            {
                if (value != _value && (typeValidator == null || typeValidator.Invoke(value)))
                {
                    if (panel != null)
                    {

                        using (ChangeEvent<Type> evt = ChangeEvent<Type>.GetPooled(_value, value))
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
        public bool TrySetValue(string typeName)
        {
            if (typeName == null || typeName == String.Empty)
            {
                value = null;
                searchFieldElement.RemoveFromClassList("invalid");
                searchFieldElement.RemoveFromClassList("valid");
                return false;
            }
            if (types.ContainsKey(typeName))
            {
                searchFieldElement.RemoveFromClassList("invalid");
                searchFieldElement.AddToClassList("valid");

                value = types[typeName];
                return true;
            }
            else
            {
                value = null;
                searchFieldElement.RemoveFromClassList("valid");
                searchFieldElement.AddToClassList("invalid");
                return false;
            }


        }
        private class Suggestion : Label
        {

            public Suggestion(Type type, TypeSelector typeSelector) : base(type.FullName)
            {
                this.RegisterCallback<MouseDownEvent>(x =>
                {
                    typeSelector.searchFieldElement.RemoveFromClassList("invalid");
                    typeSelector.searchFieldElement.AddToClassList("valid");
                    typeSelector.value = type;
                });
            }



        }
        public new class UxmlFactory : UxmlFactory<TypeSelector, UxmlTraits> { }
        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            UxmlIntAttributeDescription maxResults = new UxmlIntAttributeDescription { name = "max-results", defaultValue = 10 };

            UxmlStringAttributeDescription value = new UxmlStringAttributeDescription { name = "value" };

            UxmlStringAttributeDescription label = new UxmlStringAttributeDescription { name = "label" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                TypeSelector typeSelector = ve as TypeSelector;
                typeSelector.label = label.GetValueFromBag(bag, cc);
                typeSelector.maxResults = maxResults.GetValueFromBag(bag, cc);
                var type = value.GetValueFromBag(bag, cc);
                if (type != null || type != String.Empty)
                {
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        try
                        {
                            typeSelector.value = assembly.GetType(type, true, false);
                            break;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

            }


        }

    }

    /* public class TypeSelector : TextField, INotifyValueChanged<Type>
    {
        private readonly Dictionary<string, Type> types = new Dictionary<string, Type>();
        private VisualElement suggestionsElement;

        private StyleSheet styleSheet;
        private Func<Type, bool> typeValidator;
        private int maxResults = 32;

        public TypeSelector(Func<Type, bool> typeValidator = null)
        {
        }


        public TypeSelector(string label, Func<Type, bool> typeValidator = null) : base(label)
        {
        }
        private void Initialize()
        {

            suggestionsElement = new VisualElement();

            suggestionsElement.AddToClassList("reactics-type-suggestions");
            this.AddToClassList("reactics-type-search");
            this.AddToClassList("reactics-type-selector");
            this.AddToClassList("unity-base-field");
            this.AddToClassList("unity-base-text-field");
            this.AddToClassList("unity-text-field");
            styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Resources/Editor/TypeSelector.uss");

            types.Clear();
            if (typeValidator != null)
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (typeValidator.Invoke(type))
                            types[type.FullName] = type;
                    }

                }
            else
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        types[type.FullName] = type;
                    }
                }

            searchFieldElement.RegisterValueChangedCallback(change =>
            {
                if (TrySetValue(change.newValue))
                {
                    suggestionsElement.Clear();
                    suggestionsElement.visible = false;
                }
                else
                {
                    suggestionsElement.Clear();
                    if (change.newValue != null && change.newValue != String.Empty)
                    {

                        foreach (var type in types)
                        {
                            if (type.Key.StartsWith(change.newValue))
                            {
                                suggestionsElement.Add(new Suggestion(type.Value, this));


                            }
                            if (suggestionsElement.childCount > maxResults)
                                break;
                        }
                    }

                    suggestionsElement.visible = suggestionsElement.childCount != 0;
                }


            });
            this.textInputBase.RegisterCallback<FocusInEvent>(focus =>
            {
                VisualElement root = this;
                while (root.parent != null)
                    root = root.parent;
                suggestionsElement.RemoveFromHierarchy();
                foreach (var child in root.Children())
                {
                    if (child.name.StartsWith("rootVisualContainer"))
                    {
                        child.Add(suggestionsElement);
                        break;
                    }
                }
                UpdateSuggestionsBoxLayout(suggestionsElement);
            });
            this.RegisterCallback<GeometryChangedEvent>(geometry =>
            {
                UpdateSuggestionsBoxLayout(suggestionsElement);
            });
            this.RegisterCallback<FocusOutEvent>(focus =>
            {
                suggestionsElement.visible = false;
            });
            suggestionsElement.style.position = Position.Absolute;


            suggestionsElement.styleSheets.Add(styleSheet);
            this.styleSheets.Add(styleSheet);

        }

        private void UpdateSuggestionsBoxLayout(VisualElement suggestions)
        {

            suggestions.transform.position = new Vector3(this.worldBound.x, this.worldBound.y - 5, 0);
            suggestions.style.width = this.worldBound.width;
            suggestions.visible = suggestions.childCount != 0;
            suggestions.BringToFront();
        }



        Type INotifyValueChanged<Type>.value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        public void SetValueWithoutNotify(Type newValue)
        {
            throw new NotImplementedException();
        }
        private class Suggestion : Label
        {

            public Suggestion(Type type, TypeSelector typeSelector) : base(type.FullName)
            {
                this.RegisterCallback<MouseDownEvent>(x =>
                {
                    typeSelector.v
                    typeSelector.value = type;
                });
            }



        }
    } */
}