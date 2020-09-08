using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;

#endif

namespace Hierarchy2
{
#if UNITY_EDITOR
    [CustomEditor(typeof(HierarchyFolder))]
    internal class HierarchyFolderInspector : Editor
    {
        private static Texture FolderIcon;
        private static Texture EmptyFolderIcon;

        private void OnEnable()
        {
            FolderIcon = EditorGUIUtility.IconContent("Folder Icon").image;
            EmptyFolderIcon = EditorGUIUtility.IconContent("FolderEmpty Icon").image;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var script = target as HierarchyFolder;

            var root = new VisualElement();

            IMGUIContainer imguiContainer = new IMGUIContainer(() =>
            {
                script.doFlatten = (HierarchyFolder.DoFlatten) EditorGUILayout.EnumPopup("Do Flatten", script.doFlatten);
                if (script.doFlatten != HierarchyFolder.DoFlatten.None)
                {
                    script.flattenMode = (HierarchyFolder.FlattenMode) EditorGUILayout.EnumPopup("Flatten Mode", script.flattenMode);
                    script.destroyAfterFlatten = EditorGUILayout.Toggle("Destroy After Flatten", script.destroyAfterFlatten);
                }
            });
            root.Add(imguiContainer);

            UnityEngine.UIElements.Foldout iconSettingsFoldout = new UnityEngine.UIElements.Foldout();
            iconSettingsFoldout.text = "Icon Settings";
            iconSettingsFoldout.value = false;
            root.Add(iconSettingsFoldout);

            ObjectField customIconField = new ObjectField("Custom Folder Icon");
            customIconField.objectType = typeof(Texture);
            customIconField.RegisterValueChangedCallback(e => { script.customIcon = e.newValue as Texture; });
            iconSettingsFoldout.Add(customIconField);

            ColorField folderColorField = new ColorField("Custom Folder Color");
            folderColorField.value = script.iconColor;
            iconSettingsFoldout.Add(folderColorField);

            return root;
        }

        [MenuItem("GameObject/Hierarchy Folder", priority = 0)]
        static void CreateInstance() => new GameObject("Folder", new Type[1] {typeof(HierarchyFolder)});
    }
#endif

    [AddComponentMenu("Hierarchy 2/Hierarchy Folder (Experimental)", 0)]
    public class HierarchyFolder : MonoBehaviour
    {
        public enum DoFlatten
        {
            None = 0,
            Editor = 1,
            Runtime = 2,
            All = 3
        }

        public enum FlattenMode
        {
            Parent = 0,
            World = 1
        }

        public DoFlatten doFlatten = DoFlatten.None;

        public FlattenMode flattenMode = FlattenMode.Parent;

        public bool destroyAfterFlatten = true;

        public Texture customIcon;

        public Color iconColor = Color.grey;

        private void OnEnable()
        {
            Flatten();
        }

        public void Flatten()
        {
            if (doFlatten == DoFlatten.None)
                return;

            if (doFlatten != DoFlatten.All)
            {
                if (doFlatten == DoFlatten.Editor && !Application.isEditor)
                    return;

                if (doFlatten == DoFlatten.Runtime && Application.isEditor)
                    return;
            }

            var parent = flattenMode == FlattenMode.World ? null : transform.parent;
            var childCount = transform.childCount;
            while (childCount-- > 0)
                transform.GetChild(0).SetParent(parent);

            if (destroyAfterFlatten)
                Destroy(gameObject);
        }
    }
}