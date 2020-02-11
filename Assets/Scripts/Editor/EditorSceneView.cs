using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Reactics.Util;

namespace Reactics.Editors
{

    [ExecuteInEditMode]
    public partial class EditorScene : ScriptableObject
    {
        private string previousScene;
        public EditorSceneSubWindow Window { get; internal set; }
        public EditorSceneWindow Scene { get; internal set; }
        internal SerializedObject serializedObject;
        internal UnityEngine.Object Target { get => serializedObject.targetObject; }

        private bool initiated = false;
        public static T GetSceneView<T>(ScriptableObject target, bool force = false) where T : EditorScene
        {
Debug.Log(target);
            T[] editorScenes = Resources.FindObjectsOfTypeAll<T>();
            T editorScene;
            if (editorScenes.Length > 0)
            {
                editorScene = editorScenes[0];
                Debug.Log(editorScene.Scene);
                editorScene.Init();
                if (editorScene.Scene.Scene.IsValid())
                    editorScene.UpdateInfo(editorScene.Scene.Scene, editorScene.previousScene, target);
                else
                {
                    string previousScene = editorScene.previousScene == string.Empty ? SceneManager.GetActiveScene().path : editorScene.previousScene;
                    var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    editorScene.UpdateInfo(scene, previousScene, target);
                }
            }
            else
            {
                string previousScene = SceneManager.GetActiveScene().path;
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                editorScene = CreateInstance<T>();
                editorScene.UpdateInfo(scene, previousScene, target);
            }
            return editorScene;
        }
        private void UpdateInfo(Scene scene, string previousScene, ScriptableObject target)
        {
            Cleanup();
            this.previousScene = previousScene;
            serializedObject = new SerializedObject(target);
            if (!initiated)
                Init();
            Scene.UpdateInfo(this, scene);
            Window.UpdateInfo(this);
        }
        private void Awake()
        {
            Init();
        }
        private void OnEnable()
        {
            Init();
        }
        private void Init()
        {
            Cleanup();
            EditorSceneWindow[] s = Resources.FindObjectsOfTypeAll<EditorSceneWindow>();
            if (s.Length > 0)
                Scene = s[0];
            else
                Scene = CreateInstance<EditorSceneWindow>();


            //Dock next to inspector window
            Window = EditorWindow.GetWindow<EditorSceneSubWindow>("Scene Target Editor", Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll"));
            Scene.InjectDelegates(typeof(EditorSceneWindow), this, GetType());
            Window.InjectDelegates(typeof(EditorSceneSubWindow), this, GetType());
            Window.Init();
            initiated = true;
        }
        private void Cleanup()
        {
            if (serializedObject != null)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(serializedObject.targetObject);
            }
            if (Scene != null)
                Scene.Cleanup();
            if (Window != null)
                Window.Cleanup();
        }
        private void OnDisable()
        {
            Cleanup();
        }

        private void OnDestroy()
        {
            Cleanup();
        }


        [ExecuteInEditMode]
        public sealed class EditorSceneSubWindow : EditorWindow
        {
            internal VisualElement rootElement;

            [DelegateField("OnCreateWindowInspector")]
            private Func<VisualElement, VisualElement> onCreateInspector;

            public void Init()
            {
                rootElement = new VisualElement();
            }
            public void UpdateInfo(EditorScene root)
            {
                rootVisualElement.Clear();
                if (onCreateInspector != null)
                    rootElement = onCreateInspector(rootElement);
                rootVisualElement.Add(rootElement);
                rootElement.Bind(root.serializedObject);

                Show();


            }
            private void OnDisable()
            {
                Debug.Log("Disabled");
            }
            private void OnDestroy()
            {
                Debug.Log("Destroyed");
            }
            public void Cleanup()
            {
                EditorWindow.GetWindow<EditorSceneSubWindow>().rootVisualElement.Clear();
            }
        }
        [ExecuteInEditMode]
        public sealed class EditorSceneWindow : ScriptableObject
        {
            private string previousScene;
            private EditorScene root;
            internal VisualElement rootElement;

            public Scene Scene { get; internal set; }
            [DelegateField]
            private Action onSceneGUI;
            [DelegateField]
            private Action<Scene> onSceneInit;

            [DelegateField]
            private Action onTargetUpdate;

            [DelegateField("OnCreateSceneInspector")]
            private Func<VisualElement, VisualElement> onCreateInspector;

            public SceneView SceneView { get; internal set; }
            private Button backButton;
            private void Awake()
            {
                rootElement = new VisualElement();
                backButton = new Button().Apply(button =>
                {
                    button.text = "Back";
                    button.style.width = new StyleLength(new Length(10f, LengthUnit.Percent));
                    button.clicked += () =>
                    {
                        root?.Cleanup();
                        EditorSceneManager.OpenScene(previousScene);
                    };
                });
            }
            public void UpdateInfo(EditorScene root, Scene scene)
            {
                this.root = root;
                scene.name = root.Target.name;
                Scene = scene;
                previousScene = root.previousScene;
                GameObject[] objs = Scene.GetRootGameObjects();
                foreach (var item in objs)
                {
                    DestroyImmediate(item);
                }
                InitializeSceneView(SceneView.lastActiveSceneView);
                SceneView = SceneView.lastActiveSceneView;

                if (onSceneInit != null)
                {
                    onSceneInit.Invoke(Scene);
                }
                rootElement.Bind(root.serializedObject);

                if (onSceneGUI != null)
                    SceneView.duringSceneGui += RenderSceneGUI;
            }
            private void InitializeSceneView(SceneView sceneView)
            {

                sceneView.rootVisualElement.Clear();
                rootElement.Clear();
                if (!rootElement.Contains(backButton))
                    rootElement.Add(backButton);
                if (onCreateInspector != null)
                    rootElement = onCreateInspector.Invoke(rootElement);

                sceneView.rootVisualElement.Add(rootElement);
            }
            private void RenderSceneGUI(SceneView sceneView)
            {

                if (SceneView != sceneView)
                {
                    SceneView = sceneView;
                    InitializeSceneView(sceneView);
                }
                rootElement.style.top = Math.Max(sceneView.position.height - sceneView.camera.targetTexture.height, 0f);
                rootElement.style.left = Math.Max(sceneView.position.width - sceneView.camera.targetTexture.width, 0f);

                onSceneGUI?.Invoke();
            }
            public void Cleanup()
            {
                if (SceneView != null)
                {
                    SceneView.duringSceneGui -= RenderSceneGUI;
                    if (SceneView.rootVisualElement.Contains(rootElement))
                        SceneView.rootVisualElement.Remove(rootElement);
                }

                rootElement?.Clear();
            }

        }

    }




}