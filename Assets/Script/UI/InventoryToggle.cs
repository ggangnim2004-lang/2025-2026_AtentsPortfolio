using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryToggle : MonoBehaviour
{

    public RectTransform inventoryPanel;
    public KeyCode toggleKey = KeyCode.Tab;

    [Tooltip("전투 씬에서 항상 켜두기: true")]
    public bool forceOpenInBattle = true;

    private void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (inventoryPanel == null) return;

        if (Input.GetKeyDown(toggleKey))
        {
            inventoryPanel.gameObject.SetActive(!inventoryPanel.gameObject.activeSelf);
        }
    }

}
