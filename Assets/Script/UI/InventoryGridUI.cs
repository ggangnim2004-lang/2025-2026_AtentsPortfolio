using UnityEngine;
using UnityEngine.UI;

public class InventoryGridUI : MonoBehaviour
{
    public int width = 8;
    public int height = 8;

    public Vector2 cellSize = new Vector2(64, 64);
    public Vector2 cellSpacing = new Vector2(4, 4);
    public Color cellColor = new Color(1, 1, 1, 0.08f);

    public RectTransform rectTransform;

    private void Awake()
    {
        if (rectTransform == null) rectTransform = (RectTransform)transform;
        BuildGridVisual();
    }

    private void BuildGridVisual()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var go = new GameObject($"Cell_{x}_{y}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);

                var rt = (RectTransform)go.transform;
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.sizeDelta = cellSize;

                float px = x * (cellSize.x + cellSpacing.x);
                float py = -y * (cellSize.y + cellSpacing.y);
                rt.anchoredPosition = new Vector2(px, py);

                var img = go.GetComponent<Image>();
                img.color = cellColor;
                img.raycastTarget = false;
            }

        float totalW = width * cellSize.x + (width - 1) * cellSpacing.x;
        float totalH = height * cellSize.y + (height - 1) * cellSpacing.y;
        rectTransform.sizeDelta = new Vector2(totalW, totalH);
    }

    public bool ScreenToGrid(Vector2 screenPos, Camera uiCamera, out Vector2Int gridPos)
    {
        gridPos = new Vector2Int(-1, -1);

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle
            (rectTransform, screenPos, uiCamera, out Vector2 local))
            return false;

        // pivot/anchor와 무관하게 rectTransform 기준으로 0 ~ width, 0 ~ height 좌표로 변환
        Rect r = rectTransform.rect;

        // local은 pivot 기준 좌표라서 rect 범위로 재매핑
        float x = local.x - r.xMin;         // 좌측이 0
        float y = r.yMax - local.y;         // 상단이 0 (UI는 위가 +가 아니라서 이렇게 뒤집음)

        float stepX = cellSize.x + cellSpacing.x;
        float stepY = cellSize.y + cellSpacing.y;

        int gx = Mathf.FloorToInt(x / stepX);
        int gy = Mathf.FloorToInt(y / stepY);

        if (gx < 0 || gx >= width || gy < 0 || gy >= height)
            return false;

        // 셀 내부인지(spacing 제외)
        float inCellX = x - gx * stepX;
        float incellY = y - gy * stepY;

        if (inCellX > cellSize.x || incellY > cellSize.y)
            return false;

        gridPos = new Vector2Int(gx, gy);
        return true;

    }

    public Vector2 GridToAnchoredPos(Vector2Int gridPos)
    {
        float stepX = cellSize.x + cellSpacing.x;
        float stepY = cellSize.y + cellSpacing.y;
        return new Vector2(gridPos.x * stepX, -gridPos.y * stepY);
    }

    public Vector2 GetCellSize() => cellSize;
    public Vector2 GetCellSpacing() => cellSpacing;

}
