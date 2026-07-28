using System;
using Ruitk.Core;
using Ruitk.Core.Fiber;
using Ruitk.Elements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Ruitk.Ugui
{
    /// <summary>
    /// Props for the &lt;UitkHost&gt; island — a uGUI element hosting a
    /// nested UI Toolkit tree via PanelSettings.targetTexture. Supply a
    /// PanelSettings asset (themed) for production visuals; when omitted a
    /// minimal runtime PanelSettings is created (functional, default theme
    /// absent).
    /// </summary>
    public sealed class UguiUitkHostProps : UguiBaseProps
    {
        public Func<VirtualNode> Content { get; set; }
        public PanelSettings Panel { get; set; }

        public override bool ShallowEquals(UguiBaseProps other)
        {
            if (!(other is UguiUitkHostProps o))
                return false;
            if (Content != o.Content)
                return false;
            if (!ReferenceEquals(Panel, o.Panel))
                return false;
            return base.ShallowEquals(other);
        }

        internal override void __ResetFields()
        {
            Content = null;
            Panel = null;
            base.__ResetFields();
        }

        internal override void __ReturnToPool()
        {
            Pool<UguiUitkHostProps>.Return(this);
        }
    }

    /// <summary>
    /// Runtime bridge: keeps the RenderTexture sized to the RawImage rect,
    /// mounts the UI Toolkit fiber tree onto the panel, and maps pointer
    /// input from the uGUI rect into panel space via
    /// SetScreenToPanelSpaceFunction (NaN outside the rect, per the Unity
    /// render-texture-panel input contract).
    /// </summary>
    [AddComponentMenu("")]
    public sealed class UguiUitkHostBridge : MonoBehaviour
    {
        internal Func<VirtualNode> Content;

        private PanelSettings _settings;
        private bool _ownsSettings;
        private RenderTexture _texture;
        private UIDocument _document;
        private RawImage _rawImage;
        private FiberRenderer _renderer;
        private Func<VirtualNode> _renderedContent;

        internal void Configure(PanelSettings userSettings, Func<VirtualNode> content)
        {
            Content = content;
            if (_settings == null)
            {
                if (userSettings != null)
                {
                    _settings = userSettings;
                    _ownsSettings = false;
                }
                else
                {
                    _settings = ScriptableObject.CreateInstance<PanelSettings>();
                    _settings.name = "UitkHostPanelSettings";
                    _settings.scaleMode = PanelScaleMode.ConstantPixelSize;
                    _ownsSettings = true;
                }
                _settings.SetScreenToPanelSpaceFunction(ScreenToPanel);
            }
            _rawImage = GetComponent<RawImage>();
            EnsureDocument();
            RenderContent();
        }

        private void EnsureDocument()
        {
            if (_document == null)
            {
                _document = gameObject.GetComponent<UIDocument>();
                if (_document == null)
                {
                    _document = gameObject.AddComponent<UIDocument>();
                }
                _document.panelSettings = _settings;
            }
        }

        private void LateUpdate()
        {
            SyncTexture();
        }

        private void SyncTexture()
        {
            var rt = (RectTransform)transform;
            int width = Mathf.Max(1, Mathf.RoundToInt(rt.rect.width));
            int height = Mathf.Max(1, Mathf.RoundToInt(rt.rect.height));
            if (_texture != null && _texture.width == width && _texture.height == height)
            {
                return;
            }
            if (_texture != null)
            {
                _texture.Release();
                UnityEngine.Object.Destroy(_texture);
            }
            _texture = new RenderTexture(width, height, 24)
            {
                name = "UitkHostTexture",
            };
            _settings.targetTexture = _texture;
            if (_rawImage != null)
            {
                _rawImage.texture = _texture;
            }
            RenderContent();
        }

        private void RenderContent()
        {
            if (_document == null || _document.rootVisualElement == null || Content == null)
            {
                return;
            }
            if (_renderer == null)
            {
                _renderer = new FiberRenderer(
                    _document.rootVisualElement,
                    new HostContext(ElementRegistryProvider.GetDefaultRegistry())
                );
                _renderedContent = null;
            }
            if (_renderedContent != Content)
            {
                _renderedContent = Content;
                var content = Content;
                _renderer.Render(V.Func((props, children) => content()));
            }
        }

        private Vector2 ScreenToPanel(Vector2 screenPosition)
        {
            if (_rawImage == null || _texture == null)
            {
                return new Vector2(float.NaN, float.NaN);
            }
            var rt = (RectTransform)transform;
            var canvas = GetComponentInParent<Canvas>();
            Camera eventCamera =
                canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? canvas.worldCamera
                    : null;
            if (
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rt,
                    screenPosition,
                    eventCamera,
                    out var local
                )
            )
            {
                return new Vector2(float.NaN, float.NaN);
            }
            var rect = rt.rect;
            float u = (local.x - rect.xMin) / rect.width;
            float v = (local.y - rect.yMin) / rect.height;
            if (u < 0f || u > 1f || v < 0f || v > 1f)
            {
                return new Vector2(float.NaN, float.NaN);
            }
            return new Vector2(u * _texture.width, (1f - v) * _texture.height);
        }

        private void OnDestroy()
        {
            if (_renderer != null)
            {
                _renderer.Clear();
                _renderer = null;
            }
            if (_texture != null)
            {
                _texture.Release();
                UnityEngine.Object.Destroy(_texture);
                _texture = null;
            }
            if (_ownsSettings && _settings != null)
            {
                UnityEngine.Object.Destroy(_settings);
                _settings = null;
            }
        }
    }

    public sealed class UguiUitkHostAdapter : UguiElementAdapter
    {
        public override GameObject Create()
        {
            var go = new GameObject("UitkHost");
            var raw = go.AddComponent<RawImage>();
            raw.raycastTarget = true;
            go.AddComponent<UguiUitkHostBridge>();
            return go;
        }

        public override void ApplyTypedFull(GameObject go, UguiBaseProps props)
        {
            if (props is UguiUitkHostProps hp)
            {
                go.GetComponent<UguiUitkHostBridge>()?.Configure(hp.Panel, hp.Content);
            }
            base.ApplyTypedFull(go, props);
        }

        public override void ApplyTypedDiff(GameObject go, UguiBaseProps prev, UguiBaseProps next)
        {
            if (next is UguiUitkHostProps np)
            {
                var pp = prev as UguiUitkHostProps;
                if (pp == null || pp.Content != np.Content || !ReferenceEquals(pp.Panel, np.Panel))
                {
                    go.GetComponent<UguiUitkHostBridge>()?.Configure(np.Panel, np.Content);
                }
            }
            base.ApplyTypedDiff(go, prev, next);
        }
    }
}
