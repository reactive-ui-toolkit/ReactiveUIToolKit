using System;
using System.Reflection;
using ReactiveUITK.Core;
using UnityEngine;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// Assigns ref targets for uGUI hosts. Supported shapes:
    /// <c>Ref&lt;GameObject&gt;</c>, <c>Ref&lt;RectTransform&gt;</c>,
    /// <c>Ref&lt;Transform&gt;</c>, <c>Ref&lt;T&gt;</c> for any Component type
    /// (resolved via GetComponent), and <c>Action&lt;GameObject&gt;</c>
    /// callback refs (invoked with null on removal, React-style).
    /// </summary>
    internal static class UguiRefUtility
    {
        internal static void Assign(object refTarget, GameObject go)
        {
            if (refTarget == null)
                return;

            switch (refTarget)
            {
                case Ref<GameObject> goRef:
                    goRef.Current = go;
                    return;
                case Ref<RectTransform> rtRef:
                    rtRef.Current = go != null ? go.transform as RectTransform : null;
                    return;
                case Ref<Transform> tRef:
                    tRef.Current = go != null ? go.transform : null;
                    return;
                case Action<GameObject> action:
                    action(go);
                    return;
            }

            TrySetGenericRef(refTarget, go);
        }

        private static void TrySetGenericRef(object refTarget, GameObject go)
        {
            var type = refTarget.GetType();
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Ref<>))
                return;

            var expected = type.GetGenericArguments()[0];
            PropertyInfo current = type.GetProperty("Current");
            if (current == null)
                return;

            if (go == null)
            {
                current.SetValue(refTarget, null);
                return;
            }

            if (typeof(Component).IsAssignableFrom(expected))
            {
                current.SetValue(refTarget, go.GetComponent(expected));
            }
            else if (expected.IsInstanceOfType(go))
            {
                current.SetValue(refTarget, go);
            }
        }
    }
}
