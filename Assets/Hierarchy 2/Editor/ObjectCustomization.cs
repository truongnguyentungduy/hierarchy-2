using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.UIElements;

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
                    evt.StopPropagation();
                    return;
                }

                if (Application.isPlaying)
                {
                    Debug.LogWarning("Cannot custom object in play mode.");
                    evt.StopPropagation();
                    return;
                }
                
                if (Selection.gameObjects.Length == 1 && Selection.activeGameObject != null)
                {
                    ObjectCustomizationPopup.ShowPopup(Selection.activeGameObject);
                }
                else
                {
                    if (Selection.gameObjects.Length > 1)
                    {
                        Debug.LogWarning("Only one object is allowed.");
                    }
                    else
                        Debug.LogWarning("No object has been selected.");
                }

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
        static EditorWindow window;
        GameObject gameObject;
        HierarchyLocalData hierarchyLocalData;

        public static ObjectCustomizationPopup ShowPopup(GameObject gameObject)
        {
            if (Selection.gameObjects.Length > 1 || Selection.activeGameObject == null)
                return null;

            if (window == null)
                window = ObjectCustomizationPopup.GetWindow<ObjectCustomizationPopup>(true, gameObject.name);
            else
            {
                window.Close();
                window = ObjectCustomizationPopup.GetWindow<ObjectCustomizationPopup>(true, gameObject.name);
            }

            Vector2 v2 = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            Vector2 size = new Vector2(256, 100);
            window.position = new Rect(v2.x, v2.y, size.x, size.y);
            window.maxSize = window.minSize = size;
            window.ShowPopup();
            window.Focus();

            ObjectCustomizationPopup objectCustomizationPopup = window as ObjectCustomizationPopup;
            return objectCustomizationPopup;
        }

        void OnEnable()
        {
            rootVisualElement.StyleBorderWidth(1);
            Color c = new Color32(58, 121, 187, 255);
            rootVisualElement.StyleBorderColor(c);

            // EditorHelpBox helpBox = new EditorHelpBox("This feature currently in preview state", MessageType.Info);
            // rootVisualElement.Add(helpBox);

            hierarchyLocalData = HierarchyEditor.Instance.GetHierarchyLocalData(Selection.activeGameObject.scene);
            gameObject = Selection.activeGameObject;
            Selection.activeGameObject = null;

            CustomRowItem customRowItem = null;
            if (hierarchyLocalData.TryGetCustomRowData(gameObject, out customRowItem) == false)
            {
                customRowItem = hierarchyLocalData.CreateCustomRowItemFor(gameObject);
            }

            Hierarchy2.Toggle useBackground = new Hierarchy2.Toggle("Use Background",
                customRowItem.useBackground,
                Justify.FlexStart,
                (evt) =>
                {
                    customRowItem.useBackground = evt.newValue;
                    EditorApplication.RepaintHierarchyWindow();
                });
            rootVisualElement.Add(useBackground);

            EnumField backgroundStyle = new EnumField(customRowItem.backgroundStyle);
            backgroundStyle.label = "Background Style";
            backgroundStyle.RegisterValueChangedCallback((evt) =>
            {
                customRowItem.backgroundStyle = (CustomRowItem.BackgroundStyle) evt.newValue;
                EditorApplication.RepaintHierarchyWindow();
            });
            rootVisualElement.Add(backgroundStyle);

            EnumField backgroundMode = new EnumField(customRowItem.backgroundMode);
            backgroundMode.label = "Background Mode";
            backgroundMode.RegisterValueChangedCallback((evt) =>
            {
                customRowItem.backgroundMode = (CustomRowItem.BackgroundMode) evt.newValue;
                EditorApplication.RepaintHierarchyWindow();
            });
            rootVisualElement.Add(backgroundMode);

            ColorField backgroundColor = new ColorField("Background Color");
            backgroundColor.value = customRowItem.backgroundColor;
            backgroundColor.RegisterValueChangedCallback((evt) =>
            {
                customRowItem.backgroundColor = evt.newValue;
                EditorApplication.RepaintHierarchyWindow();
            });
            rootVisualElement.Add(backgroundColor);
        }
    }
}