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

        Vector2Int seedPos;
        bool gotSeed = gridUI.ScreenToGridLoose(eventData.position, eventData.pressEventCamera, out seedPos);

        bool placed = false;

        if (gotSeed)
        {
            // 반경 3 안에서 가장 가까운 유효 배치 찾기
            if (TryFindNearestValid(seedPos, 3, out Vector2Int bestPos))
            {
                rt.SetParent(originalParent, false);
                anchorGridPos = bestPos;
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

    private bool TryFindNearestValid(Vector2Int seed, int maxRadius, out Vector2Int bestPos)
    {
        bestPos = seed;
        bool found = false;
        int bestDistSq = int.MaxValue;

        // 반경 0부터 점점 확장
        for (int r = 0; r <= maxRadius; r++)
        {
            // r 테두리만
            for (int dx = -r; dx <= r; dx++)
            {
                int dy1 = r;
                int dy2 = -r;

                // (dx, +r)
                Vector2Int p1 = new Vector2Int(seed.x + dx, seed.y + dy1);
                if (CheckCandidate(p1, seed, ref found, ref bestDistSq, ref bestPos)) { }

                // (dx, -r) (r = 0이면 중복이라 건너뛴다)
                if (r != 0)
                {
                    Vector2Int p2 = new Vector2Int(seed.x + dx, seed.y + dy2);
                    if (CheckCandidate(p2, seed, ref found, ref bestDistSq, ref bestPos)) { }
                }
            }

            for (int dy = - r + 1; dy <= r - 1; dy++)
            {
                int dx1 = r;
                int dx2 = -r;

                Vector2Int p1 = new Vector2Int(seed.x + dx1, seed.y + dy);
                if (CheckCandidate(p1, seed, ref found, ref bestDistSq, ref bestPos)) { }

                if (r != 0)
                {
                    Vector2Int p2 = new Vector2Int(seed.x + dx2, seed.y + dy);
                    if (CheckCandidate(p2, seed, ref found, ref bestDistSq,ref bestPos)) { }
                }
            }

            if (found) return true;
        }

        return false;
    }

    private bool CheckCandidate(Vector2Int candidate, Vector2Int seed, ref bool found, ref int bestDistSq, ref Vector2Int bestPos)
    {
        // grid 범위 체크
        if (candidate.x < 0 || candidate.x >= gridUI.width || candidate.y < 0 || candidate.y >= gridUI.height)
            return false;

        // 배치 가능 체크
        if (!model.CanPlace(this, candidate, shapeOffsets))
            return false;

        int dx = candidate.x - seed.x;
        int dy = candidate.y - seed.y;
        int distSq = dx * dx + dy * dy;

        if (distSq < bestDistSq)
        {
            bestDistSq = distSq;
            bestPos = candidate;
            found = true;
        }
        return true;
    }

}
