using System;
using UnityEngine;
using UnityEngine.UI;

namespace Ruitk.Ugui
{
    /// <summary>
    /// Bridges Button.onClick to the current declarative handler. Subscribes
    /// to the UnityEvent exactly once; prop diffs are a single delegate-field
    /// write, so user-added listeners (via refs) are never disturbed and
    /// there is no listener churn per render.
    /// </summary>
    [AddComponentMenu("")]
    public sealed class UguiButtonBinding : MonoBehaviour
    {
        internal Action Current;
        private bool _subscribed;

        internal static UguiButtonBinding GetOrAdd(GameObject go)
        {
            var binding = go.GetComponent<UguiButtonBinding>();
            if (binding == null)
            {
                binding = go.AddComponent<UguiButtonBinding>();
                binding.hideFlags = HideFlags.HideInInspector;
            }
            binding.EnsureSubscribed();
            return binding;
        }

        private void EnsureSubscribed()
        {
            if (_subscribed)
                return;
            var button = GetComponent<Button>();
            if (button == null)
                return;
            button.onClick.AddListener(Invoke);
            _subscribed = true;
        }

        private void Invoke()
        {
            Current?.Invoke();
        }
    }
}
