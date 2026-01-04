using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public InventoryGridUI gridUI;
    public InventoryModel model;
    public Canvas rootCanvas;
    public Image image;

    public Vector2Int[] shapeOffsets = new Vector2Int[]
    {
        new Vector2Int(0,0),
        new Vector2Int(1,0),
        new Vector2Int(0,1),
    };

    public Vector2Int anchorGridPos = new Vector2Int(0, 0);

    private RectTransform rt;
    private Transform originalParent;
    private Vector2 originalAnchoredPos;
    private Vector2Int originalAnchor;
    private bool dragging;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (image == null) image = GetComponent<Image>();
    }

    private void Start()
    {
        if (gridUI != null && model != null)
        {
            SnapToGrid(anchorGridPos);
            model.Place(this, anchorGridPos, shapeOffsets);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (gridUI == null || model == null || rootCanvas == null) return;

        dragging = true;
        originalParent = rt.parent;
        originalAnchoredPos = rt.anchoredPosition;
        originalAnchor = anchorGridPos;

        model.Clear(this);
        rt.SetParent(rootCanvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;

        rt.position = eventData.position;

        if (gridUI.ScreenToGrid(eventData.position, eventData.pressEventCamera, out Vector2Int gridPos))
            SetPreviewColor(model.CanPlace(this, gridPos, shapeOffsets));
        else
            SetPreviewColor(false);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;

        bool placed = false;

        if (gridUI.ScreenToGrid(eventData.position, eventData.pressEventCamera, out Vector2Int gridPos))
        {
            if (model.CanPlace(this, gridPos, shapeOffsets))
            {
                rt.SetParent(originalParent, false);
                anchorGridPos = gridPos;
                SnapToGrid(anchorGridPos);
                model.Place(this, anchorGridPos, shapeOffsets);
                placed = true;
            }
        }

        if (!placed)
        {
            rt.SetParent(originalParent, false);
            anchorGridPos = originalAnchor;
            rt.anchoredPosition = originalAnchoredPos;
            model.Place(this, anchorGridPos, shapeOffsets);
        }

        SetPreviewColor(true);
    }

    private void SnapToGrid(Vector2Int gridPos)
    {
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);

        rt.sizeDelta = CalcBoundingSizePixels();
        rt.anchoredPosition = gridUI.GridToAnchoredPos(gridPos);
    }

    private Vector2 CalcBoundingSizePixels()
    {
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        for (int i = 0; i < shapeOffsets.Length; i++)
        {
            minX = Mathf.Min(minX, shapeOffsets[i].x);
            minY = Mathf.Min(minY, shapeOffsets[i].y);
            maxX = Mathf.Max(maxX, shapeOffsets[i].x);
            maxY = Mathf.Max(maxY, shapeOffsets[i].y);
        }

        int cellsW = (maxX - minX) + 1;
        int cellsH = (maxY - minY) + 1;

        Vector2 cell = gridUI.GetCellSize();
        Vector2 spacing = gridUI.GetCellSpacing();

        float w = cellsW * cell.x + (cellsW - 1) * spacing.x;
        float h = cellsH * cell.y + (cellsH - 1) * spacing.y;
        return new Vector2(w, h);
    }

    private void SetPreviewColor(bool canPlace)
    {
        if (image == null) return;
        image.color = canPlace ? Color.white : new Color(1f, 0.5f, 0.5f, 1f);
    }
}
