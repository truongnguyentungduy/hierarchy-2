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
        private bool iconSettingsFoldout = false;
        
        private void OnEnable()
        {

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

                this.iconSettingsFoldout = EditorGUILayout.Foldout(this.iconSettingsFoldout, "Icon Settings");
                if (this.iconSettingsFoldout)
                {
                    script.customIcon = EditorGUILayout.ObjectField("Custom Folder Icon", script.customIcon, typeof(Texture), false) as Texture;
                    script.iconColor = EditorGUILayout.ColorField("Custom Folder Color", script.iconColor);
                }
                
            });
            root.Add(imguiContainer);
            
            return root;
        }

        [MenuItem("GameObject/Hierarchy Folder", priority = 0)]
        static void CreateInstance() => new GameObject("Folder", new Type[1] {typeof(HierarchyFolder)});
    }
}