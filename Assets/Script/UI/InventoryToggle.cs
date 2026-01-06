using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryToggle : MonoBehaviour
{

    public RectTransform inventoryPanel;
    public KeyCode toggleKey = KeyCode.Tab;

    [Tooltip("전투 씬에서 항상 켜두기: true")]
    public bool forceOpenInBattle = true;

    public InventoryLayoutController layout;

    private void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.gameObject.SetActive(false);
        }
    }

    private void Update()
    {

        if (Input.GetKeyDown(toggleKey))
        {
            bool newState = !inventoryPanel.gameObject.activeSelf;
            inventoryPanel.gameObject.SetActive(newState);

            if (newState && layout != null)
            {
                // 켜질 때 현재 모드 기준으로 다시 Apply
                // layout 쪽 DetectMode를 public으로 다시 빼거나, 간단히 Apply(Stage/Battle) 직접 호출
            }
        }
    }

}
