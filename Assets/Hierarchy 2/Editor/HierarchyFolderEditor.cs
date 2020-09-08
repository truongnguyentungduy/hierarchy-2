using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace Hierarchy2
{
    [CustomEditor(typeof(HierarchyFolder))]
    internal class HierarchyFolderEditor : Editor
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
}

