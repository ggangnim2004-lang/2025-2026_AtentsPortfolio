using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class BattleTransitionController : MonoBehaviour
{
    public static BattleTransitionController Instance { get; private set; }

    [Header("UI")]
    public CanvasGroup fadeCanvas; // 전체 화면 페이드용
    public Text battleText;        // 전투 시작 텍스트

    [Header("Timing")]
    public float textDuration = 0.8f;
    public float fadeDuration = 0.6f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 초기 상태 정리
        if (battleText != null)
        {
            battleText.gameObject.SetActive(false);
        }

        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 0f;
        }
    }

    public void StartBattle(string battleSceneName)
    {
        StartCoroutine(BattleSequence(battleSceneName));
    }

    private IEnumerator BattleSequence(string battleScene)
    {
        // 1) 전투 시작 텍스트
        battleText.gameObject.SetActive(true);
        battleText.text = "전투 시작";

        yield return new WaitForSeconds(textDuration);

        // 2) 화면 페이드 아웃
        yield return Fade(0f, 1f);

        // 3) 씬 전환
        SceneManager.LoadScene(battleScene);
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        fadeCanvas.alpha = from;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = to;
    }
}
