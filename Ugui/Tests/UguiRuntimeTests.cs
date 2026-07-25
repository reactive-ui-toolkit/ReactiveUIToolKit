using System.Collections.Generic;
using NUnit.Framework;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Fiber;
using ReactiveUITK.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReactiveUITK.Ugui.Tests
{
    /// <summary>
    /// EditMode runtime tests for the uGUI backend: mounting, prop diffs,
    /// anchor presets, prop groups, events, controlled-component semantics,
    /// keyed reorder, and pooling. Renders are synchronous (no scheduler in
    /// the HostContext), so every assertion runs immediately after Render.
    /// </summary>
    public class UguiRuntimeTests
    {
        private GameObject _canvasGo;
        private RectTransform _mountRect;
        private FiberRenderer _renderer;

        [SetUp]
        public void SetUp()
        {
            _canvasGo = new GameObject("TestCanvas", typeof(Canvas));
            var mount = new GameObject("Mount", typeof(RectTransform));
            _mountRect = (RectTransform)mount.transform;
            _mountRect.SetParent(_canvasGo.transform, false);

            var context = new HostContext(
                ElementRegistryProvider.GetDefaultRegistry(),
                new UguiHostConfig(UguiElementRegistryProvider.GetDefaultRegistry())
            );
            _renderer = new FiberRenderer((object)_mountRect.gameObject, context);
        }

        [TearDown]
        public void TearDown()
        {
            _renderer?.Clear();
            if (_canvasGo != null)
                Object.DestroyImmediate(_canvasGo);
            var staging = GameObject.Find("ReactiveUITK.Ugui.Staging");
            if (staging != null)
                Object.DestroyImmediate(staging);
        }

        private GameObject Child(int index) => _mountRect.GetChild(index).gameObject;

        // ── Mounting & structure ─────────────────────────────────────────────

        [Test]
        public void Mount_LayoutGroupWithTextAndButton_BuildsExpectedHierarchy()
        {
            var group = UguiBaseProps.__Rent<UguiVerticalLayoutGroupProps>();
            group.Spacing = 12f;
            group.PaddingLeft = 16;
            var text = UguiBaseProps.__Rent<UguiTextProps>();
            text.Text = "Hello";
            var button = UguiBaseProps.__Rent<UguiButtonProps>();

            _renderer.Render(U.VerticalLayoutGroup(group, null, U.Text(text), U.Button(button)));

            Assert.AreEqual(1, _mountRect.childCount);
            var groupGo = Child(0);
            var layout = groupGo.GetComponent<VerticalLayoutGroup>();
            Assert.NotNull(layout);
            Assert.AreEqual(12f, layout.spacing);
            Assert.AreEqual(16, layout.padding.left);
            Assert.AreEqual(2, groupGo.transform.childCount);
            var tmp = groupGo.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            Assert.NotNull(tmp);
            Assert.AreEqual("Hello", tmp.text);
            Assert.NotNull(groupGo.transform.GetChild(1).GetComponent<Button>());
        }

        [Test]
        public void Rerender_ChangedText_UpdatesInPlace()
        {
            var t1 = UguiBaseProps.__Rent<UguiTextProps>();
            t1.Text = "One";
            _renderer.Render(U.Text(t1));
            var textGo = Child(0);

            var t2 = UguiBaseProps.__Rent<UguiTextProps>();
            t2.Text = "Two";
            _renderer.Render(U.Text(t2));

            Assert.AreSame(textGo, Child(0), "diff must update the same GameObject");
            Assert.AreEqual("Two", textGo.GetComponent<TextMeshProUGUI>().text);
        }

        // ── Anchor presets ───────────────────────────────────────────────────

        [Test]
        public void AnchorPreset_TopStretch_AppliesInspectorTriple()
        {
            var props = UguiBaseProps.__Rent<UguiPanelProps>();
            props.Anchors = UguiAnchorPreset.TopStretch;
            props.SizeDelta = new Vector2(0f, 40f);
            _renderer.Render(U.Panel(props));

            var rt = (RectTransform)Child(0).transform;
            Assert.AreEqual(new Vector2(0f, 1f), rt.anchorMin);
            Assert.AreEqual(new Vector2(1f, 1f), rt.anchorMax);
            Assert.AreEqual(new Vector2(0.5f, 1f), rt.pivot);
            Assert.AreEqual(new Vector2(0f, 40f), rt.sizeDelta);
        }

        // ── Prop groups (add / update / remove) ──────────────────────────────

        [Test]
        public void LayoutElementGroup_AddUpdateRemove_ManagesComponent()
        {
            var p1 = UguiBaseProps.__Rent<UguiImageProps>();
            p1.LayoutElement = new UguiLayoutElement { MinHeight = 40f };
            _renderer.Render(U.Image(p1));
            var go = Child(0);
            var le = go.GetComponent<LayoutElement>();
            Assert.NotNull(le);
            Assert.AreEqual(40f, le.minHeight);

            var p2 = UguiBaseProps.__Rent<UguiImageProps>();
            p2.LayoutElement = new UguiLayoutElement { MinHeight = 64f };
            _renderer.Render(U.Image(p2));
            Assert.AreEqual(64f, go.GetComponent<LayoutElement>().minHeight);

            var p3 = UguiBaseProps.__Rent<UguiImageProps>();
            _renderer.Render(U.Image(p3));
            Assert.IsNull(go.GetComponent<LayoutElement>(), "null-transition must remove");
        }

        [Test]
        public void CanvasGroupProp_AppliesAlphaAndInteractable()
        {
            var props = UguiBaseProps.__Rent<UguiPanelProps>();
            props.CanvasGroup = new UguiCanvasGroup { Alpha = 0.5f, Interactable = false };
            _renderer.Render(U.Panel(props));

            var cg = Child(0).GetComponent<CanvasGroup>();
            Assert.NotNull(cg);
            Assert.AreEqual(0.5f, cg.alpha);
            Assert.IsFalse(cg.interactable);
        }

        // ── Events & controlled components ───────────────────────────────────

        [Test]
        public void ButtonClick_DrivesStateAndRerender()
        {
            int clicks = 0;
            VirtualNode Render(IProps p, IReadOnlyList<VirtualNode> c)
            {
                var (count, setCount) = Hooks.UseState(0);
                clicks = count;
                var bp = UguiBaseProps.__Rent<UguiButtonProps>();
                bp.OnClick = () => setCount(count + 1);
                var tp = UguiBaseProps.__Rent<UguiTextProps>();
                tp.Text = $"Count: {count}";
                return U.Button(bp, null, U.Text(tp));
            }

            _renderer.Render(V.Func(Render));
            var button = Child(0).GetComponent<Button>();
            button.onClick.Invoke();

            Assert.AreEqual(1, clicks);
            var tmp = Child(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            Assert.AreEqual("Count: 1", tmp.text);
        }

        [Test]
        public void ToggleIsOnProp_DoesNotEchoValueChanged()
        {
            int callbacks = 0;
            var p1 = UguiBaseProps.__Rent<UguiToggleProps>();
            p1.IsOn = false;
            p1.OnValueChanged = _ => callbacks++;
            _renderer.Render(U.Toggle(p1));

            var p2 = UguiBaseProps.__Rent<UguiToggleProps>();
            p2.IsOn = true;
            p2.OnValueChanged = p1.OnValueChanged;
            _renderer.Render(U.Toggle(p2));

            Assert.IsTrue(Child(0).GetComponent<Toggle>().isOn);
            Assert.AreEqual(0, callbacks, "controlled write must not fire onValueChanged");
        }

        // ── Keyed reorder ────────────────────────────────────────────────────

        [Test]
        public void KeyedReorder_PreservesInstances()
        {
            VirtualNode Item(string key)
            {
                var tp = UguiBaseProps.__Rent<UguiTextProps>();
                tp.Text = key;
                return U.Text(tp, key);
            }

            _renderer.Render(U.Panel(null, null, Item("a"), Item("b"), Item("c")));
            var panel = Child(0).transform;
            var a = panel.GetChild(0).gameObject;
            var c = panel.GetChild(2).gameObject;

            _renderer.Render(U.Panel(null, null, Item("c"), Item("b"), Item("a")));

            Assert.AreSame(c, panel.GetChild(0).gameObject, "keyed child must move, not rebuild");
            Assert.AreSame(a, panel.GetChild(2).gameObject);
            Assert.AreEqual("c", panel.GetChild(0).GetComponent<TextMeshProUGUI>().text);
        }

        // ── Pooling ──────────────────────────────────────────────────────────

        [Test]
        public void UnmountedImage_IsPooled_AndReturnsPristine()
        {
            var p1 = UguiBaseProps.__Rent<UguiImageProps>();
            p1.Color = Color.red;
            p1.RaycastTarget = true;
            _renderer.Render(U.Panel(null, null, U.Image(p1)));
            var firstImage = Child(0).transform.GetChild(0).gameObject;

            _renderer.Render(U.Panel(null));
            Assert.AreEqual(0, Child(0).transform.childCount);

            var p2 = UguiBaseProps.__Rent<UguiImageProps>();
            _renderer.Render(U.Panel(null, null, U.Image(p2)));
            var secondImage = Child(0).transform.GetChild(0).gameObject;

            Assert.AreSame(firstImage, secondImage, "stateless Image must be reused from the pool");
            var image = secondImage.GetComponent<Image>();
            Assert.AreEqual(Color.white, image.color, "pooled instance must return pristine");
            Assert.IsFalse(image.raycastTarget, "pooled instance must return pristine");
        }

        [Test]
        public void UnmountedButton_IsNotPooled()
        {
            _renderer.Render(U.Panel(null, null, U.Button(null)));
            var button = Child(0).transform.GetChild(0).gameObject;

            _renderer.Render(U.Panel(null));
            _renderer.Render(U.Panel(null, null, U.Button(null)));
            var second = Child(0).transform.GetChild(0).gameObject;

            Assert.AreNotSame(button, second, "stateful controls must never be pooled");
        }
    }
}
