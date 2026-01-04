using UnityEngine;

public class InventoryModel : MonoBehaviour
{
    
    public int width = 8;
    public int height = 8;

    private InventoryItemUI[,] occupied;

    private void Awake()
    {
        occupied = new InventoryItemUI[width, height];
    }

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

    public void Place(InventoryItemUI item, Vector2Int anchor, Vector2Int[] shapeOffsets)
    {
        Clear(item);

        for (int i = 0; i < shapeOffsets.Length; i++)
        {
            int x = anchor.x + shapeOffsets[i].x;
            int y = anchor.y + shapeOffsets[i].y;
            occupied[x, y] = item;
        }
    }

    public void Clear(InventoryItemUI item)
    {
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (occupied[x, y] == item)
                    occupied[x, y] = null;
    }
    
}
