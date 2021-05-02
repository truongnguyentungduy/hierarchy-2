using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace Hierarchy2
{
    public class ObjectCustomizationShelf : IHierarchyShelf
    {
        HierarchyCanvas canvas;

        public void Canvas(HierarchyCanvas canvas) => this.canvas = canvas;

        public VisualElement CreateShelfElement()
        {
            VisualElement shelfButton = new VisualElement();
            shelfButton.name = nameof(OpenSettings);
            shelfButton.tooltip = "";
            shelfButton.StyleHeight(23);
            shelfButton.StyleJustifyContent(Justify.Center);
            Color c = EditorGUIUtility.isProSkin ? new Color32(32, 32, 32, 255) : new Color32(128, 128, 128, 255);
            shelfButton.StyleBorderColor(c);
            shelfButton.StyleBorderWidth(0, 0, 1, 0);

            shelfButton.Add(new Label("Custom Selection"));

            shelfButton.RegisterCallback<MouseDownEvent>((evt) =>
            {
                var isPrefabMode = PrefabStageUtility.GetCurrentPrefabStage() != null ? true : false;
                if (isPrefabMode)
                {
                    Debug.LogWarning("Cannot custom object in prefab.");
                    shelfButton.parent.StyleDisplay(false);
                    evt.StopPropagation();
                    return;
                }

                if (Application.isPlaying)
                {
                    Debug.LogWarning("Cannot custom object in play mode.");
                    shelfButton.parent.StyleDisplay(false);
                    evt.StopPropagation();
                    return;
                }
                
                ObjectCustomizationPopup.ShowPopup(Selection.GetFiltered<GameObject>(SelectionMode.ExcludePrefab));
                Selection.activeGameObject = null;

                shelfButton.parent.StyleDisplay(false);
                evt.StopPropagation();
            });

            shelfButton.RegisterCallback<MouseEnterEvent>((evt) =>
            {
                shelfButton.StyleBackgroundColor(new Color(.5f, .5f, .5f, .5f));
            });

            shelfButton.RegisterCallback<MouseLeaveEvent>((evt) => { shelfButton.StyleBackgroundColor(Color.clear); });

            return shelfButton;
        }

        public int ShelfPriority()
        {
            return 98;
        }
    }

    public class ObjectCustomizationPopup : EditorWindow
    {
        public class CustomRowItemHolder : ScriptableObject
        {
            public CustomRowItem customRowItem;
        }

        static ObjectCustomizationPopup window;
        static GameObject[] gameObjects;
        HierarchyLocalData hierarchyLocalData;
        CustomRowItemHolder[] customRowItemHolders;

        public static ObjectCustomizationPopup ShowPopup(GameObject[] gameObjects)
        {
            if (window != null)
                window.Close();

            if (gameObjects == null || gameObjects.Length == 0)
            { 
                Debug.LogWarning("No object has been selected.");
                return null;
            }

            ObjectCustomizationPopup.gameObjects = gameObjects;

            Vector2 v2 = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            Vector2 size = new Vector2(256, 138);
            window = GetWindow<ObjectCustomizationPopup>(gameObjects[0].name);
            window.position = new Rect(v2.x - size.x - 30f, v2.y - size.y * 0.5f, size.x, size.y);
            window.maxSize = window.minSize = size;
            window.ShowPopup();
            window.Focus();

            return window;
        }

        void OnEnable()
        {
            if (gameObjects == null)
                return;

            List<GameObject> _gameObjects = new List<GameObject>(gameObjects);
            _gameObjects.RemoveAll((gameObject) => gameObject == null);
            if (_gameObjects.Count == 0) // All GameObjects are destroyed
            { 
                Close();
                return;
            }

            rootVisualElement.StyleMargin(4, 4, 2, 0);

            customRowItemHolders = new CustomRowItemHolder[_gameObjects.Count];

            for (int i = 0; i < _gameObjects.Count; i++)
            {
                if (_gameObjects[i] == null)
                    continue;

                hierarchyLocalData = HierarchyEditor.Instance.GetHierarchyLocalData(_gameObjects[i].scene);

                CustomRowItem customRowItem;
                if (hierarchyLocalData.TryGetCustomRowData(_gameObjects[i], out customRowItem) == false)
                    customRowItem = hierarchyLocalData.CreateCustomRowItemFor(_gameObjects[i]);

                customRowItemHolders[i] = CreateInstance<CustomRowItemHolder>();
                customRowItemHolders[i].customRowItem = customRowItem;
            }
            
            SerializedProperty customRowItemsSerialized = new SerializedObject(customRowItemHolders).FindProperty("customRowItem");
            SerializedProperty useBackground = customRowItemsSerialized.FindPropertyRelative("useBackground");
            SerializedProperty backgroundStyle = customRowItemsSerialized.FindPropertyRelative("backgroundStyle");
            SerializedProperty backgroundMode = customRowItemsSerialized.FindPropertyRelative("backgroundMode");
            SerializedProperty backgroundColor = customRowItemsSerialized.FindPropertyRelative("backgroundColor");
            SerializedProperty overrideLabel = customRowItemsSerialized.FindPropertyRelative("overrideLabel");
            SerializedProperty labelOffset = customRowItemsSerialized.FindPropertyRelative("labelOffset");
            SerializedProperty labelColor = customRowItemsSerialized.FindPropertyRelative("labelColor");

            IMGUIContainer iMGUIContainer = new IMGUIContainer(() =>
            {
                if (hierarchyLocalData == null || EditorApplication.isCompiling)
                {
                    Close();
                    return;
                }

                var wideMode = EditorGUIUtility.wideMode;
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.wideMode = true;
                EditorGUIUtility.labelWidth = 140f;

                EditorGUI.BeginChangeCheck();

                customRowItemsSerialized.serializedObject.Update();

                EditorGUILayout.PropertyField(useBackground, new GUIContent("Background"));

                GUI.enabled = useBackground.boolValue || useBackground.hasMultipleDifferentValues;
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(backgroundStyle);
                EditorGUILayout.PropertyField(backgroundMode);
                EditorGUILayout.PropertyField(backgroundColor);

                EditorGUI.indentLevel--;
                GUI.enabled = true;

                EditorGUILayout.PropertyField(overrideLabel);

                GUI.enabled = overrideLabel.boolValue || overrideLabel.hasMultipleDifferentValues;
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(labelOffset);
                EditorGUILayout.PropertyField(labelColor);

                EditorGUI.indentLevel--;
                GUI.enabled = true;

                customRowItemsSerialized.serializedObject.ApplyModifiedProperties();
                
                if (EditorGUI.EndChangeCheck())
                    EditorApplication.RepaintHierarchyWindow();

                EditorGUIUtility.wideMode = wideMode;
                EditorGUIUtility.labelWidth = labelWidth;
            } );
            rootVisualElement.Add(iMGUIContainer);

            Undo.undoRedoPerformed -= Repaint;
            Undo.undoRedoPerformed += Repaint;

        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= Repaint;

            if (customRowItemHolders != null)
            {
                foreach (CustomRowItemHolder customRowItemHolder in customRowItemHolders)
                    DestroyImmediate(customRowItemHolder);
            }
        }
    }
}