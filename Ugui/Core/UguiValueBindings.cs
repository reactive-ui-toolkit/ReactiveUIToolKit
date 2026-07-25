using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// One-time UnityEvent subscriptions per control; prop diffs are plain
    /// delegate-field writes (no listener churn, user listeners untouched).
    /// One binding component per control family.
    /// </summary>
    [AddComponentMenu("")]
    public sealed class UguiToggleBinding : MonoBehaviour
    {
        internal Action<bool> Current;
        internal bool AutoJoinGroup;

        internal static UguiToggleBinding GetOrAdd(GameObject go)
        {
            var b = go.GetComponent<UguiToggleBinding>();
            if (b == null)
            {
                b = go.AddComponent<UguiToggleBinding>();
                b.hideFlags = HideFlags.HideInInspector;
                var toggle = go.GetComponent<Toggle>();
                if (toggle != null)
                    toggle.onValueChanged.AddListener(v => b.Current?.Invoke(v));
            }
            return b;
        }

        private void Start()
        {
            TryJoinGroup();
        }

        private void OnTransformParentChanged()
        {
            TryJoinGroup();
        }

        private void TryJoinGroup()
        {
            if (!AutoJoinGroup)
                return;
            var toggle = GetComponent<Toggle>();
            if (toggle == null || toggle.group != null)
                return;
            toggle.group = GetComponentInParent<ToggleGroup>();
        }
    }

    [AddComponentMenu("")]
    public sealed class UguiSliderBinding : MonoBehaviour
    {
        internal Action<float> Current;

        internal static UguiSliderBinding GetOrAdd(GameObject go)
        {
            var b = go.GetComponent<UguiSliderBinding>();
            if (b == null)
            {
                b = go.AddComponent<UguiSliderBinding>();
                b.hideFlags = HideFlags.HideInInspector;
                var slider = go.GetComponent<Slider>();
                if (slider != null)
                    slider.onValueChanged.AddListener(v => b.Current?.Invoke(v));
            }
            return b;
        }
    }

    [AddComponentMenu("")]
    public sealed class UguiScrollbarBinding : MonoBehaviour
    {
        internal Action<float> Current;

        internal static UguiScrollbarBinding GetOrAdd(GameObject go)
        {
            var b = go.GetComponent<UguiScrollbarBinding>();
            if (b == null)
            {
                b = go.AddComponent<UguiScrollbarBinding>();
                b.hideFlags = HideFlags.HideInInspector;
                var bar = go.GetComponent<Scrollbar>();
                if (bar != null)
                    bar.onValueChanged.AddListener(v => b.Current?.Invoke(v));
            }
            return b;
        }
    }

    [AddComponentMenu("")]
    public sealed class UguiDropdownBinding : MonoBehaviour
    {
        internal Action<int> Current;

        internal static UguiDropdownBinding GetOrAdd(GameObject go)
        {
            var b = go.GetComponent<UguiDropdownBinding>();
            if (b == null)
            {
                b = go.AddComponent<UguiDropdownBinding>();
                b.hideFlags = HideFlags.HideInInspector;
                var dropdown = go.GetComponent<TMP_Dropdown>();
                if (dropdown != null)
                    dropdown.onValueChanged.AddListener(v => b.Current?.Invoke(v));
            }
            return b;
        }
    }

    [AddComponentMenu("")]
    public sealed class UguiInputFieldBinding : MonoBehaviour
    {
        internal Action<string> ValueChanged;
        internal Action<string> EndEdit;
        internal Action<string> Submit;

        internal static UguiInputFieldBinding GetOrAdd(GameObject go)
        {
            var b = go.GetComponent<UguiInputFieldBinding>();
            if (b == null)
            {
                b = go.AddComponent<UguiInputFieldBinding>();
                b.hideFlags = HideFlags.HideInInspector;
                var input = go.GetComponent<TMP_InputField>();
                if (input != null)
                {
                    input.onValueChanged.AddListener(v => b.ValueChanged?.Invoke(v));
                    input.onEndEdit.AddListener(v => b.EndEdit?.Invoke(v));
                    input.onSubmit.AddListener(v => b.Submit?.Invoke(v));
                }
            }
            return b;
        }
    }

    /// <summary>
    /// ScrollRect binding: onValueChanged bridge plus content auto-wiring —
    /// the user's single child of the viewport IS the scroll content, exactly
    /// as they would build it in the scene; the binding keeps
    /// ScrollRect.content pointed at it as children mount/swap.
    /// </summary>
    [AddComponentMenu("")]
    public sealed class UguiScrollRectBinding : MonoBehaviour
    {
        internal Action<Vector2> Current;
        internal RectTransform Viewport;

        private ScrollRect _scroll;

        internal static UguiScrollRectBinding GetOrAdd(GameObject go, RectTransform viewport)
        {
            var b = go.GetComponent<UguiScrollRectBinding>();
            if (b == null)
            {
                b = go.AddComponent<UguiScrollRectBinding>();
                b.hideFlags = HideFlags.HideInInspector;
                b._scroll = go.GetComponent<ScrollRect>();
                if (b._scroll != null)
                    b._scroll.onValueChanged.AddListener(v => b.Current?.Invoke(v));
            }
            b.Viewport = viewport;
            return b;
        }

        private void LateUpdate()
        {
            if (_scroll == null || Viewport == null)
                return;
            RectTransform content =
                Viewport.childCount > 0 ? Viewport.GetChild(0) as RectTransform : null;
            if (!ReferenceEquals(_scroll.content, content))
            {
                _scroll.content = content;
            }
        }
    }
}
