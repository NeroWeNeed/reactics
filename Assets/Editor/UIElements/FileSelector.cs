using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Core.Editor {
    public class FileSelector : VisualElement, INotifyValueChanged<string> {
        public readonly string defaultDirectory;
        public FileSelectorMode Mode { get; set; }
        private TextField textFieldElement;

        public string currentDirectory;

        public string extension;

        public string defaultName;
        private Button buttonElement;
        private string _value;
        public string value
        {
            get => _value;
            set
            {
                if (value != _value) {
                    if (panel != null) {

                        using (ChangeEvent<string> evt = ChangeEvent<string>.GetPooled(_value, value)) {
                            evt.target = this;
                            SetValueWithoutNotify(value);
                            textFieldElement.SetValueWithoutNotify(value);
                            SendEvent(evt);
                        }
                    }
                    else {
                        SetValueWithoutNotify(value);

                        textFieldElement.SetValueWithoutNotify(value);
                    }
                }
            }
        }
        public FileSelector(FileSelectorMode mode, string extension, string defaultDirectory = "Assets/Resources") {
            this.Mode = mode;
            this.defaultDirectory = defaultDirectory;
            this.extension = extension;
            Initialize();
        }

        private void Initialize() {
            buttonElement = new Button
            {
                text = "..."
            };
            buttonElement.clicked += OnFileSelect;
            textFieldElement = new TextField();

            this.Add(textFieldElement);
            Add(buttonElement);
            textFieldElement.style.flexGrow = 1;
            this.style.flexDirection = FlexDirection.Row;
        }
        private void OnFileSelect() {
            switch (Mode) {
                case FileSelectorMode.SAVE_FILE:

                    var result = EditorUtility.SaveFilePanel("Save As", currentDirectory, defaultName, extension);
                    if (result.Length != 0) {
                        currentDirectory = result.Substring(0, result.LastIndexOf('/'));
                        Debug.Log(result);
                        value = result;
                    }
                    break;
                case FileSelectorMode.LOAD_FILE:
                    break;
                case FileSelectorMode.SAVE_DIRECTORY:
                    break;
                case FileSelectorMode.LOAD_DIRECTORY:
                    break;
            }
        }
        public enum FileSelectorMode {
            SAVE_FILE, LOAD_FILE, SAVE_DIRECTORY, LOAD_DIRECTORY
        }

        public void SetValueWithoutNotify(string newValue) {
            _value = newValue;
        }
    }
}