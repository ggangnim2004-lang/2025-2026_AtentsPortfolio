using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Refs")]
    public InventoryGridUI gridUI;
    public InventoryModel model;
    public Canvas rootCanvas;
    public Image image;

    [Header("Shape (cells relative to anchor)")]
    public Vector2Int[] shapeOffsets = new Vector2Int[]
    {
        new Vector2Int(0,0),
        new Vector2Int(1,0),
        new Vector2Int(0,1),
    };

    [Header("Current Anchor")]
    public Vector2Int anchorGridPos = new Vector2Int(0, 0);

    private RectTransform rt;

    // 원복용
    private Transform originalParent;
    private Vector2 originalAnchoredPos;
    private Vector2Int originalAnchor;

    // 드래그 오프셋(마우스 찍은 지점 유지)
    private Vector2 dragOffsetLocal;
    private bool dragging;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (image == null) image = GetComponent<Image>();
    }

    private void Start()
    {
        // 초기 배치
        if (gridUI != null && model != null)
        {
            // gridRoot 기준(0,1)로 맞춤
            rt.SetParent(gridUI.gridRoot, false);
            SetRectForGrid();

            // 초기 위치 스냅 + 모델 배치
            SnapToGrid(anchorGridPos);
            model.TryPlace(this, anchorGridPos, shapeOffsets);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (gridUI == null || model == null) return;

        dragging = true;

        // 원복 정보 저장
        originalParent = rt.parent;
        originalAnchoredPos = rt.anchoredPosition;
        originalAnchor = anchorGridPos;

        // 드래그 시작 시 모델 점유 해제(이 아이템만)
        model.Clear(this);

        // 현재 부모 기준 로컬 마우스 위치 구해서 오프셋 계산
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)rt.parent, eventData.position, eventData.pressEventCamera, out Vector2 mouseLocalInParent);

        dragOffsetLocal = rt.anchoredPosition - mouseLocalInParent;

        // 위로 올리기
        rt.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)rt.parent, eventData.position, eventData.pressEventCamera, out Vector2 mouseLocalInParent);

        rt.anchoredPosition = mouseLocalInParent + dragOffsetLocal;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;

        bool placed = false;

        // 드롭 위치를 grid 좌표로 변환
        if (gridUI.ScreenToGrid(eventData.position, eventData.pressEventCamera, out Vector2Int dropGridPos))
        {
            // 배치 가능하면 스냅 + 모델 배치
            if (model.TryPlace(this, dropGridPos, shapeOffsets))
            {
                anchorGridPos = dropGridPos;
                SnapToGrid(anchorGridPos);
                placed = true;
            }
        }

        // 실패하면 원위치 복구 + 모델도 원복 배치
        if (!placed)
        {
            rt.SetParent(originalParent, false);
            anchorGridPos = originalAnchor;
            rt.anchoredPosition = originalAnchoredPos;

            model.TryPlace(this, anchorGridPos, shapeOffsets);
        }
    }

    private void SetRectForGrid()
    {
        // gridRoot가 (0,1) pivot이라고 가정하므로 아이템도 동일하게 맞춤
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);

        // 아이템의 픽셀 크기 재계산
        rt.sizeDelta = CalcBoundingSizePixels();
    }

    private void SnapToGrid(Vector2Int gridPos)
    {
        // 혹시 누가 설정 바꿨을 수 있으니 매번 보정
        SetRectForGrid();

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
}
