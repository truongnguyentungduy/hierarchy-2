using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

namespace DlfU.UIElements
{
#if UNITY_EDITOR
    namespace Editor
    {
        public class VisualWindow : VisualElement
        {
            public string Label { get { return labelElement.text; } set { labelElement.text = name = value; } }
            public Label labelElement;
            public VisualElement contentElement;
            public HorizontalLayout headerElement;

            // bool registerCallbackToParent = false;

            public VisualWindow()
            {
                this.StyleBorderRadius(4, 4, 4, 4);
                this.StyleBorderWidth(1, 1, 2, 2);
                Color borderColor = new Color32(58, 121, 187, 255);
                this.StyleBorderColor(Color.grey, Color.grey, borderColor, borderColor);
                Color backgroundColor = EditorGUIUtility.isProSkin ? new Color32(64, 64, 64, 255) : new Color32(184, 184, 184, 255);
                this.StyleBackgroundColor(backgroundColor);
                this.StyleMinSize(128, 32);
                this.StyleMaxSize(9999, 9999);
                this.StylePosition(Position.Absolute);
                this.AddManipulator(new UIDragger(this, true));

                this.RegisterCallback<MouseOverEvent>(RegisterParentGeometryChanged);


                headerElement = new HorizontalLayout();
                headerElement.name = nameof(headerElement).ToUpper();
                headerElement.StyleMaxHeight(24);
                headerElement.StyleMinHeight(24);
                headerElement.StylePadding(18, 18, 4, 4);
                headerElement.StyleJustifyContent(Justify.Center);
                headerElement.RegisterCallback<MouseUpEvent>((callback) =>
                {
                    if (callback.button == 2)
                    {
                        this.style.width = this.style.height = StyleKeyword.Auto;
                        Repaint();
                    }
                });
                base.Add(headerElement);

                labelElement = new Label();
                labelElement.name = nameof(labelElement).ToUpper();
                name = labelElement.text = "Untitled";
                labelElement.StyleMargin(0, 0, 0, 2);
                labelElement.StyleAlignSelf(Align.Center);
                labelElement.StyleFont(FontStyle.Bold);
                headerElement.Add(labelElement);

                Image closeButton = new Image();
                closeButton.name = nameof(closeButton).ToUpper();
                closeButton.image = EditorGUIUtility.IconContent("winbtn_win_close").image;
                closeButton.StyleSize(16, 16);
                closeButton.StyleBorderWidth(1, 0, 0, 1);
                closeButton.StyleBorderRadius(0, 0, 4, 0);
                closeButton.StyleBorderColor(borderColor);
                closeButton.StylePosition(Position.Absolute);
                closeButton.StyleAlignSelf(Align.FlexEnd);
                base.Add(closeButton);

                closeButton.RegisterCallback<MouseDownEvent>((e) =>
                {
                    this.StyleDisplay(DisplayStyle.None);
                });

                contentElement = new VisualElement();
                contentElement.name = nameof(contentElement).ToUpper();
                contentElement.StyleMargin(0, 0, 0, 9);
                base.Add(contentElement);

                Image resizeButton = new Image();
                resizeButton.name = nameof(resizeButton).ToUpper();
                resizeButton.StyleAlignSelf(Align.FlexEnd);
                resizeButton.StylePosition(Position.Absolute);
                resizeButton.StyleSize(8, 8);
                resizeButton.StyleBorderRadius(0, 0, 0, 4);
                resizeButton.StyleBorderWidth(0, 2, 0, 2);
                resizeButton.StyleBorderColor(Color.grey);
                resizeButton.StyleRight(1);
                resizeButton.StyleBottom(1);
                base.Add(resizeButton);
                var uIResizer = new UIResizer(this);
                resizeButton.AddManipulator(uIResizer);
                uIResizer.mouseMoveAction += () =>
                {
                    resizeButton.StyleRight(1);
                    resizeButton.StyleBottom(1);
                };
            }

            void RegisterParentGeometryChanged(MouseOverEvent e)
            {
                if (parent != null)
                {
                    parent.RegisterCallback<GeometryChangedEvent>((callback) =>
                    {
                        Repaint();
                    });
                    UnregisterCallback<MouseOverEvent>(RegisterParentGeometryChanged);
                }
            }

            void Repaint()
            {
                this.style.left = Mathf.Clamp(this.style.left.value.value, 0f, this.parent.layout.width - this.layout.width);
                this.style.top = Mathf.Clamp(this.style.top.value.value, 0f, this.parent.layout.height - this.layout.height);
                this.style.right = float.NaN;
                this.style.bottom = float.NaN;
            }

            public new void Add(VisualElement visualElement)
            {
                contentElement.Add(visualElement);
            }
        }

        public class Foldout : VisualElement
        {
            public Image imageElement;
            public HorizontalLayout headerElement;
            public Label labelElement;
            public VerticalLayout contentElement;

            Image foloutImage;

            Texture onIcon = EditorGUIUtility.IconContent("IN foldout on@2x").image;
            Texture offIcon = EditorGUIUtility.IconContent("IN foldout@2x").image;

            bool value;

            public bool Value
            {
                get
                {
                    return value;
                }

                set
                {
                    this.value = value;
                    contentElement.StyleDisplay(this.value);
                    foloutImage.image = this.value ? onIcon : offIcon;
                }
            }

            public string Title { get { return labelElement.text; } set { labelElement.text = value; } }

            public Foldout() => Init("");

            public Foldout(string title) => Init(title);

            private void Init(string title)
            {
                this.StyleFont(FontStyle.Bold);
                this.StyleMinHeight(20);
                this.StyleBorderWidth(0, 0, 1, 0);
                Color borderColor = EditorGUIUtility.isProSkin ? new Color32(35, 35, 35, 255) : new Color32(153, 153, 153, 255);
                this.StyleBorderColor(borderColor);

                headerElement = new HorizontalLayout();
                headerElement.StyleHeight(21);
                headerElement.StyleMaxHeight(21);
                headerElement.StyleMinHeight(21);
                headerElement.StylePadding(4, 0, 0, 0);
                headerElement.StyleAlignItem(Align.Center);
                Color backgroundColor = EditorGUIUtility.isProSkin ? new Color32(80, 80, 80, 255) : new Color32(222, 222, 222, 255);
                headerElement.StyleBackgroundColor(backgroundColor);
                Color hoverBorderColor = new Color32(58, 121, 187, 255);
                headerElement.RegisterCallback<MouseEnterEvent>((evt) =>
                {
                    headerElement.StyleBorderWidth(1);
                    headerElement.StyleBorderColor(hoverBorderColor);
                });
                headerElement.RegisterCallback<MouseLeaveEvent>((evt) =>
                {
                    headerElement.StyleBorderWidth(0);
                    headerElement.StyleBorderColor(Color.clear);
                });
                base.Add(headerElement);

                contentElement = new VerticalLayout();
                contentElement.StyleDisplay(value);
                base.Add(contentElement);

                labelElement = new Label();
                labelElement.text = title;
                headerElement.Add(labelElement);

                imageElement = new Image();
                imageElement.name = nameof(imageElement);
                imageElement.StyleMargin(0, 4, 0, 0);
                imageElement.StyleSize(16, 16);
                headerElement.Add(imageElement);
                imageElement.SendToBack();
                imageElement.RegisterCallback<GeometryChangedEvent>((evt) =>
                {
                    imageElement.StyleDisplay(imageElement.image == null ? DisplayStyle.None : DisplayStyle.Flex);
                });

                foloutImage = new Image();
                foloutImage.StyleWidth(13);
                foloutImage.StyleMargin(0, 2, 0, 0);
                foloutImage.scaleMode = ScaleMode.ScaleToFit;
                foloutImage.image = value ? onIcon : offIcon;
                if (!EditorGUIUtility.isProSkin)
                    foloutImage.tintColor = Color.grey;
                headerElement.Add(foloutImage);
                foloutImage.SendToBack();


                headerElement.RegisterCallback<MouseUpEvent>((evt) =>
                {
                    if (evt.button == 0)
                    {
                        Value = !Value;
                        evt.StopPropagation();
                    }
                });
            }

            new public void Add(VisualElement visualElement)
            {
                contentElement.Add(visualElement);
            }
        }

        public class EditorHelpBox : VisualElement
        {
            public string Label { get { return label; } set { label = value; } }
            private string label = "";

            public EditorHelpBox(string text, MessageType messageType, bool wide = true)
            {
                style.marginLeft = style.marginRight = style.marginTop = style.marginBottom = 4;
                Label = text;

                IMGUIContainer iMGUIContainer = new IMGUIContainer(() =>
                {
                    EditorGUILayout.HelpBox(label, messageType, wide);
                });

                iMGUIContainer.name = nameof(IMGUIContainer);
                Add(iMGUIContainer);
            }
        }

        public class EditorIntSliderField : VisualElement
        {
            public SliderInt sliderInt;
            public IntegerField integerField;
            public VisualElement labelElement { get { return sliderInt.labelElement; } }
            public string Label { get { return sliderInt.label; } set { sliderInt.label = value; } }

            public EditorIntSliderField(string label, int value, int min, int max, EventCallback<ChangeEvent<int>> callback)
            {
                sliderInt = new SliderInt(label, min, max, SliderDirection.Horizontal);
                sliderInt.name = nameof(sliderInt);
                sliderInt.value = value;
                labelElement.StylePadding(0, 8, 0, 0);
                Add(sliderInt);

                integerField = new IntegerField();
                integerField.name = nameof(integerField);
                integerField.StyleWidth(64);
                integerField.style.paddingLeft = 4;
                integerField.style.marginRight = 0;
                integerField.value = value;
                integerField.RegisterValueChangedCallback(callback);
                integerField.RegisterValueChangedCallback((callbackChangedSlider) =>
                {
                    sliderInt.value = callbackChangedSlider.newValue;
                });
                sliderInt.Add(integerField);

                sliderInt.RegisterValueChangedCallback((callbackSlide) =>
                {
                    integerField.value = callbackSlide.newValue;
                });
            }
        }

        public class EditorFloatSliderField : VisualElement
        {
            public Slider slider;
            public FloatField floatField;
            public VisualElement labelElement { get { return slider.labelElement; } }
            public string Label { get { return slider.label; } set { slider.label = value; } }

            public EditorFloatSliderField(string label, float value, float min, float max, EventCallback<ChangeEvent<float>> callback)
            {
                slider = new Slider(label, min, max, SliderDirection.Horizontal);
                slider.name = nameof(slider);
                slider.value = value;
                labelElement.StylePadding(0, 8, 0, 0);
                Add(slider);

                floatField = new FloatField();
                floatField.name = nameof(floatField);
                floatField.StyleWidth(64);
                floatField.style.paddingLeft = 4;
                floatField.style.marginRight = 0;
                floatField.value = value;
                floatField.RegisterValueChangedCallback(callback);
                floatField.RegisterValueChangedCallback((callbackChangedSlider) =>
                {
                    slider.value = callbackChangedSlider.newValue;
                });
                slider.Add(floatField);

                slider.RegisterValueChangedCallback((callbackSlide) =>
                {
                    floatField.value = callbackSlide.newValue;
                });
            }
        }
    }
#endif

    public class HorizontalLayout : VisualElement
    {
        public HorizontalLayout()
        {
            name = nameof(HorizontalLayout);
            this.StyleFlexDirection(FlexDirection.Row);
            this.StyleFlexGrow(1);
        }
    }

    public class VerticalLayout : VisualElement
    {
        public VerticalLayout()
        {
            name = nameof(VerticalLayout);
            this.StyleFlexDirection(FlexDirection.Column);
            this.StyleFlexGrow(1);
        }
    }

    public class LabelImage : VisualElement
    {
        public Image imageElement;
        public Label labelElement;

        public string Label { get { return labelElement.text; } set { labelElement.text = value; } }

        public LabelImage(string label, Texture2D image)
        {
            imageElement = new Image();
            imageElement.image = image;
            imageElement.StyleSize(16, 16);
            Add(imageElement);

            labelElement = new Label();
            labelElement.text = label;
            Add(labelElement);
        }
    }

    public class Toggle : UnityEngine.UIElements.Toggle
    {
        public Toggle(string text, bool value, Justify contentJustify, EventCallback<ChangeEvent<bool>> callback)
        {
            this[0].StyleJustifyContent(contentJustify);
            this[0][0].StyleMargin(0, 8, 0, 0);
            this.text = text;
            this.value = value;
            this.RegisterValueChangedCallback(callback);
            this.StyleFont(value ? FontStyle.Italic : FontStyle.Normal);
            this.RegisterValueChangedCallback((internalCallback) => { this.StyleFont(internalCallback.newValue ? FontStyle.Italic : FontStyle.Normal); });
        }
    }





    public class UIDragger : MouseManipulator
    {
        protected bool active;
        protected VisualElement moveTarget;
        public Action mouseDownAction, mouseMoveAction, mouseUpAction;
        protected bool alwaysIn;


        public UIDragger(VisualElement targetMove = null, bool alwaysIn = true)
        {
            this.moveTarget = targetMove == null ? target : targetMove;
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            active = false;
            this.alwaysIn = alwaysIn;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected void OnMouseDown(MouseDownEvent e)
        {
            if (active)
            {
                e.StopImmediatePropagation();
                return;
            }

            if (CanStartManipulation(e))
            {
                active = true;
                mouseDownAction?.Invoke();
                target.CaptureMouse();
                e.StopPropagation();
            }
        }

        protected void OnMouseMove(MouseMoveEvent evt)
        {
            if (!active)
                return;

            float left = moveTarget.layout.x + evt.mouseDelta.x;
            float top = moveTarget.layout.y + evt.mouseDelta.y;

            if (alwaysIn)
            {
                moveTarget.style.left = Mathf.Clamp(left, 0f, moveTarget.parent.layout.width - moveTarget.layout.width);
                moveTarget.style.top = Mathf.Clamp(top, 0f, moveTarget.parent.layout.height - moveTarget.layout.height);
                moveTarget.style.right = float.NaN;
                moveTarget.style.bottom = float.NaN;
            }
            else
            {
                moveTarget.StyleLeft(left);
                moveTarget.StyleTop(top);
            }


            mouseMoveAction?.Invoke();
            evt.StopPropagation();
        }

        protected void OnMouseUp(MouseUpEvent evt)
        {
            active = false;

            mouseUpAction?.Invoke();

            if (target.HasMouseCapture())
                target.ReleaseMouse();

            evt.StopPropagation();
        }
    }

    public class UIResizer : MouseManipulator
    {
        private Vector2 m_Start;
        protected bool m_Active;
        private VisualElement targetResize;
        public Action mouseDownAction, mouseMoveAction, mouseUpAction;
        public UIResizer(VisualElement targetResize = null)
        {
            this.targetResize = targetResize == null ? target : targetResize;
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            m_Active = false;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected void OnMouseDown(MouseDownEvent e)
        {
            if (m_Active)
            {
                e.StopImmediatePropagation();
                return;
            }

            if (CanStartManipulation(e))
            {
                m_Start = e.localMousePosition;
                m_Active = true;

                mouseDownAction?.Invoke();
                target.CaptureMouse();
                e.StopPropagation();
            }
        }

        protected void OnMouseMove(MouseMoveEvent e)
        {
            if (!m_Active || !target.HasMouseCapture())
                return;

            Vector2 diff = e.localMousePosition - m_Start;

            float height = targetResize.layout.height + diff.y;
            float width = targetResize.layout.width + diff.x;

            Vector2 minSize = targetResize.StyleMinSize();
            Vector2 maxSize = targetResize.StyleMaxSize();

            height = Mathf.Clamp(height, minSize.y, maxSize.y);
            width = Mathf.Clamp(width, minSize.x, maxSize.x);

            while (width + targetResize.layout.xMin > targetResize.parent.layout.width)
            {
                width -= 1;
            }

            while (height + targetResize.layout.yMin > targetResize.parent.layout.height)
            {
                height -= 1;
            }

            targetResize.style.height = height;
            targetResize.style.width = width;

            mouseMoveAction?.Invoke();
            e.StopPropagation();
        }

        protected void OnMouseUp(MouseUpEvent e)
        {
            if (!m_Active || !target.HasMouseCapture() || !CanStopManipulation(e))
                return;

            m_Active = false;
            mouseUpAction?.Invoke();
            target.ReleaseMouse();
            e.StopPropagation();
        }
    }

    public static class UIElementsExstensions
    {
        public static void StyleDisplay(this VisualElement ui, DisplayStyle displayStyle) => ui.style.display = displayStyle;

        public static void StyleDisplay(this VisualElement ui, bool value) => ui.StyleDisplay(value ? DisplayStyle.Flex : DisplayStyle.None);

        public static bool IsDisplaying(this VisualElement ui) => ui.style.display == DisplayStyle.Flex;

        public static void StyleVisibility(this VisualElement ui, Visibility visibility) => ui.style.visibility = visibility;

        public static void StyleVisibility(this VisualElement ui, bool value) => ui.StyleVisibility(value ? Visibility.Visible : Visibility.Hidden);

        public static Vector2 StylePosition(this VisualElement ui)
        {
            return new Vector2(ui.style.left.value.value, ui.style.top.value.value);
        }

        public static Vector2 StyleSize(this VisualElement ui)
        {
            return new Vector2(ui.style.width.value.value, ui.style.height.value.value);
        }

        public static Vector2 StyleMinSize(this VisualElement ui)
        {
            return new Vector2(ui.style.minWidth.value.value, ui.style.minHeight.value.value);
        }

        public static Vector2 StyleMaxSize(this VisualElement ui)
        {
            return new Vector2(ui.style.maxWidth.value.value, ui.style.maxHeight.value.value);
        }

        public static void StylePosition(this VisualElement ui, Vector2 position)
        {
            ui.StyleLeft(position.x);
            ui.StyleTop(position.y);
        }

        public static void StylePosition(this VisualElement ui, StyleLength x, StyleLength y)
        {
            ui.StyleLeft(x);
            ui.StyleTop(y);
        }

        public static void StyleTop(this VisualElement ui, StyleLength value) => ui.style.top = value;

        public static void StyleBottom(this VisualElement ui, StyleLength value) => ui.style.bottom = value;

        public static void StyleLeft(this VisualElement ui, StyleLength value) => ui.style.left = value;

        public static void StyleRight(this VisualElement ui, StyleLength value) => ui.style.right = value;

        public static float StyleTop(this VisualElement ui) => ui.style.top.value.value;

        public static float StyleBottom(this VisualElement ui) => ui.style.bottom.value.value;

        public static float StyleLeft(this VisualElement ui) => ui.style.left.value.value;

        public static float StyleRight(this VisualElement ui) => ui.style.right.value.value;

        public static void StylePosition(this VisualElement ui, Position type) => ui.style.position = type;

        public static void StyleSize(this VisualElement ui, StyleLength width, StyleLength height)
        {
            ui.StyleWidth(width);
            ui.StyleHeight(height);
        }

        public static void StyleSize(this VisualElement ui, Vector2 size) => StyleSize(ui, size.x, size.y);

        public static void StyleMinSize(this VisualElement ui, StyleLength width, StyleLength height)
        {
            ui.StyleMinWidth(width);
            ui.StyleMinHeight(height);
        }

        public static void StyleMaxSize(this VisualElement ui, StyleLength width, StyleLength height)
        {
            ui.StyleMaxWidth(width);
            ui.StyleMaxHeight(height);
        }


        public static void StyleWidth(this VisualElement ui, StyleLength width) => ui.style.width = width;

        public static void StyleMinWidth(this VisualElement ui, StyleLength width) => ui.style.minWidth = width;

        public static void StyleMaxWidth(this VisualElement ui, StyleLength width) => ui.style.maxWidth = width;

        public static void StyleHeight(this VisualElement ui, StyleLength height) => ui.style.height = height;

        public static void StyleMinHeight(this VisualElement ui, StyleLength height) => ui.style.minHeight = height;

        public static void StyleMaxHeight(this VisualElement ui, StyleLength height) => ui.style.maxHeight = height;

        public static void StyleFont(this VisualElement ui, FontStyle fontStyle) => ui.style.unityFontStyleAndWeight = fontStyle;

        public static void StyleFontSize(this VisualElement ui, StyleLength size) => ui.style.fontSize = size;

        public static void StyleTextAlign(this VisualElement ui, TextAnchor textAnchor) => ui.style.unityTextAlign = textAnchor;

        public static void StyleAlignSelf(this VisualElement ui, Align align) => ui.style.alignSelf = align;

        public static void StyleAlignItem(this VisualElement ui, Align align) => ui.style.alignItems = align;

        public static void StyleJustifyContent(this VisualElement ui, Justify justify) => ui.style.justifyContent = justify;

        public static void StyleFlexDirection(this VisualElement ui, FlexDirection flexDirection) => ui.style.flexDirection = flexDirection;

        public static void StyleMargin(this VisualElement ui, StyleLength value) => ui.StyleMargin(value, value, value, value);

        public static void StyleMargin(this VisualElement ui, StyleLength left, StyleLength right, StyleLength top, StyleLength bottom)
        {
            ui.style.marginLeft = left;
            ui.style.marginRight = right;
            ui.style.marginTop = top;
            ui.style.marginBottom = bottom;
        }

        public static void StyleMarginLeft(this VisualElement ui, StyleLength value) => ui.style.marginLeft = value;

        public static void StyleMarginRight(this VisualElement ui, StyleLength value) => ui.style.marginRight = value;

        public static void StyleMarginTop(this VisualElement ui, StyleLength value) => ui.style.marginTop = value;

        public static void StyleMarginBottom(this VisualElement ui, StyleLength value) => ui.style.marginBottom = value;

        public static void StylePadding(this VisualElement ui, StyleLength value) => ui.StylePadding(value, value, value, value);

        public static void StylePadding(this VisualElement ui, StyleLength left, StyleLength right, StyleLength top, StyleLength bottom)
        {
            ui.style.paddingLeft = left;
            ui.style.paddingRight = right;
            ui.style.paddingTop = top;
            ui.style.paddingBottom = bottom;
        }

        public static void StylePaddingLeft(this VisualElement ui, StyleLength value) => ui.style.paddingLeft = value;

        public static void StylePaddingRight(this VisualElement ui, StyleLength value) => ui.style.paddingRight = value;

        public static void StylePaddingTop(this VisualElement ui, StyleLength value) => ui.style.paddingTop = value;

        public static void StylePaddingBottom(this VisualElement ui, StyleLength value) => ui.style.paddingBottom = value;

        public static void StyleBorderRadius(this VisualElement ui, StyleLength radius) => ui.StyleBorderRadius(radius, radius, radius, radius);

        public static void StyleBorderRadius(this VisualElement ui, StyleLength topLeft, StyleLength topRight, StyleLength bottomLeft, StyleLength bottomRight)
        {
            ui.style.borderTopLeftRadius = topLeft;
            ui.style.borderTopRightRadius = topRight;
            ui.style.borderBottomLeftRadius = bottomLeft;
            ui.style.borderBottomRightRadius = bottomRight;
        }

        public static void StyleBorderWidth(this VisualElement ui, StyleFloat width) => ui.StyleBorderWidth(width, width, width, width);

        public static void StyleBorderWidth(this VisualElement ui, StyleFloat left, StyleFloat right, StyleFloat top, StyleFloat bottom)
        {
            ui.style.borderLeftWidth = left;
            ui.style.borderRightWidth = right;
            ui.style.borderTopWidth = top;
            ui.style.borderBottomWidth = bottom;
        }

        public static void StyleBorderColor(this VisualElement ui, StyleColor color) => ui.StyleBorderColor(color, color, color, color);

        public static void StyleBorderColor(this VisualElement ui, StyleColor left, StyleColor right, StyleColor top, StyleColor bottom)
        {
            ui.style.borderLeftColor = left;
            ui.style.borderRightColor = right;
            ui.style.borderTopColor = top;
            ui.style.borderBottomColor = bottom;
        }

        public static void StyleFlexBasisAsPercent(this VisualElement ui, StyleLength basis) => ui.style.flexBasis = basis;

        public static void StyleFlexGrow(this VisualElement ui, StyleFloat grow) => ui.style.flexGrow = grow;

        public static void StyleBackgroundColor(this VisualElement ui, StyleColor color) => ui.style.backgroundColor = color;

        public static void StyleTextColor(this VisualElement ui, StyleColor color) => ui.style.color = color;

        public static VisualElement FindChildren(this VisualElement ui, string name)
        {
            return ui.Children().ToList().Find(childElement => childElement.name == name);
        }

        public static T FindChildren<T>(this VisualElement ui, string name) where T : VisualElement
        {
            return ui.FindChildren(name) as T;
        }

        public static VisualElement FindChildrenPhysicalHierarchy(this VisualElement ui, string name)
        {
            return ui.hierarchy.Children().ToList().Find(childElement => childElement.name == name);
        }

        public static VisualElement FindChildrenPhysicalHierarchy<T>(this VisualElement ui, string name) where T : VisualElement
        {
            return ui.FindChildrenPhysicalHierarchy(name) as T;
        }
    }
}
