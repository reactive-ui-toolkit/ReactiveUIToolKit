using System;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Fiber;
using ReactiveUITK.Elements;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Canvas = UnityEngine.Canvas;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// Props for the &lt;UguiHost&gt; island — a UI Toolkit element hosting a
    /// nested uGUI tree. Content is a factory so the nested mount always
    /// renders fresh (pooled) vnodes inside its own render cycle.
    /// </summary>
    public sealed class UguiHostProps : BaseProps
    {
        public Func<VirtualNode> Content { get; set; }
        public int? SortingOrder { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!(other is UguiHostProps o))
                return false;
            if (Content != o.Content)
                return false;
            if (SortingOrder != o.SortingOrder)
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            Content = null;
            SortingOrder = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<UguiHostProps>.Return(this);
        }
    }

    /// <summary>
    /// The island VisualElement: owns a runtime screen-space-overlay Canvas
    /// whose island rect tracks this element's on-screen bounds (geometry
    /// synced through the panel's screen mapping). Input is native uGUI —
    /// the island is a real canvas under the EventSystem. Known limitation
    /// (documented): the island is not clipped by UI Toolkit overflow/masks.
    /// </summary>
    public sealed class UguiHostView : VisualElement
    {
        private GameObject _canvasGo;
        private RectTransform _islandRect;
        private Canvas _canvas;
        private FiberRenderer _renderer;
        private Func<VirtualNode> _content;
        private int _sortingOrder;

        public UguiHostView()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        internal void SetContent(Func<VirtualNode> content, int sortingOrder)
        {
            _content = content;
            _sortingOrder = sortingOrder;
            if (_canvas != null)
            {
                _canvas.sortingOrder = sortingOrder;
            }
            RenderContent();
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            EnsureCanvas();
            RenderContent();
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            TearDown();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            SyncIslandRect();
        }

        private void EnsureCanvas()
        {
            if (_canvasGo != null)
            {
                return;
            }
            _canvasGo = new GameObject("UguiHostIsland");
            _canvasGo.hideFlags = HideFlags.DontSave;
            _canvas = _canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = _sortingOrder;
            _canvasGo.AddComponent<GraphicRaycaster>();

            var island = new GameObject("Island", typeof(RectTransform));
            _islandRect = (RectTransform)island.transform;
            _islandRect.SetParent(_canvasGo.transform, false);
            _islandRect.anchorMin = Vector2.zero;
            _islandRect.anchorMax = Vector2.zero;
            _islandRect.pivot = Vector2.zero;

            var registry = UguiElementRegistryProvider.GetDefaultRegistry();
            var context = new HostContext(
                ElementRegistryProvider.GetDefaultRegistry(),
                new UguiHostConfig(registry)
            );
            if (RenderScheduler.Instance != null)
            {
                context.Environment["scheduler"] = RenderScheduler.Instance;
            }
            _renderer = new FiberRenderer((object)island, context);
            SyncIslandRect();
        }

        private void TearDown()
        {
            if (_renderer != null)
            {
                _renderer.Clear();
                _renderer = null;
            }
            if (_canvasGo != null)
            {
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(_canvasGo);
                else
                    UnityEngine.Object.DestroyImmediate(_canvasGo);
                _canvasGo = null;
                _islandRect = null;
                _canvas = null;
            }
        }

        private void RenderContent()
        {
            if (_renderer == null || _content == null)
            {
                return;
            }
            var content = _content;
            _renderer.Render(V.Func((props, children) => content()));
        }

        private void SyncIslandRect()
        {
            if (_islandRect == null || panel == null)
            {
                return;
            }
            var bound = worldBound;
            if (float.IsNaN(bound.width) || float.IsNaN(bound.height))
            {
                return;
            }

            Vector2 screenTopLeft = PanelToScreen(new Vector2(bound.xMin, bound.yMin));
            Vector2 screenBottomRight = PanelToScreen(new Vector2(bound.xMax, bound.yMax));
            float width = Mathf.Abs(screenBottomRight.x - screenTopLeft.x);
            float height = Mathf.Abs(screenBottomRight.y - screenTopLeft.y);

            _islandRect.anchoredPosition = new Vector2(
                screenTopLeft.x,
                Screen.height - screenTopLeft.y - height
            );
            _islandRect.sizeDelta = new Vector2(width, height);
        }

        /// <summary>
        /// Maps panel coordinates to screen coordinates by measuring the
        /// panel's screen-to-panel transform at two points and inverting the
        /// affine map (RuntimePanelUtils only exposes the forward direction).
        /// Identity for unscaled panels.
        /// </summary>
        private Vector2 PanelToScreen(Vector2 panelPos)
        {
            try
            {
                Vector2 origin = RuntimePanelUtils.ScreenToPanel(panel, Vector2.zero);
                Vector2 unit = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(100f, 100f));
                Vector2 scale = (unit - origin) / 100f;
                if (Mathf.Approximately(scale.x, 0f) || Mathf.Approximately(scale.y, 0f))
                {
                    return panelPos;
                }
                return (panelPos - origin) / scale;
            }
            catch (InvalidCastException)
            {
                // Editor-window panels are not runtime panels; islands are a
                // runtime feature — fall back to identity mapping.
                return panelPos;
            }
        }
    }

    public sealed class UguiHostAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            return new UguiHostView();
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is UguiHostView view && props is UguiHostProps hp)
            {
                view.SetContent(hp.Content, hp.SortingOrder ?? 0);
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (element is UguiHostView view && next is UguiHostProps np)
            {
                var pp = prev as UguiHostProps;
                if (pp == null || pp.Content != np.Content || pp.SortingOrder != np.SortingOrder)
                {
                    view.SetContent(np.Content, np.SortingOrder ?? 0);
                }
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
