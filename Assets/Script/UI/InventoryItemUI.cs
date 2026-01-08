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

    private Vector2 dragOffsetLocal; // 클릭 지점 오프셋
    private Color baseColor; // 아이템 기본 색
    private bool baseColorCached = false; 

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

            // 생성 직후 색을 기본 색으로 캐시 
            if (!baseColorCached && image != null)
            {
                baseColor = image.color;
                baseColorCached = true;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (gridUI == null || model == null) return;

        dragging = true;

        // 원위치 복구용 값 저장
        originalParent = rt.parent;
        originalAnchoredPos = rt.anchoredPosition;
        originalAnchor = anchorGridPos;

        // 1) baseColor 저장
        if (!baseColorCached)
        {
            baseColor = image != null ? image.color : Color.white;
            baseColorCached = true;
        }

        // 2) 현재 위치 점유 해제
        model.Clear(this);

        // 3) 드래그 오프셋 계산
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)rt.parent, eventData.position, eventData.pressEventCamera, out Vector2 mouseLocalInParent);

        // anchoredPosition - 마우스 위치 = 오프셋
        dragOffsetLocal = rt.anchoredPosition - mouseLocalInParent;

        // 4) 드래그 중에는 최상단으로
        rt.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)rt.parent, eventData.position, eventData.pressEventCamera, out Vector2 mouseLocalInParent);

        rt.anchoredPosition = mouseLocalInParent + dragOffsetLocal;
        
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

        if (image != null)
        {
            image.color = baseColor;
        }
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
        image.color = canPlace ? baseColor : new Color(1f, 0.5f, 0.5f, 1f);
    }
}
