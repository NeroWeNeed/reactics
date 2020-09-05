using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
namespace Reactics.Editor {
    public class TabView : VisualElement {
        public const string TAB_BUTTON_CLASS = "tab-button";
        public const string TAB_BUTTON_SELECTED_CLASS = "tab-button--selected";
        public const string TAB_PANE_CLASS = "tab-pane";
        public const string TAB_PANE_SELECTED_CLASS = "tab-pane--selected";
        public const string USS_GUID = "638e7020a3e9f4d47b3ae8380a5d5e58";
        public VisualElement navigation;
        public VisualElement container;
        public TabView() {
            navigation = new VisualElement
            {
                name = "tab-navigation"
            };
            container = new VisualElement
            {
                name = "tab-container"
            };

            this.Add(navigation);
            this.Add(container);
            this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(USS_GUID)));
        }
        public void AddTab(int index, string name, VisualElement element) {
            element.tabIndex = index;
            TabButton button = navigation.Query<TabButton>().Where((b) => b.tabIndex == index).First();
            if (button == null) {
                button = new TabButton(index)
                {
                    text = name
                };
                button.AddToClassList(TAB_BUTTON_CLASS);
                button.clicked += () =>
                {
                    container.Query<VisualElement>(null, TAB_PANE_CLASS).ForEach((pane) =>
                    {
                        if (pane.tabIndex == button.tabIndex) {
                            pane.AddToClassList(TAB_PANE_SELECTED_CLASS);
                        }
                        else {
                            pane.RemoveFromClassList(TAB_PANE_SELECTED_CLASS);
                        }
                    });
                    navigation.Query<TabButton>(null, TAB_BUTTON_SELECTED_CLASS).ForEach((e) => e.RemoveFromClassList(TAB_BUTTON_SELECTED_CLASS));
                    button.AddToClassList(TAB_BUTTON_SELECTED_CLASS);
                };
                navigation.Add(button);
            }
            element.AddToClassList(TAB_PANE_CLASS);
            if (container.Children().Count((e) => e.ClassListContains(TAB_PANE_CLASS)) == 0) {

                container.AddToClassList(TAB_PANE_SELECTED_CLASS);
                button.AddToClassList(TAB_BUTTON_SELECTED_CLASS);
            }
            container.Add(element);
        }
        public void RemoveTab(int index) {
            bool selectFirst = false;
            foreach (var button in navigation.Query<TabButton>(null, TAB_BUTTON_CLASS).ToList()) {
                if (button.tabIndex == index) {
                    if (button.ClassListContains(TAB_BUTTON_SELECTED_CLASS))
                        selectFirst = true;
                    button.RemoveFromHierarchy();
                }
            }
            foreach (var pane in container.Query<VisualElement>(null, TAB_PANE_CLASS).ToList()) {
                if (pane.tabIndex == index)
                    pane.RemoveFromHierarchy();
            }
            if (selectFirst && navigation.childCount > 0) {
                var newSelected = navigation.Children().First();
                var newIndex = newSelected.tabIndex;
                newSelected.AddToClassList(TAB_BUTTON_SELECTED_CLASS);
                foreach (var pane in container.Query<VisualElement>(null, TAB_PANE_CLASS).ToList()) {
                    if (pane.tabIndex == newIndex)
                        pane.AddToClassList(TAB_PANE_SELECTED_CLASS);
                }
            }
        }
    }
    public class TabButton : Button {

        public TabButton() : this(-1) { }

        public TabButton(int tabIndex) {
            this.tabIndex = tabIndex;
        }
    }

}