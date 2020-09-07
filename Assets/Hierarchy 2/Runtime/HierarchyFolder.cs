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
        public override VisualElement CreateInspectorGUI()
        {
            var script = target as HierarchyFolder;
            
            var root = new VisualElement();

            ColorField color = new ColorField("Folder Color: ");
            color.value = script.color;
            root.Add(color);
            
            return root;
        }
    }
#endif

    [AddComponentMenu("Hierarchy 2/Hierarchy Folder", 0)]
    public class HierarchyFolder : MonoBehaviour
    {
        public Color color = Color.grey;
    }
}