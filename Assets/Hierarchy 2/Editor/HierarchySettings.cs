using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace Hierarchy2
{
    internal class HierarchySettings : ScriptableObject
    {
        [Serializable]
        public struct ThemeData
        {
            public Color colorRowEven;
            public Color colorRowOdd;
            public Color colorGrid;
            public Color colorTreeView;
            public Color colorLockIcon;
            public Color tagColor;
            public Color layerColor;
            public Color comSelBGColor;
            public Color selectionColor;
            public Color colorHeaderTitle;
            public Color colorHeaderBackground;

            public ThemeData(ThemeData themeData)
            {
                colorRowEven = themeData.colorRowEven;
                colorRowOdd = themeData.colorRowOdd;
                colorGrid = themeData.colorGrid;
                colorTreeView = themeData.colorTreeView;
                colorLockIcon = themeData.colorLockIcon;
                tagColor = themeData.tagColor;
                layerColor = themeData.layerColor;
                comSelBGColor = themeData.comSelBGColor;
                selectionColor = themeData.selectionColor;
                colorHeaderTitle = themeData.colorHeaderTitle;
                colorHeaderBackground = themeData.colorHeaderBackground;
            }

            public void BlendMultiply(Color blend)
            {
                colorRowEven = colorRowEven * blend;
                colorRowOdd = colorRowOdd * blend;
                colorGrid = colorGrid * blend;
                colorTreeView = colorTreeView * blend;
                colorLockIcon = colorLockIcon * blend;
                tagColor = tagColor * blend;
                layerColor = layerColor * blend;
                comSelBGColor = comSelBGColor * blend;
                selectionColor = selectionColor * blend;
                colorHeaderTitle = colorHeaderTitle * blend;
                colorHeaderBackground = colorHeaderBackground * blend;
            }
        }

        public enum ComponentSize
        {
            Small,
            Normal,
            Large
        }

        public enum ElementAlignment
        {
            AfterName,
            Right
        }

        [Flags]
        public enum ContentDisplay
        {
            Component = (1 << 0),
            Tag = (1 << 1),
            Layer = (1 << 2)
        }

        public ThemeData personalTheme;
        public ThemeData professionalTheme;
        public ThemeData playmodeTheme;
        private bool useThemePlaymode = false;

        public ThemeData usedTheme
        {
            get
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    if (useThemePlaymode == false)
                    {
                        playmodeTheme = new ThemeData(EditorGUIUtility.isProSkin ? professionalTheme : personalTheme);
                        playmodeTheme.BlendMultiply(GUI.color);
                        useThemePlaymode = true;
                    }

                    return playmodeTheme;
                }
                else
                {
                    useThemePlaymode = false;
                    return EditorGUIUtility.isProSkin ? professionalTheme : personalTheme;
                }
            }
        }

        [HideInInspector] public bool activeHierarchy = true;
        [HideInInspector] public bool autoCreateHLD = true;
        [HideInInspector] public bool pingHierarchyLocalDataObject = false;
        [HideInInspector] public bool displayVersion = true;
        public bool displayObjectIcon = true;
        public bool displayCustomObjectIcon = true;
        public bool displayTreeView = true;
        public bool displayRowBackground = true;
        public bool displayGrid = false;
        [HideInInspector] public bool displayStaticButton = true;
        public int offSetIconAfterName = 8;
        public bool displayComponents = true;
        public ElementAlignment componentAlignment = ElementAlignment.AfterName;

        public enum ComponentDisplayMode
        {
            All = 0,
            ScriptOnly = 1,
            Below = 2,
            Ignore = 3
        }

        public ComponentDisplayMode componentDisplayMode = ComponentDisplayMode.Ignore;
        public string[] components = new string[] {"Transform", "RectTransform"};
        [HideInInspector] public int componentLimited = 0;
        [Range(12, 16)] public int componentSize = 16;
        public int componentSpacing = 0;
        public bool displayTag = true;
        public ElementAlignment tagAlignment = ElementAlignment.AfterName;
        public bool displayLayer = true;
        public ElementAlignment layerAlignment = ElementAlignment.AfterName;
        [HideInInspector] public bool applyStaticTargetAndChild = true;
        public bool applyTagTargetAndChild = false;
        public bool applyLayerTargetAndChild = true;
        public string headerPrefix = "$h";
        public bool displayDirtyTrack = false;
        public bool onlyDisplayWhileMouseEnter = false;
        public ContentDisplay contentDisplay = ContentDisplay.Component | ContentDisplay.Tag | ContentDisplay.Layer;


        public delegate void OnSettingsChangedCallback(string param);

        public OnSettingsChangedCallback onSettingsChanged;

        public void OnSettingsChanged(string param = "")
        {
            switch (param)
            {
                case nameof(componentSize):
                    if (componentSize % 2 != 0) componentSize -= 1;
                    break;

                case nameof(componentSpacing):
                    if (componentSpacing < 0) componentSpacing = 0;
                    break;
            }

            onSettingsChanged?.Invoke(param);
            hideFlags = HideFlags.None;

            EditorUtility.SetDirty(this);
        }

        [SettingsProvider]
        static SettingsProvider UIElementSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Hierarchy", SettingsScope.Project)
            {
                label = "Hierarchy",

                activateHandler = (searchContext, rootElement) =>
                {
                    Editor editor = Editor.CreateEditor(GetAssets());
                    var settings = editor.target as HierarchySettings;

                    float TITLE_MARGIN_TOP = 10;
                    float TITLE_MARGIN_BOTTOM = 4;
                    float CONTENT_MARGIN_LEFT = 10;

                    Label hierarchyTitle = new Label("Hierarchy");
                    hierarchyTitle.StyleFontSize(20);
                    hierarchyTitle.StyleMargin(10, 0, 2, 2);
                    hierarchyTitle.StyleFont(FontStyle.Bold);
                    rootElement.Add(hierarchyTitle);


                    ScrollView scrollView = new ScrollView();
                    rootElement.Add(scrollView);

                    VerticalLayout verticalLayout = new VerticalLayout();
                    verticalLayout.StylePadding(8, 8, 8, 8);
                    scrollView.Add(verticalLayout);

                    Label Object = new Label("Object");
                    Object.StyleFont(FontStyle.Bold);
                    Object.StyleMargin(0, 0, TITLE_MARGIN_TOP, TITLE_MARGIN_BOTTOM);
                    verticalLayout.Add(Object);

                    Hierarchy2.Toggle displayObjectIcon = new Hierarchy2.Toggle("Display Object Icon",
                        settings.displayObjectIcon,
                        Justify.FlexStart,
                        (evt) =>
                        {
                            settings.displayObjectIcon = evt.newValue;
                            settings.OnSettingsChanged(nameof(settings.displayObjectIcon));
                        });
                    displayObjectIcon.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                    verticalLayout.Add(displayObjectIcon);

                    Hierarchy2.Toggle displayCustomObjectIcon = new Hierarchy2.Toggle("Display Custom Icon",
                        settings.displayCustomObjectIcon,
                        Justify.FlexStart,
                        (evt) =>
                        {
                            settings.displayCustomObjectIcon = evt.newValue;
                            settings.OnSettingsChanged(nameof(settings.displayCustomObjectIcon));
                        });
                    displayCustomObjectIcon.StyleMarginLeft(CONTENT_MARGIN_LEFT * 2);
                    verticalLayout.Add(displayCustomObjectIcon);
                    displayCustomObjectIcon.StyleDisplay(settings.displayObjectIcon);
                    displayObjectIcon.RegisterValueChangedCallback((evt) =>
                    {
                        displayCustomObjectIcon.StyleDisplay(evt.newValue);
                    });

                    Hierarchy2.Toggle displayDirtyTrack = new Hierarchy2.Toggle("Display Dirty Track [EXPERIMENTAL]",
                        settings.displayDirtyTrack,
                        Justify.FlexStart,
                        (evt) =>
                        {
                            settings.displayDirtyTrack = evt.newValue;
                            settings.OnSettingsChanged(nameof(settings.displayDirtyTrack));
                        });
                    displayDirtyTrack.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                    verticalLayout.Add(displayDirtyTrack);

                    Label View = new Label("View");
                    View.StyleFont(FontStyle.Bold);
                    View.StyleMargin(0, 0, TITLE_MARGIN_TOP, TITLE_MARGIN_BOTTOM);
                    verticalLayout.Add(View);

                    Hierarchy2.Toggle displayRowBackground = new Hierarchy2.Toggle("Display RowBackground",
                        settings.displayRowBackground,
                        Justify.FlexStart,
                        (evt) =>
                        {
                            settings.displayRowBackground = evt.newValue;
                            settings.OnSettingsChanged(nameof(settings.displayRowBackground));
                        });
                    displayRowBackground.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                    verticalLayout.Add(displayRowBackground);

                    Hierarchy2.Toggle displayTreeView = new Hierarchy2.Toggle("Display TreeView",
                        settings.displayTreeView,
                        Justify.FlexStart,
                        (evt) =>
                        {
                            settings.displayTreeView = evt.newValue;
                            settings.OnSettingsChanged(nameof(settings.displayTreeView));
                        });
                    displayTreeView.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                    verticalLayout.Add(displayTreeView);

                    Hierarchy2.Toggle displayGrid = new Hierarchy2.Toggle("Display Grid",
                        settings.displayGrid,
                        Justify.FlexStart,
                        (evt) =>
                        {
                            settings.displayGrid = evt.newValue;
                            settings.OnSettingsChanged(nameof(settings.displayGrid));
                        });
                    displayGrid.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                    verticalLayout.Add(displayGrid);

                    Label Components = new Label("Components");
                    Components.StyleFont(FontStyle.Bold);
                    Components.StyleMargin(0, 0, TITLE_MARGIN_TOP, TITLE_MARGIN_BOTTOM);
                    verticalLayout.Add(Components);

                    Hierarchy2.Toggle displayComponents = new Hierarchy2.Toggle("Display Components Icon",
                        settings.displayComponents,
                        Justify.FlexStart,
                        (evt) =>
                        {
                            settings.displayComponents = evt.newValue;
                            settings.OnSettingsChanged(nameof(settings.displayComponents));
                        });
                    displayComponents.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                    verticalLayout.Add(displayComponents);

                    EnumField componentAlignment = new EnumField(settings.componentAlignment);
                    componentAlignment.label = "Component Alignment";
                    componentAlignment.RegisterValueChangedCallback((evt) =>
                    {
                        settings.componentAlignment = (ElementAlignment) evt.newValue;
                        settings.OnSettingsChanged(nameof(settings.componentAlignment));
                    });
                    componentAlignment.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                    verticalLayout.Add(componentAlignment);

                    EnumField componentDisplayMode = new EnumField(settings.componentDisplayMode);
                    componentDisplayMode.label = "Component Display Mode";
                    componentDisplayMode.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                    verticalLayout.Add(componentDisplayMode);

                    TextField componentListInput = new TextField();
                    componentListInput.value = string.Join(" ", settings.components);
                    componentListInput.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                    verticalLayout.Add(componentListInput);
                    componentListInput.RegisterValueChangedCallback((evt) =>
                    {
                        settings.components = evt.newValue.Split(' ');
                        settings.OnSettingsChanged(nameof(settings.components));
                    });
                    componentDisplayMode.RegisterValueChangedCallback((evt) =>
                    {
                        settings.componentDisplayMode = (ComponentDisplayMode) evt.newValue;
                        switch (settings.componentDisplayMode)
                        {
                            case ComponentDisplayMode.Below:
                                componentListInput.StyleDisplay(true);
                                break;

                            case ComponentDisplayMode.Ignore:
                                componentListInput.StyleDisplay(true);
                                break;

                            case ComponentDisplayMode.All:
                                componentListInput.StyleDisplay(false);
                                break;

                            case ComponentDisplayMode.ScriptOnly:
                                componentListInput.StyleDisplay(false);
                                break;
                        }

                        settings.OnSettingsChanged(nameof(settings.componentDisplayMode));
                    });

                    var componentSizeEnum = ComponentSize.Normal;
                    switch (settings.componentSize)
                    {
                        case 12:
                            componentSizeEnum = ComponentSize.Small;
                            break;

                        case 14:
                            componentSizeEnum = ComponentSize.Normal;
                            break;

                        case 16:
                            componentSizeEnum = ComponentSize.Large;
                            break;
                    }

                    EnumField componentSize = new EnumField(componentSizeEnum);
                    componentSize.label = "Component Size";
                    componentSize.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                    componentSize.RegisterValueChangedCallback((evt) =>
                    {
                        switch (evt.newValue)
                        {
                            case ComponentSize.Small:
                                settings.componentSize = 12;
                                break;

                            case ComponentSize.Normal:
                                settings.componentSize = 14;
                                break;

                            case ComponentSize.Large:
                                settings.componentSize = 16;
                                break;
                        }

                        settings.OnSettingsChanged(nameof(settings.componentSize));
                    });
                    verticalLayout.Add(componentSize);

                    IntegerField componentSpacing = new IntegerField();
                    componentSpacing.label = "Component Spacing";
                    componentSpacing.value = settings.componentSpacing;
                    componentSpacing.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                    componentSpacing.RegisterValueChangedCallback((evt) =>
                    {
                        settings.componentSpacing = evt.newValue;
                        settings.OnSettingsChanged(nameof(settings.componentSpacing));
                    });
                    verticalLayout.Add(componentSpacing);

                    Label TagAndLayer = new Label("Tag And Layer");
                    TagAndLayer.StyleFont(FontStyle.Bold);
                    TagAndLayer.StyleMargin(0, 0, TITLE_MARGIN_TOP, TITLE_MARGIN_BOTTOM);
                    verticalLayout.Add(TagAndLayer);

                    Hierarchy2.Toggle displayTag = new Hierarchy2.Toggle("Display Tag",
                        settings.displayTag,
                        Justify.FlexStart,
                        (evt) =>
                        {
                            settings.displayTag = evt.newValue;
                            settings.OnSettingsChanged(nameof(settings.displayTag));
                        });
                    displayTag.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                    verticalLayout.Add(displayTag);

                    Hierarchy2.Toggle applyTagTargetAndChild = new Hierarchy2.Toggle(
                        "Apply Child Tag When Parent Tag Change",
                        settings.applyTagTargetAndChild,
                        Justify.FlexStart,
                        (evt) =>
                        {
                            settings.applyTagTargetAndChild = evt.newValue;
                            settings.OnSettingsChanged(nameof(settings.applyTagTargetAndChild));
                        });
                    applyTagTargetAndChild.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                    verticalLayout.Add(applyTagTargetAndChild);

                    EnumField tagAlignment = new EnumField(settings.tagAlignment);
                    tagAlignment.label = "Tag Alignment";
                    tagAlignment.RegisterValueChangedCallback((evt) =>
                    {
                        settings.tagAlignment = (ElementAlignment) evt.newValue;
                        settings.OnSettingsChanged(nameof(settings.tagAlignment));
                    });
                    tagAlignment.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                    verticalLayout.Add(tagAlignment);

                    Hierarchy2.Toggle displayLayer = new Hierarchy2.Toggle("Display Layer",
                        settings.displayLayer,
                        Justify.FlexStart,
                        (evt) =>
                        {
                            settings.displayLayer = evt.newValue;
                            settings.OnSettingsChanged(nameof(settings.displayLayer));
                        });
                    displayLayer.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                    verticalLayout.Add(displayLayer);

                    Hierarchy2.Toggle applyLayerTargetAndChild = new Hierarchy2.Toggle(
                        "Apply Child Layer When Parent Layer Change",
                        settings.applyLayerTargetAndChild,
                        Justify.FlexStart,
                        (evt) =>
                        {
                            settings.applyLayerTargetAndChild = evt.newValue;
                            settings.OnSettingsChanged(nameof(settings.applyLayerTargetAndChild));
                        });
                    applyLayerTargetAndChild.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                    verticalLayout.Add(applyLayerTargetAndChild);

                    EnumField layerAlignment = new EnumField(settings.layerAlignment);
                    layerAlignment.label = "Layer Alignment";
                    layerAlignment.RegisterValueChangedCallback((evt) =>
                    {
                        settings.layerAlignment = (ElementAlignment) evt.newValue;
                        settings.OnSettingsChanged(nameof(settings.layerAlignment));
                    });
                    layerAlignment.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                    verticalLayout.Add(layerAlignment);

                    Label advanced = new Label("Advanced");
                    advanced.StyleFont(FontStyle.Bold);
                    advanced.StyleMargin(0, 0, TITLE_MARGIN_TOP, TITLE_MARGIN_BOTTOM);
                    verticalLayout.Add(advanced);

                    Hierarchy2.Toggle onlyDisplayWhileMouseHovering = new Hierarchy2.Toggle(
                        "Only Display While Mouse Hovering",
                        settings.onlyDisplayWhileMouseEnter,
                        Justify.FlexStart,
                        (evt) =>
                        {
                            settings.onlyDisplayWhileMouseEnter = evt.newValue;
                            settings.OnSettingsChanged(nameof(settings.onlyDisplayWhileMouseEnter));
                        });
                    onlyDisplayWhileMouseHovering.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                    verticalLayout.Add(onlyDisplayWhileMouseHovering);

                    EnumFlagsField contentMaskEnumFlags = new EnumFlagsField(settings.contentDisplay);
                    contentMaskEnumFlags.StyleDisplay(onlyDisplayWhileMouseHovering.value);
                    contentMaskEnumFlags.label = "Content Mask";
                    onlyDisplayWhileMouseHovering.RegisterValueChangedCallback((evt) =>
                    {
                        contentMaskEnumFlags.StyleDisplay(evt.newValue);
                    });
                    contentMaskEnumFlags.RegisterValueChangedCallback((evt) =>
                    {
                        settings.contentDisplay = (ContentDisplay) evt.newValue;
                        settings.OnSettingsChanged(nameof(settings.contentDisplay));
                    });
                    verticalLayout.Add(contentMaskEnumFlags);

                    Label Theme = new Label("Theme");
                    Theme.StyleFont(FontStyle.Bold);
                    Theme.StyleMargin(0, 0, TITLE_MARGIN_TOP, TITLE_MARGIN_BOTTOM);
                    verticalLayout.Add(Theme);

                    if (EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        EditorHelpBox themeWarningPlaymode =
                            new EditorHelpBox("This setting only available on edit mode.", MessageType.Info);
                        verticalLayout.Add(themeWarningPlaymode);
                    }
                    else
                    {
                        EditorHelpBox selectionColorHelpBox = new EditorHelpBox(
                            "Theme selection color require editor assembly recompile to take affect.\nBy selecting any script, right click -> Reimport. it will force the editor to recompile.",
                            MessageType.Info);
                        selectionColorHelpBox.StyleDisplay(false);
                        verticalLayout.Add(selectionColorHelpBox);

                        ColorField colorRowEven = new ColorField("Row Even");
                        colorRowEven.value = settings.usedTheme.colorRowEven;
                        colorRowEven.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                        colorRowEven.RegisterValueChangedCallback((evt) =>
                        {
                            if (EditorGUIUtility.isProSkin)
                                settings.professionalTheme.colorRowEven = evt.newValue;
                            else
                                settings.personalTheme.colorRowEven = evt.newValue;

                            settings.OnSettingsChanged();
                        });
                        verticalLayout.Add(colorRowEven);

                        ColorField colorRowOdd = new ColorField("Row Odd");
                        colorRowOdd.value = settings.usedTheme.colorRowOdd;
                        colorRowOdd.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                        colorRowOdd.RegisterValueChangedCallback((evt) =>
                        {
                            if (EditorGUIUtility.isProSkin)
                                settings.professionalTheme.colorRowOdd = evt.newValue;
                            else
                                settings.personalTheme.colorRowOdd = evt.newValue;

                            settings.OnSettingsChanged();
                        });
                        verticalLayout.Add(colorRowOdd);

                        ColorField colorGrid = new ColorField("Grid Color");
                        colorGrid.value = settings.usedTheme.colorGrid;
                        colorGrid.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                        colorGrid.RegisterValueChangedCallback((evt) =>
                        {
                            if (EditorGUIUtility.isProSkin)
                                settings.professionalTheme.colorGrid = evt.newValue;
                            else
                                settings.personalTheme.colorGrid = evt.newValue;

                            settings.OnSettingsChanged();
                        });
                        verticalLayout.Add(colorGrid);

                        ColorField colorTreeView = new ColorField("TreeView");
                        colorTreeView.value = settings.usedTheme.colorTreeView;
                        colorTreeView.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                        colorTreeView.RegisterValueChangedCallback((evt) =>
                        {
                            if (EditorGUIUtility.isProSkin)
                                settings.professionalTheme.colorTreeView = evt.newValue;
                            else
                                settings.personalTheme.colorTreeView = evt.newValue;

                            settings.OnSettingsChanged();
                        });
                        verticalLayout.Add(colorTreeView);

                        ColorField colorLockIcon = new ColorField("Lock Icon");
                        colorLockIcon.value = settings.usedTheme.colorLockIcon;
                        colorLockIcon.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                        colorLockIcon.RegisterValueChangedCallback((evt) =>
                        {
                            if (EditorGUIUtility.isProSkin)
                                settings.professionalTheme.colorLockIcon = evt.newValue;
                            else
                                settings.personalTheme.colorLockIcon = evt.newValue;

                            settings.OnSettingsChanged();
                        });
                        verticalLayout.Add(colorLockIcon);

                        ColorField tagColor = new ColorField("Tag Text");
                        tagColor.value = settings.usedTheme.tagColor;
                        tagColor.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                        tagColor.RegisterValueChangedCallback((evt) =>
                        {
                            if (EditorGUIUtility.isProSkin)
                                settings.professionalTheme.tagColor = evt.newValue;
                            else
                                settings.personalTheme.tagColor = evt.newValue;

                            settings.OnSettingsChanged();
                        });
                        verticalLayout.Add(tagColor);

                        ColorField layerColor = new ColorField("Layer Text");
                        layerColor.value = settings.usedTheme.layerColor;
                        layerColor.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                        layerColor.RegisterValueChangedCallback((evt) =>
                        {
                            if (EditorGUIUtility.isProSkin)
                                settings.professionalTheme.layerColor = evt.newValue;
                            else
                                settings.personalTheme.layerColor = evt.newValue;

                            settings.OnSettingsChanged();
                        });
                        verticalLayout.Add(layerColor);

                        ColorField colorHeaderTitle = new ColorField("Header Title");
                        colorHeaderTitle.value = settings.usedTheme.colorHeaderTitle;
                        colorHeaderTitle.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                        colorHeaderTitle.RegisterValueChangedCallback((evt) =>
                        {
                            if (EditorGUIUtility.isProSkin)
                                settings.professionalTheme.colorHeaderTitle = evt.newValue;
                            else
                                settings.personalTheme.colorHeaderTitle = evt.newValue;

                            settings.OnSettingsChanged();
                        });
                        verticalLayout.Add(colorHeaderTitle);

                        ColorField colorHeaderBackground = new ColorField("Header Background");
                        colorHeaderBackground.value = settings.usedTheme.colorHeaderBackground;
                        colorHeaderBackground.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                        colorHeaderBackground.RegisterValueChangedCallback((evt) =>
                        {
                            if (EditorGUIUtility.isProSkin)
                                settings.professionalTheme.colorHeaderBackground = evt.newValue;
                            else
                                settings.personalTheme.colorHeaderBackground = evt.newValue;

                            settings.OnSettingsChanged();
                        });
                        verticalLayout.Add(colorHeaderBackground);

                        ColorField comSelBGColor = new ColorField("Component Selection");
                        comSelBGColor.value = settings.usedTheme.comSelBGColor;
                        comSelBGColor.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                        comSelBGColor.RegisterValueChangedCallback((evt) =>
                        {
                            if (EditorGUIUtility.isProSkin)
                                settings.professionalTheme.comSelBGColor = evt.newValue;
                            else
                                settings.personalTheme.comSelBGColor = evt.newValue;

                            settings.OnSettingsChanged();
                        });
                        verticalLayout.Add(comSelBGColor);

                        // ColorField selectionColor = new ColorField("Selection");
                        // selectionColor.value = settings.usedTheme.selectionColor;
                        // selectionColor.StyleMarginLeft(CONTENT_MARGIN_LEFT);
                        // selectionColor.RegisterValueChangedCallback((evt) =>
                        // {
                        //     if (EditorGUIUtility.isProSkin)
                        //         settings.professionalTheme.selectionColor = evt.newValue;
                        //     else
                        //         settings.personalTheme.selectionColor = evt.newValue;
                        //
                        //     selectionColorHelpBox.StyleDisplay(true);
                        //     settings.OnSettingsChanged();
                        // });
                        // verticalLayout.Add(selectionColor);
                    }
                },

                keywords = new HashSet<string>(new[] {"Hierarchy"})
            };

            return provider;
        }

        internal static HierarchySettings GetAssets()
        {
            var guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(HierarchySettings).Name));

            if (guids.Length > 0)
            {
                var asset = AssetDatabase.LoadAssetAtPath<HierarchySettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
                if (asset != null)
                    return asset;
            }

            return null;
        }

        internal static HierarchySettings CreateAssets()
        {
            String path = EditorUtility.SaveFilePanelInProject("Save as...", "Settings", "asset", "");
            if (path.Length > 0)
            {
                HierarchySettings settings = ScriptableObject.CreateInstance<HierarchySettings>();
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = settings;
                return settings;
            }

            return null;
        }
    }

    [CustomEditor(typeof(HierarchySettings))]
    internal class SettingsInspector : Editor
    {
        HierarchySettings settings;

        void OnEnable() => settings = target as HierarchySettings;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Go to Edit -> Project Settings -> Hierarchy tab", MessageType.Info);
            if (GUILayout.Button("Open Settings"))
                SettingsService.OpenProjectSettings("Project/Hierarchy");
        }
    }
}