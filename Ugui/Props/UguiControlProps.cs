using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    public sealed class UguiToggleProps : UguiSelectableProps
    {
        public bool? IsOn { get; set; }
        public Action<bool> OnValueChanged { get; set; }
        public Sprite CheckmarkSprite { get; set; }
        public Color? CheckmarkColor { get; set; }
        public ToggleGroup Group { get; set; }
        public bool? JoinGroup { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiToggleProps o))
                return false;
            if (IsOn != o.IsOn)
                return false;
            if (OnValueChanged != o.OnValueChanged)
                return false;
            if (!ReferenceEquals(CheckmarkSprite, o.CheckmarkSprite))
                return false;
            if (CheckmarkColor != o.CheckmarkColor)
                return false;
            if (!ReferenceEquals(Group, o.Group))
                return false;
            if (JoinGroup != o.JoinGroup)
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            IsOn = null;
            OnValueChanged = null;
            CheckmarkSprite = null;
            CheckmarkColor = null;
            Group = null;
            JoinGroup = null;
            base.__ResetFields();
        }

        internal override void __ReturnToPool()
        {
            Pool<UguiToggleProps>.Return(this);
        }
    }

    public sealed class UguiSliderProps : UguiSelectableProps
    {
        public float? MinValue { get; set; }
        public float? MaxValue { get; set; }
        public bool? WholeNumbers { get; set; }
        public float? Value { get; set; }
        public Slider.Direction? Direction { get; set; }
        public Action<float> OnValueChanged { get; set; }
        public Sprite BackgroundSprite { get; set; }
        public Color? BackgroundColor { get; set; }
        public Sprite FillSprite { get; set; }
        public Color? FillColor { get; set; }
        public Sprite HandleSprite { get; set; }
        public Color? HandleColor { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiSliderProps o))
                return false;
            if (MinValue != o.MinValue)
                return false;
            if (MaxValue != o.MaxValue)
                return false;
            if (WholeNumbers != o.WholeNumbers)
                return false;
            if (Value != o.Value)
                return false;
            if (Direction != o.Direction)
                return false;
            if (OnValueChanged != o.OnValueChanged)
                return false;
            if (!ReferenceEquals(BackgroundSprite, o.BackgroundSprite))
                return false;
            if (BackgroundColor != o.BackgroundColor)
                return false;
            if (!ReferenceEquals(FillSprite, o.FillSprite))
                return false;
            if (FillColor != o.FillColor)
                return false;
            if (!ReferenceEquals(HandleSprite, o.HandleSprite))
                return false;
            if (HandleColor != o.HandleColor)
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            MinValue = null;
            MaxValue = null;
            WholeNumbers = null;
            Value = null;
            Direction = null;
            OnValueChanged = null;
            BackgroundSprite = null;
            BackgroundColor = null;
            FillSprite = null;
            FillColor = null;
            HandleSprite = null;
            HandleColor = null;
            base.__ResetFields();
        }

        internal override void __ReturnToPool()
        {
            Pool<UguiSliderProps>.Return(this);
        }
    }

    public sealed class UguiScrollbarProps : UguiSelectableProps
    {
        public float? Value { get; set; }
        public float? Size { get; set; }
        public int? NumberOfSteps { get; set; }
        public Scrollbar.Direction? Direction { get; set; }
        public Action<float> OnValueChanged { get; set; }
        public Sprite HandleSprite { get; set; }
        public Color? HandleColor { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiScrollbarProps o))
                return false;
            if (Value != o.Value)
                return false;
            if (Size != o.Size)
                return false;
            if (NumberOfSteps != o.NumberOfSteps)
                return false;
            if (Direction != o.Direction)
                return false;
            if (OnValueChanged != o.OnValueChanged)
                return false;
            if (!ReferenceEquals(HandleSprite, o.HandleSprite))
                return false;
            if (HandleColor != o.HandleColor)
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            Value = null;
            Size = null;
            NumberOfSteps = null;
            Direction = null;
            OnValueChanged = null;
            HandleSprite = null;
            HandleColor = null;
            base.__ResetFields();
        }

        internal override void __ReturnToPool()
        {
            Pool<UguiScrollbarProps>.Return(this);
        }
    }

    public sealed class UguiScrollRectProps : UguiImageProps
    {
        public bool? Horizontal { get; set; }
        public bool? Vertical { get; set; }
        public ScrollRect.MovementType? MovementType { get; set; }
        public float? Elasticity { get; set; }
        public bool? Inertia { get; set; }
        public float? DecelerationRate { get; set; }
        public float? ScrollSensitivity { get; set; }
        public bool? ShowHorizontalScrollbar { get; set; }
        public bool? ShowVerticalScrollbar { get; set; }
        public Action<Vector2> OnValueChanged { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiScrollRectProps o))
                return false;
            if (Horizontal != o.Horizontal)
                return false;
            if (Vertical != o.Vertical)
                return false;
            if (MovementType != o.MovementType)
                return false;
            if (Elasticity != o.Elasticity)
                return false;
            if (Inertia != o.Inertia)
                return false;
            if (DecelerationRate != o.DecelerationRate)
                return false;
            if (ScrollSensitivity != o.ScrollSensitivity)
                return false;
            if (ShowHorizontalScrollbar != o.ShowHorizontalScrollbar)
                return false;
            if (ShowVerticalScrollbar != o.ShowVerticalScrollbar)
                return false;
            if (OnValueChanged != o.OnValueChanged)
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            Horizontal = null;
            Vertical = null;
            MovementType = null;
            Elasticity = null;
            Inertia = null;
            DecelerationRate = null;
            ScrollSensitivity = null;
            ShowHorizontalScrollbar = null;
            ShowVerticalScrollbar = null;
            OnValueChanged = null;
            base.__ResetFields();
        }

        internal override void __ReturnToPool()
        {
            Pool<UguiScrollRectProps>.Return(this);
        }
    }

    public sealed class UguiDropdownProps : UguiSelectableProps
    {
        public IReadOnlyList<string> Options { get; set; }
        public int? Value { get; set; }
        public Action<int> OnValueChanged { get; set; }
        public float? LabelFontSize { get; set; }
        public Color? LabelColor { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiDropdownProps o))
                return false;
            if (!OptionsEqual(Options, o.Options))
                return false;
            if (Value != o.Value)
                return false;
            if (OnValueChanged != o.OnValueChanged)
                return false;
            if (LabelFontSize != o.LabelFontSize)
                return false;
            if (LabelColor != o.LabelColor)
                return false;
            return base.ShallowEquals(other);
        }

        internal static bool OptionsEqual(IReadOnlyList<string> a, IReadOnlyList<string> b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a == null || b == null)
                return false;
            if (a.Count != b.Count)
                return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (!string.Equals(a[i], b[i], StringComparison.Ordinal))
                    return false;
            }
            return true;
        }

        internal override void __ResetFields()
        {
            Options = null;
            Value = null;
            OnValueChanged = null;
            LabelFontSize = null;
            LabelColor = null;
            base.__ResetFields();
        }

        internal override void __ReturnToPool()
        {
            Pool<UguiDropdownProps>.Return(this);
        }
    }

    public sealed class UguiInputFieldProps : UguiSelectableProps
    {
        public string Text { get; set; }
        public string Placeholder { get; set; }
        public int? CharacterLimit { get; set; }
        public TMP_InputField.ContentType? ContentType { get; set; }
        public TMP_InputField.LineType? LineType { get; set; }
        public bool? ReadOnly { get; set; }
        public float? FontSize { get; set; }
        public Color? TextColor { get; set; }
        public Color? PlaceholderColor { get; set; }
        public float? CaretBlinkRate { get; set; }
        public Color? SelectionColor { get; set; }
        public Action<string> OnValueChanged { get; set; }
        public Action<string> OnEndEdit { get; set; }
        public Action<string> OnSubmit { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiInputFieldProps o))
                return false;
            if (Text != o.Text)
                return false;
            if (Placeholder != o.Placeholder)
                return false;
            if (CharacterLimit != o.CharacterLimit)
                return false;
            if (ContentType != o.ContentType)
                return false;
            if (LineType != o.LineType)
                return false;
            if (ReadOnly != o.ReadOnly)
                return false;
            if (FontSize != o.FontSize)
                return false;
            if (TextColor != o.TextColor)
                return false;
            if (PlaceholderColor != o.PlaceholderColor)
                return false;
            if (CaretBlinkRate != o.CaretBlinkRate)
                return false;
            if (SelectionColor != o.SelectionColor)
                return false;
            if (OnValueChanged != o.OnValueChanged)
                return false;
            if (OnEndEdit != o.OnEndEdit)
                return false;
            if (OnSubmit != o.OnSubmit)
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            Text = null;
            Placeholder = null;
            CharacterLimit = null;
            ContentType = null;
            LineType = null;
            ReadOnly = null;
            FontSize = null;
            TextColor = null;
            PlaceholderColor = null;
            CaretBlinkRate = null;
            SelectionColor = null;
            OnValueChanged = null;
            OnEndEdit = null;
            OnSubmit = null;
            base.__ResetFields();
        }

        internal override void __ReturnToPool()
        {
            Pool<UguiInputFieldProps>.Return(this);
        }
    }
}
