using System.Collections.Generic;
using Ruitk.Props;
using UnityEngine.UIElements;

namespace Ruitk.Elements
{
    public sealed class VisualElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            return new VisualElement();
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (properties == null)
            {
                return;
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}
