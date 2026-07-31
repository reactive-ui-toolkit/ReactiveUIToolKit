using System.Collections.Generic;
using Ruitk.Core.Animation;

namespace Ruitk.Props.Typed
{
    public sealed class AnimateProps : BaseProps
    {
        public List<AnimateTrack> Tracks { get; set; }
        public bool Autoplay { get; set; } = true;

        internal override void __ResetFields()
        {
            Tracks = null;
            Autoplay = false;
        }

        internal override void __ReturnToPool()
        {
            Pool<AnimateProps>.Return(this);
        }
    }
}
