using UnityEngine;
using UnityEngine.UI;

public class InventoryTestItemSpawner : MonoBehaviour
{
    [Header("References")]
    public InventoryGridUI gridUI;
    public InventoryModel model;
    public Canvas inventoryCanvas;
    public RectTransform gridRoot;

    [Header("Test Item Visual")]
    public Sprite testSprite;
    public Vector2Int startGridPos = new Vector2Int(0, 0);

    [Header("Auto Layout")]
    public int spacingInGrid = 2; // 아이템 간격(셀 단위)

    private void Start()
    {
        SpawnTestItems();
    }

    void SpawnTestItems()
    {
        if (gridUI == null || model == null || inventoryCanvas == null || gridRoot == null)
        {
            Debug.LogError("[InventoryTestItemSpawner] Reference missing");
            return;
        }

        Vector2Int cursor = startGridPos;

        // 테스트용 아이템 모양들
        Vector2Int[][] testShapes = new Vector2Int[][]
        {
            // 1x1
            new Vector2Int[] { new Vector2Int(0,0) },

            // 1x2
            new Vector2Int[] { new Vector2Int(0,0), new Vector2Int(0,1) },

            // L자
            new Vector2Int[] {
                new Vector2Int(0,0),
                new Vector2Int(1,0),
                new Vector2Int(0,1)
            },

            // 2x2
            new Vector2Int[] {
                new Vector2Int(0,0),
                new Vector2Int(1,0),
                new Vector2Int(0,1),
                new Vector2Int(1,1)
            }
        };

        foreach (var shape in testShapes)
        {
            CreateOneTestItem(shape, cursor);

            cursor.x += spacingInGrid;
            if (cursor.x >= gridUI.Width)
            {
                cursor.x = startGridPos.x;
                cursor.y += spacingInGrid;
            }
        }
    }

    void CreateOneTestItem(Vector2Int[] shapeOffsets, Vector2Int gridPos)
    {
        GameObject go = new GameObject("TestItem", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(gridRoot, false);

        Image img = go.GetComponent<Image>();
        img.sprite = testSprite;
        img.color = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.8f, 1f);
        img.raycastTarget = true;

        InventoryItemUI item = go.AddComponent<InventoryItemUI>();
        item.gridUI = gridUI;
        item.model = model;
        item.rootCanvas = inventoryCanvas;
        item.image = img;

        item.shapeOffsets = shapeOffsets;
        item.anchorGridPos = gridPos;
    }
}
