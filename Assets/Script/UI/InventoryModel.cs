using UnityEngine;

public class InventoryModel : MonoBehaviour
{
    [Header("Grid Size")]
    public int width = 6;
    public int height = 4;

    private InventoryItemUI[,] occupied;

    private void Awake()
    {
        occupied = new InventoryItemUI[width, height];
    }

    /// <summary> 배치 가능 여부(범위 + 겹침) </summary>
    public bool CanPlace(InventoryItemUI item, Vector2Int anchor, Vector2Int[] shapeOffsets)
    {
        for (int i = 0; i < shapeOffsets.Length; i++)
        {
            int x = anchor.x + shapeOffsets[i].x;
            int y = anchor.y + shapeOffsets[i].y;

            if (x < 0 || x >= width || y < 0 || y >= height)
                return false;

            var occ = occupied[x, y];
            if (occ != null && occ != item)
                return false;
        }
        return true;
    }

    /// <summary>
    /// 덮어쓰기 방지 배치. 실패하면 아무 것도 바꾸지 않음.
    /// </summary>
    public bool TryPlace(InventoryItemUI item, Vector2Int anchor, Vector2Int[] shapeOffsets)
    {
        if (!CanPlace(item, anchor, shapeOffsets))
            return false;

        Clear(item);

        for (int i = 0; i < shapeOffsets.Length; i++)
        {
            int x = anchor.x + shapeOffsets[i].x;
            int y = anchor.y + shapeOffsets[i].y;
            occupied[x, y] = item;
        }
        return true;
    }

    /// <summary> 이 아이템이 점유한 모든 셀 해제 </summary>
    public void Clear(InventoryItemUI item)
    {
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (occupied[x, y] == item)
                    occupied[x, y] = null;
    }

    // (디버깅용) 특정 셀 점유자 확인
    public InventoryItemUI GetOccupant(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return null;
        return occupied[x, y];
    }
}
