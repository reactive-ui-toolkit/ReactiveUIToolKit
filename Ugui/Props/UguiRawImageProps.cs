using UnityEngine;

namespace ReactiveUITK.Ugui
{
    public sealed class UguiRawImageProps : UguiGraphicProps
    {
        public Texture Texture { get; set; }
        public Rect? UvRect { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiRawImageProps o))
                return false;
            if (!ReferenceEquals(Texture, o.Texture))
                return false;
            if (UvRect != o.UvRect)
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            Texture = null;
            UvRect = null;
            base.__ResetFields();
        }

        internal override void __ReturnToPool()
        {
            Pool<UguiRawImageProps>.Return(this);
        }
    }
}
