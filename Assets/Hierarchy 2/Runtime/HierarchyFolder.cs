using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hierarchy2
{
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

        public Color iconColor = Color.clear;

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