using System;
using Ruitk.Core;

namespace Ruitk.Props.Typed
{
    public sealed class ErrorBoundaryProps : global::Ruitk.Core.IProps
    {
        public VirtualNode Fallback { get; set; }
        public ErrorEventHandler OnError { get; set; }
        public string ResetKey { get; set; }
    }
}
