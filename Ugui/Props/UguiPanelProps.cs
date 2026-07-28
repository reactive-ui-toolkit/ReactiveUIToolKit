namespace Ruitk.Ugui
{
    /// <summary>
    /// Props for the Panel element — a plain RectTransform container with no
    /// Graphic. Created full-stretch by default (the uGUI "Panel" habit).
    /// </summary>
    public sealed class UguiPanelProps : UguiBaseProps
    {
        internal override void __ReturnToPool()
        {
            Pool<UguiPanelProps>.Return(this);
        }
    }
}
