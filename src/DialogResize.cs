using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Jundroo.Juicy.Widgets;

namespace SP2VariableDialogSize
{
    public enum GripAxis
    {
        Horizontal,
        Vertical,
        Both
    }

    /// <summary>
    /// 给变量对话框加“拖动边角/边缘改变大小”的能力。
    ///
    /// 对话框本体是 LayoutWidget（id="panel"，sizeFitter=Vertical，高度由内容撑开）：
    ///   - 宽度：直接设置 panel.Width（写入 LayoutElement 的 preferredWidth 并同步 RectTransform）；
    ///   - 高度：设置的其实是内部 ScrollViewWidget 的高度，panel 会随内容自动长高/变矮。
    /// 拖动时保持左上角不动，并把结果写回配置，下次打开沿用。
    /// </summary>
    public static class DialogResize
    {
        private const float MinWidth = 520f;
        private const float MinHeight = 120f;

        public static void Attach(Widget panelWidget)
        {
            if (panelWidget == null)
            {
                return;
            }

            LayoutWidget panel = panelWidget as LayoutWidget;
            if (panel == null)
            {
                Plugin.Log.LogWarning("对话框面板不是 LayoutWidget（" + panelWidget.GetType().Name + "），跳过拖动缩放。");
                return;
            }

            if (panel.transform.Find("sp2-resize-corner") != null)
            {
                return;
            }

            ScrollViewWidget scroll = panel.GetComponentInChildren<ScrollViewWidget>(true);
            if (scroll == null)
            {
                Plugin.Log.LogWarning("对话框里没找到 ScrollViewWidget，无法启用拖动缩放。");
                return;
            }

            ApplyConfiguredSize(panel, scroll);

            RectTransform rect = panel.Rect;

            // 右下角：同时改宽高
            CreateGrip(rect, panel, scroll, "sp2-resize-corner", GripAxis.Both,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(16f, 16f), new Vector2(-1f, 1f), 0.30f);

            // 右边缘：只改宽度
            CreateGrip(rect, panel, scroll, "sp2-resize-right", GripAxis.Horizontal,
                new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f),
                new Vector2(6f, -20f), new Vector2(0f, 0f), 0.14f);

            // 下边缘：只改高度
            CreateGrip(rect, panel, scroll, "sp2-resize-bottom", GripAxis.Vertical,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-20f, 6f), new Vector2(0f, 0f), 0.14f);
        }

        private static void ApplyConfiguredSize(LayoutWidget panel, ScrollViewWidget scroll)
        {
            if (Plugin.Width.Value > 0)
            {
                panel.Width = Plugin.Width.Value;
            }

            if (Plugin.Height.Value > 0)
            {
                scroll.Height = Plugin.Height.Value;
            }

            if (panel.gameObject.activeInHierarchy)
            {
                LayoutRebuilder.MarkLayoutForRebuild(panel.Rect);
            }
        }

        private static void CreateGrip(
            RectTransform parent,
            LayoutWidget panel,
            ScrollViewWidget scroll,
            string name,
            GripAxis axis,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 sizeDelta,
            Vector2 anchoredPosition,
            float alpha)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(DialogResizeGrip));
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPosition;

            Image image = go.GetComponent<Image>();
            image.raycastTarget = true;
            image.color = new Color(1f, 1f, 1f, alpha);

            // 不要让这个附加元素参与 VerticalLayout 的排版。
            LayoutElement layoutElement = go.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            go.GetComponent<DialogResizeGrip>().Initialize(axis, panel, scroll, image);
        }

        internal static float ClampWidth(float width, RectTransform parent)
        {
            float max = parent != null ? parent.rect.width - 16f : 3200f;
            return Mathf.Clamp(width, MinWidth, Mathf.Max(MinWidth, max));
        }

        internal static float ClampHeight(float height, RectTransform parent)
        {
            float max = parent != null ? parent.rect.height - 140f : 2000f;
            return Mathf.Clamp(height, MinHeight, Mathf.Max(MinHeight, max));
        }
    }

    /// <summary>
    /// 挂在对话框边缘的拖动条上。拖动时改 panel 宽度、ScrollView 高度。
    /// </summary>
    public class DialogResizeGrip : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        private GripAxis _axis;
        private LayoutWidget _panel;
        private ScrollViewWidget _scroll;
        private Image _image;
        private Color _normalColor;
        private bool _dragging;

        internal void Initialize(GripAxis axis, LayoutWidget panel, ScrollViewWidget scroll, Image image)
        {
            _axis = axis;
            _panel = panel;
            _scroll = scroll;
            _image = image;
            _normalColor = image != null ? image.color : Color.white;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_image != null)
            {
                _image.color = new Color(0.45f, 0.8f, 1f, 0.7f);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_image != null && !_dragging)
            {
                _image.color = _normalColor;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _dragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_panel == null)
            {
                return;
            }

            if (_scroll == null)
            {
                _scroll = _panel.GetComponentInChildren<ScrollViewWidget>(true);
                if (_scroll == null)
                {
                    return;
                }
            }

            RectTransform panelRect = _panel.Rect;
            if (panelRect == null)
            {
                return;
            }

            Canvas canvas = _panel.GetComponentInParent<Canvas>();
            float scale = canvas != null && canvas.scaleFactor > 0.001f ? canvas.scaleFactor : 1f;

            float dx = eventData.delta.x / scale;
            float dy = eventData.delta.y / scale;

            RectTransform parentRect = panelRect.parent as RectTransform;

            if (_axis != GripAxis.Vertical)
            {
                float oldWidth = panelRect.rect.width;
                float newWidth = DialogResize.ClampWidth(oldWidth + dx, parentRect);
                if (Mathf.Abs(newWidth - oldWidth) > 0.01f)
                {
                    _panel.Width = newWidth;

                    // 保持左边缘不动。
                    Vector2 position = panelRect.anchoredPosition;
                    position.x += (newWidth - oldWidth) * panelRect.pivot.x;
                    panelRect.anchoredPosition = position;
                }
            }

            if (_axis != GripAxis.Horizontal)
            {
                float oldHeight = _scroll.Rect.rect.height;
                float newHeight = DialogResize.ClampHeight(oldHeight - dy, parentRect);
                if (Mathf.Abs(newHeight - oldHeight) > 0.01f)
                {
                    _scroll.Height = newHeight;

                    // 保持上边缘不动（panel 高度随 ScrollView 高度同步变化）。
                    Vector2 position = panelRect.anchoredPosition;
                    position.y -= (newHeight - oldHeight) * (1f - panelRect.pivot.y);
                    panelRect.anchoredPosition = position;
                }
            }

            LayoutRebuilder.MarkLayoutForRebuild(panelRect);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _dragging = false;
            if (_image != null)
            {
                _image.color = _normalColor;
            }

            if (_panel == null || _scroll == null)
            {
                return;
            }

            int width = Mathf.RoundToInt(_panel.Rect.rect.width);
            int height = Mathf.RoundToInt(_scroll.Height ?? 0f);
            Plugin.SaveSize(width, height);
        }
    }
}
