using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameViewMode { stage, Battle }

public class InventoryLayoutController : MonoBehaviour
{

    [Header("References")]
    public RectTransform inventoryPanel;

    [Header("Scene Name Contains (simple heuristic)")]
    public string battleSceneKeyword = "Battle"; // 씬 이름에 Battle이 포함되면 전투로 판단

    [Header("Stage Layout (Center)")]
    public Vector2 stageAnchorMin   = new Vector2(0.5f, 0.5f);
    public Vector2 stageAnchorMax   = new Vector2(0.5f, 0.5f);
    public Vector2 stagePivot       = new Vector2(0.5f, 0.5f);
    public Vector2 stagePos         = Vector2.zero;

    [Header("Battle Layout (Top)")]
    public Vector2 battleAnchorMin  = new Vector2(0.5f, 1.0f);
    public Vector2 battleAnchorMax  = new Vector2(0.5f, 1.0f);
    public Vector2 battlePivot      = new Vector2(0.5f, 1.0f);
    public Vector2 battlePos        = new Vector2(0.0f, -20.0f);

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        Apply(DetectMode(SceneManager.GetActiveScene().name));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Apply(DetectMode(scene.name));
    }

    private GameViewMode DetectMode(string sceneName)
    {
        // 씬 앞에 Battle이 들어가면 전투로 취급
        return sceneName.Contains(battleSceneKeyword) ? GameViewMode.Battle : GameViewMode.stage;
    }

    public void Apply (GameViewMode mode)
    {
        Debug.Log($"Apply Inventory Layout: {mode}, pos = {inventoryPanel.anchoredPosition}, anchorMin={inventoryPanel.anchorMin}");

        if (inventoryPanel == null) return;

        if (mode == GameViewMode.stage)
        {
            inventoryPanel.anchorMin = stageAnchorMin;
            inventoryPanel.anchorMax = stageAnchorMax;
            inventoryPanel.pivot = stagePivot;
            inventoryPanel.anchoredPosition = stagePos;

            inventoryPanel.localScale = Vector3.one;
            inventoryPanel.localRotation = Quaternion.identity;

            inventoryPanel.offsetMin = Vector2.zero;
            inventoryPanel.offsetMax = Vector2.zero;
        }
        else
        {
            inventoryPanel.anchorMin = battleAnchorMin;
            inventoryPanel.anchorMax = battleAnchorMax;
            inventoryPanel.pivot = battlePivot;
            inventoryPanel.anchoredPosition = battlePos;

            inventoryPanel.localScale = Vector3.one;
            inventoryPanel.localRotation = Quaternion.identity;

            inventoryPanel.offsetMin = Vector2.zero;
            inventoryPanel.offsetMax = Vector2.zero;
        }
    }

}
