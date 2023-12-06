using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Tooltip : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _CanvasGroup;
        [SerializeField] private TMP_Text _Header;
        [SerializeField] private TMP_Text _Contents;
        [SerializeField] private List<Image> _Corners;
        [SerializeField] private HudButton _Button;
        
        private enum Corner { TopLeft, TopRight, BottomLeft, BottomRight }
        private RectTransform _rectTransform;
        private Tween _fader;
        
        void Awake()
        {
            _rectTransform = transform as RectTransform;

            _CanvasGroup.alpha = 0f;
            _fader = _CanvasGroup.DOFade(1f, .2f)
                .SetAutoKill(false).Pause();
        }
        
        public void Show(Canvas canvas, RectTransform anchor, string header, string contents, System.Action onClick = null)
        {
            _Header.text = header;
            if (onClick == null) _Contents.text = contents;
            else _Contents.text = contents + "\n\n";
            LayoutRebuilder.ForceRebuildLayoutImmediate(_Contents.rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
            
            UpdatePosition(canvas, anchor);
            _fader.Rewind();
            _fader.PlayForward();
            
            if (onClick != null)
            {
                _Button.gameObject.SetActive(true);
                _Button.SetListener(() =>
                {
                    onClick();
                    _Button.gameObject.SetActive(false);
                });
            }
        }
        
        private void UpdatePosition(Canvas canvas, RectTransform anchor)
        {
            var canvasTransform = canvas.transform as RectTransform;
            
            // Evaluate anchor or mouse positioning
            Vector2 canvasSize = canvasTransform.sizeDelta;
            Vector2 viewportPos;
            if (anchor == null)
            {
                viewportPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            }
            else
            {
                Vector2 anchorPos = canvasTransform.InverseTransformPoint(anchor.position);
                anchorPos += canvasSize / 2;
                viewportPos = new Vector2(anchorPos.x / canvasSize.x, anchorPos.y / canvasSize.y);
            }
            
            // Determine preferred alignment
            Vector2 offset;
            const int OFFSET_VAL = 43;
            if (viewportPos.x < .5f) 
            {
                if (viewportPos.y < .5f)
                {
                    _rectTransform.pivot = new Vector2(0, 0); // Bottom left
                    offset = new Vector2(OFFSET_VAL, OFFSET_VAL);
                    ShowCorner(Corner.BottomLeft);
                }
                else
                {
                    _rectTransform.pivot = new Vector2(0, 1); // Top left
                    offset = new Vector2(OFFSET_VAL, -OFFSET_VAL);
                    ShowCorner(Corner.TopLeft);
                }
            }
            else
            {
                if (viewportPos.y < .5f)
                {
                    _rectTransform.pivot = new Vector2(1, 0); // Bottom right
                    offset = new Vector2(-OFFSET_VAL, OFFSET_VAL);
                    ShowCorner(Corner.BottomRight);
                }
                else
                {
                    _rectTransform.pivot = new Vector2(1, 1); // Top right
                    offset = new Vector2(-OFFSET_VAL, -OFFSET_VAL);
                    ShowCorner(Corner.TopRight);
                }
            }
            
            // Set tooltip viewportPos
            var canvasPos = new Vector2(viewportPos.x * canvasSize.x, viewportPos.y * canvasSize.y);
            canvasPos -= canvasSize / 2;
            canvasPos += offset;
            _rectTransform.localPosition = canvasPos;
            
            // Stay inside the canvas (not needed with quadrant-based alignment)
            /*
            const float BUFFER = 30;
            Vector3 pos = tooltipTransform.localPosition;
            Vector3 minPosition = canvasTransform.rect.min - tooltipTransform.rect.min;
            Vector3 maxPosition = canvasTransform.rect.max - tooltipTransform.rect.max;
            pos.x = Mathf.Clamp(tooltipTransform.localPosition.x, minPosition.x + BUFFER, maxPosition.x - BUFFER);
            pos.y = Mathf.Clamp(tooltipTransform.localPosition.y, minPosition.y + BUFFER, maxPosition.y - BUFFER);
            tooltipTransform.localPosition = pos;
            */
        }
        
        private void ShowCorner(Corner value)
        {
            _Corners[0].gameObject.SetActive(value == Corner.TopLeft);
            _Corners[1].gameObject.SetActive(value == Corner.TopRight);
            _Corners[2].gameObject.SetActive(value == Corner.BottomLeft);
            _Corners[3].gameObject.SetActive(value == Corner.BottomRight);
        }
    }
}
