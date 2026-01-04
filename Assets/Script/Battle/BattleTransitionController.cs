using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;


public class BattleTransitionController : MonoBehaviour
{
    public static BattleTransitionController Instance { get; private set; }

    [Header("UI")]
    public RectTransform curtain; // 전체 화면 페이드용
    public Text battleText;        // 전투 시작 텍스트

    [Header("Timing")]
    public float textDuration = 0.8f;
    public float curtainDownDuration = 0.35f;
    public float curtainUpDuration = 0.35f;

    private Vector2 curtainHiddenPos; // 화면 위로 숨긴 위치
    private Vector2 curtainShownPos; // 화면 중앙(덮는 위치)
    private bool isBusy = false;


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

        CacheCurtainPositions();
        SetCurtainHiddenImmediate();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void CacheCurtainPositions()
    {
        if (curtainHiddenPos == null) return;

        // Screen Space OverLay에서, 캔버스 기준으로 움직임
        // "숨김" 위치는 화면 위로 완전히 올려놓기
        float h = ((RectTransform)curtain.parent).rect.height;
        curtainShownPos = Vector2.zero;        // 중앙 (덮는 상태)
        curtainHiddenPos = new Vector2(0f, h); // 위로 숨김
    }

    private void SetCurtainHiddenImmediate()
    {
        if (curtain == null) return;
        curtain.anchoredPosition = curtainHiddenPos;
    }

    public void StartBattle (string battleSceneName)
    {
        if (isBusy) return;
        StartCoroutine(BattleSequence(battleSceneName));
    }

    private IEnumerator BattleSequence(string battleScene)
    {
        isBusy = true;

        // 1) 전투 시작 텍스트
        if (battleText != null)
        {
            battleText.gameObject.SetActive(true);
            battleText.text = "전투 시작";
        }
        yield return new WaitForSeconds(textDuration);

        // 2) 전투 화면 전환 (화면 덮기)
        yield return MoveCurtain(curtainHiddenPos, curtainShownPos, curtainDownDuration);

        // 3) 씬 로드 (덮인 상태)
        SceneManager.LoadScene(battleScene);

        // 씬 로드 후 화면 전환 스크린을 올리는 건 OnSceneLoaded에서 처리한다
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 4) 씬에 들어가면 텍스트를 끄고 화면 전환 스크린을 올려서 전투 화면 열기
        StartCoroutine(AfterLoadOpen());
    }

    private IEnumerator AfterLoadOpen()
    {
        if (battleText != null)
        {
            battleText.gameObject.SetActive(false);
        }

        // 위치 재계산
        CacheCurtainPositions();

        // 로딩 타이밍 이슈 대비
        if (curtain != null) curtain.anchoredPosition = curtainShownPos;

        // 5) 화면 전환 스크린을 올려서 전투 화면 공개
        yield return MoveCurtain(curtainShownPos, curtainHiddenPos, curtainUpDuration);

        isBusy = false;
    }

    private IEnumerator MoveCurtain(Vector2 from, Vector2 to, float duration)
    {
        if (curtain == null) yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);
            curtain.anchoredPosition = Vector2.Lerp(from, to, a);
            yield return null;
        }

        curtain.anchoredPosition = to;
    }

}
