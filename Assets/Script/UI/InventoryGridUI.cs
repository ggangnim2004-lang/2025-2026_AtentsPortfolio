using UnityEngine;

public class InventoryGridUI : MonoBehaviour
{
    [Header("References")]
    public RectTransform gridRoot;      // 그리드 기준 RectTransform
    public InventoryModel model;        // 반드시 연결

    [Header("Cell Settings")]
    public Vector2 cellSize = new Vector2(64, 64);
    public Vector2 cellSpacing = new Vector2(4, 4);

    public int Width => model != null ? model.width : 0;
    public int Height => model != null ? model.height : 0;

    /// <summary>
    /// Grid 좌표(좌상단이 0,0)를 gridRoot의 anchoredPosition으로 변환
    /// </summary>
    public Vector2 GridToAnchoredPos(Vector2Int gridPos)
    {
        // 좌상단 pivot(0,1) 기준
        float x = gridPos.x * (cellSize.x + cellSpacing.x);
        float y = -gridPos.y * (cellSize.y + cellSpacing.y);
        return new Vector2(x, y);
    }

    /// <summary>
    /// 스크린 좌표 → Grid 좌표. 그리드 밖이면 false.
    /// </summary>
    public bool ScreenToGrid(Vector2 screenPos, Camera uiCam, out Vector2Int gridPos)
    {
        gridPos = default;

        if (gridRoot == null || model == null) return false;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gridRoot, screenPos, uiCam, out Vector2 local))
            return false;

        // gridRoot가 pivot(0,1)이라고 가정: local (0,0)이 좌상단.
        float x = local.x;
        float y = -local.y;

        if (x < 0 || y < 0) return false;

        int gx = Mathf.FloorToInt(x / (cellSize.x + cellSpacing.x));
        int gy = Mathf.FloorToInt(y / (cellSize.y + cellSpacing.y));

        if (gx < 0 || gx >= model.width || gy < 0 || gy >= model.height)
            return false;

        gridPos = new Vector2Int(gx, gy);
        return true;
    }

    public Vector2 GetCellSize() => cellSize;
    public Vector2 GetCellSpacing() => cellSpacing;
}
