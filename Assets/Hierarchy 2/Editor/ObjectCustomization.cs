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
        static ObjectCustomizationPopup window;
        static GameObject[] gameObjects;
        HierarchyLocalData[] hierarchyLocalDatas;
        CustomRowItem[] customRowItems;

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

            customRowItems = new CustomRowItem[_gameObjects.Count];

            List<HierarchyLocalData> _hierarchyLocalDatas = new List<HierarchyLocalData>(2);
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                if (_gameObjects[i] == null)
                    continue;

                HierarchyLocalData hierarchyLocalData = HierarchyEditor.Instance.GetHierarchyLocalData(_gameObjects[i].scene);

                CustomRowItem customRowItem;
                if (hierarchyLocalData.TryGetCustomRowData(_gameObjects[i], out customRowItem) == false)
                    customRowItem = hierarchyLocalData.CreateCustomRowItemFor(_gameObjects[i]);

                customRowItems[i] = customRowItem;

                if (!_hierarchyLocalDatas.Contains(hierarchyLocalData))
                    _hierarchyLocalDatas.Add(hierarchyLocalData);
            }

            hierarchyLocalDatas = _hierarchyLocalDatas.ToArray();

            IMGUIContainer iMGUIContainer = new IMGUIContainer(() =>
            {
                if (hierarchyLocalDatas == null || EditorApplication.isCompiling)
                {
                    Close();
                    return;
                }

                for (int i = 0; i < hierarchyLocalDatas.Length; i++)
                {
                    if (hierarchyLocalDatas[i] == null) // A scene is closed or user has deleted HierarchyLocalData manually
                    {
                        Close();
                        return;
                    }
                }

                var wideMode = EditorGUIUtility.wideMode;
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.wideMode = true;
                EditorGUIUtility.labelWidth = 140f;

                EditorGUI.showMixedValue = CheckMultipleDifferentValues((customRowItem1, customRowItem2) => customRowItem1.useBackground == customRowItem2.useBackground);
                bool useBackground = EditorGUILayout.Toggle("Background", customRowItems[0].useBackground);
                ApplyModifiedProperties((customRowItem) => customRowItem.useBackground = useBackground);

                GUI.enabled = useBackground || EditorGUI.showMixedValue;
                EditorGUI.indentLevel++;

                EditorGUI.showMixedValue = CheckMultipleDifferentValues((customRowItem1, customRowItem2) => customRowItem1.backgroundStyle == customRowItem2.backgroundStyle);
                CustomRowItem.BackgroundStyle backgroundStyle = (CustomRowItem.BackgroundStyle) EditorGUILayout.EnumPopup("Background Style", customRowItems[0].backgroundStyle);
                ApplyModifiedProperties((customRowItem) => customRowItem.backgroundStyle = backgroundStyle);

                EditorGUI.showMixedValue = CheckMultipleDifferentValues((customRowItem1, customRowItem2) => customRowItem1.backgroundMode == customRowItem2.backgroundMode);
                CustomRowItem.BackgroundMode backgroundMode = (CustomRowItem.BackgroundMode) EditorGUILayout.EnumPopup("Background Mode", customRowItems[0].backgroundMode);
                ApplyModifiedProperties((customRowItem) => customRowItem.backgroundMode = backgroundMode);

                EditorGUI.showMixedValue = CheckMultipleDifferentValues((customRowItem1, customRowItem2) => customRowItem1.backgroundColor == customRowItem2.backgroundColor);
                Color backgroundColor = EditorGUILayout.ColorField("Background Color", customRowItems[0].backgroundColor);
                ApplyModifiedProperties((customRowItem) => customRowItem.backgroundColor = backgroundColor);

                EditorGUI.indentLevel--;
                GUI.enabled = true;

                EditorGUI.showMixedValue = CheckMultipleDifferentValues((customRowItem1, customRowItem2) => customRowItem1.overrideLabel == customRowItem2.overrideLabel);
                bool overrideLabel = EditorGUILayout.Toggle("Override Label", customRowItems[0].overrideLabel);
                ApplyModifiedProperties((customRowItem) => customRowItem.overrideLabel = overrideLabel);

                GUI.enabled = overrideLabel || EditorGUI.showMixedValue;
                EditorGUI.indentLevel++;

                EditorGUI.showMixedValue = CheckMultipleDifferentValues((customRowItem1, customRowItem2) => customRowItem1.labelOffset == customRowItem2.labelOffset);
                Vector2 labelOffset = EditorGUILayout.Vector2Field("Label Offset", customRowItems[0].labelOffset);
                ApplyModifiedProperties((customRowItem) => customRowItem.labelOffset = labelOffset);

                EditorGUI.showMixedValue = CheckMultipleDifferentValues((customRowItem1, customRowItem2) => customRowItem1.labelColor == customRowItem2.labelColor);
                Color labelColor = EditorGUILayout.ColorField("Label Color", customRowItems[0].labelColor);
                ApplyModifiedProperties((customRowItem) => customRowItem.labelColor = labelColor);

                EditorGUI.indentLevel--;
                GUI.enabled = true;

                EditorGUI.showMixedValue = false;
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
        }

        bool CheckMultipleDifferentValues(System.Func<CustomRowItem, CustomRowItem, bool> equalityComparer)
        {
            EditorGUI.BeginChangeCheck();

            for (int i = 1; i < customRowItems.Length; i++)
            {
                if (equalityComparer (customRowItems[0], customRowItems[i]) == false)
                    return true;
            }

            return false;
        }

        void ApplyModifiedProperties(System.Action<CustomRowItem> applyAction)
        {
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < hierarchyLocalDatas.Length; i++)
                    Undo.RecordObject(hierarchyLocalDatas[i], "Change Object Customization");
                
                for (int i = 0; i < customRowItems.Length; i++)
                    applyAction(customRowItems[i]);

                EditorApplication.RepaintHierarchyWindow();
            }
        }
    }
}