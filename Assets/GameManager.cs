using UnityEngine;
using TMPro; // UIを操作するために必要

public class GameManager : MonoBehaviour
{
    [Header("--- UI設定 ---")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    [Header("--- ゲーム設定 ---")]
    public float timeLimit = 60f; // 1分間

    private int score = 0;
    private float currentTime;
    private bool isGamePlaying = true;

    void Start()
    {
        currentTime = timeLimit;
        UpdateUI();
    }

    void Update()
    {
        if (!isGamePlaying) return;

        // タイマーのカウントダウン
        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            EndGame();
        }

        UpdateUI();
    }

    // ヒットしたときにスコアを加算する関数（外部から呼ばれる）
    public void AddScore()
    {
        if (!isGamePlaying) return;
        score++;
        UpdateUI();
    }

    // 現在のゲーム状態（プレイ中かどうか）を返す関数
    public bool IsPlaying()
    {
        return isGamePlaying;
    }

    void UpdateUI()
    {
        scoreText.text = $"SCORE: {score}";
        timerText.text = $"TIME: {currentTime:F1}"; // 小数点第1位まで表示
    }

    void EndGame()
    {
        isGamePlaying = false;
        timerText.text = "TIME UP!";
        
        // 練習終了後にマウスカーソルを復活させる
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log($"【練習終了】 あなたのスコアは {score} 回ヒットでした！");
    }
}
