// LET IT BE KNOWN THAT I DID THIS WITHOUT WIFI
// Yes theres probably a better way to do it but this took me too long
// Yes i could use jagged arrays to fix the if statement spaghetti
// and i dont feel like fixing it. 
// If you want to, go ahead.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _rectTransform;

    [SerializeField] private List<GameObject> _draggable;
    [SerializeField] private List<GameObject> _left;
    [SerializeField] private List<GameObject> _right;
    [SerializeField] private List<GameObject> _top;
    [SerializeField] private List<GameObject> _bottom;
    [SerializeField] private List<GameObject> _topLeft;
    [SerializeField] private List<GameObject> _topRight;
    [SerializeField] private List<GameObject> _bottomLeft;
    [SerializeField] private List<GameObject> _bottomRight;

    private enum DragType
    {
        Drag,
        Left,
        Right,
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    private DragType _type;
    
    private bool _dragging;
    
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
        if (TryFindObjectType(eventData)) {
            _dragging = true;
        }
    }
    
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
        if (_dragging) {
            _dragging = false;
        }
    }
    
    
    void IDragHandler.OnDrag(PointerEventData eventData) {
        if (_dragging) {
            Vector2 scaledDelta = eventData.delta / _canvas.scaleFactor;

            Vector2 rectSizeDelta = _rectTransform.sizeDelta;
            Vector2 rectAnchoredPosition = _rectTransform.anchoredPosition;

            switch (_type) {
                case DragType.Drag:
                    _rectTransform.anchoredPosition += scaledDelta;
                    break;

                case DragType.Left:
                    _rectTransform.sizeDelta += new Vector2(-scaledDelta.x, 0);
                    _rectTransform.anchoredPosition += new Vector2(scaledDelta.x, 0) / 2;
                    break;

                case DragType.Right:
                    _rectTransform.sizeDelta += new Vector2(scaledDelta.x, 0);
                    _rectTransform.anchoredPosition += new Vector2(scaledDelta.x, 0) / 2;
                    break;

                case DragType.Top:
                    _rectTransform.sizeDelta += new Vector2(0, scaledDelta.y);
                    _rectTransform.anchoredPosition += new Vector2(0, scaledDelta.y) / 2;
                    break;

                case DragType.Bottom:
                    _rectTransform.sizeDelta += new Vector2(0, -scaledDelta.y);
                    _rectTransform.anchoredPosition += new Vector2(0, scaledDelta.y) / 2;
                    break;

                case DragType.TopLeft:
                    _rectTransform.sizeDelta += new Vector2(-scaledDelta.x, scaledDelta.y);
                    _rectTransform.anchoredPosition += new Vector2(scaledDelta.x, scaledDelta.y) / 2;
                    break;

                case DragType.TopRight:
                    _rectTransform.sizeDelta += new Vector2(scaledDelta.x, scaledDelta.y);
                    _rectTransform.anchoredPosition += new Vector2(scaledDelta.x, scaledDelta.y) / 2;
                    break;

                case DragType.BottomLeft:
                    _rectTransform.sizeDelta += new Vector2(-scaledDelta.x, -scaledDelta.y);
                    _rectTransform.anchoredPosition += new Vector2(scaledDelta.x, scaledDelta.y) / 2;
                    break;

                case DragType.BottomRight:
                    _rectTransform.sizeDelta += new Vector2(scaledDelta.x, -scaledDelta.y);
                    _rectTransform.anchoredPosition += new Vector2(scaledDelta.x, scaledDelta.y) / 2;
                    break;
            }

            // Clamping values
            if (_rectTransform.sizeDelta.x < -1845f || _rectTransform.sizeDelta.y < -950) {
                _rectTransform.sizeDelta = rectSizeDelta;
                _rectTransform.anchoredPosition = rectAnchoredPosition;
            }
        }
    }

    private bool TryFindObjectType(PointerEventData eventData) {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var t in results) {
            GameObject hitObject = t.gameObject;
            
            if (_draggable.Contains(hitObject)) {
                _type = DragType.Drag;
                return true;
            }
            if (_left.Contains(hitObject)) {
                _type = DragType.Left;
                return true;
            }
            if (_right.Contains(hitObject)) {
                _type = DragType.Right;
                return true;
            }
            if (_top.Contains(hitObject)) {
                _type = DragType.Top;
                return true;
            }
            if (_bottom.Contains(hitObject)) {
                _type = DragType.Bottom;
                return true;
            }
            if (_topLeft.Contains(hitObject)) {
                _type = DragType.TopLeft;
                return true;
            }
            if (_topRight.Contains(hitObject)) {
                _type = DragType.TopRight;
                return true;
            }
            if (_bottomLeft.Contains(hitObject)) {
                _type = DragType.BottomLeft;
                return true;
            }
            if (_bottomRight.Contains(hitObject)) {
                _type = DragType.BottomRight;
                return true;
            }
        }

        return false;
    }
}
