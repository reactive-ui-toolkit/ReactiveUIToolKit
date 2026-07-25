using ReactiveUITK.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    internal static class UguiSelectableApplier
    {
        internal static void Apply(Selectable s, UguiSelectableProps prev, UguiSelectableProps next)
        {
            if (s == null || next == null)
                return;
            bool full = prev == null;

            if ((full || next.Interactable != prev.Interactable) && next.Interactable.HasValue)
                s.interactable = next.Interactable.Value;
            if ((full || next.Transition != prev.Transition) && next.Transition.HasValue)
                s.transition = next.Transition.Value;
            if (next.Colors.HasValue && (full || !next.Colors.Equals(prev.Colors)))
                s.colors = next.Colors.Value;
            if (next.SpriteState.HasValue && (full || !next.SpriteState.Equals(prev.SpriteState)))
                s.spriteState = next.SpriteState.Value;

            bool navChanged =
                full
                || next.NavigationMode != prev.NavigationMode
                || !ReferenceEquals(next.SelectOnUp, prev.SelectOnUp)
                || !ReferenceEquals(next.SelectOnDown, prev.SelectOnDown)
                || !ReferenceEquals(next.SelectOnLeft, prev.SelectOnLeft)
                || !ReferenceEquals(next.SelectOnRight, prev.SelectOnRight);
            if (navChanged && HasNavigationProps(next))
            {
                var nav = s.navigation;
                if (next.NavigationMode.HasValue)
                    nav.mode = next.NavigationMode.Value;
                if (next.SelectOnUp != null)
                    nav.selectOnUp = ResolveSelectable(next.SelectOnUp);
                if (next.SelectOnDown != null)
                    nav.selectOnDown = ResolveSelectable(next.SelectOnDown);
                if (next.SelectOnLeft != null)
                    nav.selectOnLeft = ResolveSelectable(next.SelectOnLeft);
                if (next.SelectOnRight != null)
                    nav.selectOnRight = ResolveSelectable(next.SelectOnRight);
                s.navigation = nav;
            }
        }

        private static bool HasNavigationProps(UguiSelectableProps p)
        {
            return p.NavigationMode.HasValue
                || p.SelectOnUp != null
                || p.SelectOnDown != null
                || p.SelectOnLeft != null
                || p.SelectOnRight != null;
        }

        private static Selectable ResolveSelectable(object target)
        {
            switch (target)
            {
                case Selectable s:
                    return s;
                case Ref<Selectable> sRef:
                    return sRef.Current;
                case Ref<GameObject> goRef:
                    return goRef.Current != null ? goRef.Current.GetComponent<Selectable>() : null;
                case GameObject go:
                    return go.GetComponent<Selectable>();
                default:
                    return null;
            }
        }
    }
}
