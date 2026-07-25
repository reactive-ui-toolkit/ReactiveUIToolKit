using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// Serialized references to Unity's builtin UI sprites (UISprite,
    /// Background, Knob, ...). Because the shipped asset references them,
    /// Unity includes builtin_extra in player builds — so runtime-created
    /// controls look exactly like the GameObject &gt; UI menu ones in the
    /// editor AND in players. Loaded once from the package's Resources
    /// folder; a missing asset degrades to null sprites (flat boxes),
    /// never throws.
    /// </summary>
    public sealed class UguiDefaultResources : ScriptableObject
    {
        public Sprite uiSprite;
        public Sprite background;
        public Sprite inputFieldBackground;
        public Sprite knob;
        public Sprite checkmark;
        public Sprite dropdownArrow;
        public Sprite mask;

        private static UguiDefaultResources s_instance;
        private static bool s_loaded;

        public static UguiDefaultResources Instance
        {
            get
            {
                if (!s_loaded)
                {
                    s_loaded = true;
                    s_instance = Resources.Load<UguiDefaultResources>("UguiDefaultResources");
                }
                return s_instance;
            }
        }

        internal static DefaultControls.Resources GetLegacyResources()
        {
            var r = Instance;
            if (r == null)
                return new DefaultControls.Resources();
            return new DefaultControls.Resources
            {
                standard = r.uiSprite,
                background = r.background,
                inputField = r.inputFieldBackground,
                knob = r.knob,
                checkmark = r.checkmark,
                dropdown = r.dropdownArrow,
                mask = r.mask,
            };
        }

        internal static TMP_DefaultControls.Resources GetTmpResources()
        {
            var r = Instance;
            if (r == null)
                return new TMP_DefaultControls.Resources();
            return new TMP_DefaultControls.Resources
            {
                standard = r.uiSprite,
                background = r.background,
                inputField = r.inputFieldBackground,
                knob = r.knob,
                checkmark = r.checkmark,
                dropdown = r.dropdownArrow,
                mask = r.mask,
            };
        }
    }
}
