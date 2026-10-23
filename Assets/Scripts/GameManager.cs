using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("--- 既存のUIオブジェクト設定 ---")]
    public TextMeshProUGUI timerText;   
    public TextMeshProUGUI scoreText;   
    public TextMeshProUGUI countdownText; 

    [Header("--- ゲームルール設定 ---")]
    public float timeRemaining = 60f;   
    private int score = 0;              
    private bool isGameActive = false; 

    // カウントダウン用のタイマー
    private float countdownTimer = 3.5f; 
    private bool isCountingDown = true;

    [Header("--- サイバーパンク光演出設定 ---")]
    // ★制御したい発光マテリアル（敵のネオンマテリアルなど）をここに登録します
    public Material neonMaterial; 
    public Material secondNeonMaterial; // ★【追加】新しく作った2つ目のマテリアルを入れる枠

    // パレットのベース色を保持する変数
    private Color baseColor;
    private Color secondBaseColor;      // ★【追加】2つ目のマテリアルのベース色を保持する変数

    public bool IsPlaying() => isGameActive; 

    void Start()
    {
        isCountingDown = true;
        isGameActive = false;
        Time.timeScale = 1f;

        // マテリアルから現在の基本の色（Emission Color）を取得しておく
        if (neonMaterial != null)
        {
            baseColor = neonMaterial.GetColor("_EmissionColor");
        }
        // ★【追加】2つ目のマテリアルの初期化
        if (secondNeonMaterial != null)
        {
            secondBaseColor = secondNeonMaterial.GetColor("_EmissionColor");
        }
            // ★【ゲーム開始前】カウントダウン中は Intensity を 0 にして消灯
            SetMaterialIntensity(0f); // 2つ同時に消灯スタート

        if (timerText != null) timerText.text = $"{Mathf.CeilToInt(timeRemaining)}";
        if (scoreText != null) scoreText.text = $"SCORE: {score}";
    }

    void Update()
    {
        // 1. カウントダウン中の処理
        if (isCountingDown)
        {
            countdownTimer -= Time.deltaTime;

            // カウントダウン中はずっと確実に消灯 (0)
            SetMaterialIntensity(0f);

            if (countdownTimer > 2.5f) { if (countdownText != null) countdownText.text = "3"; }
            else if (countdownTimer > 1.5f) { if (countdownText != null) countdownText.text = "2"; }
            else if (countdownTimer > 0.5f) { if (countdownText != null) countdownText.text = "1"; }
            else if (countdownTimer > 0f) { if (countdownText != null) countdownText.text = "START!"; }
            else
            {
                // カウントダウン終了！ゲーム開始
                isCountingDown = false;
                isGameActive = true;
                if (countdownText != null) countdownText.enabled = false; 
            }
            return; 
        }

        // 2. ゲームプレイ中の処理
        if (isGameActive)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateUI();

                // ★【ゲーム中】サイン波（Sin波）を使って、Intensityを 0～1 の間で行ったり来たりさせる（波打つ明滅）
                // Time.time * 3f の数値を大きくすると、明滅のテンポが速くなります
                float currentIntensity = (Mathf.Sin(Time.time * 3f) + 1f) / 2f; 
                SetMaterialIntensity(currentIntensity);
            }
            else
            {
                timeRemaining = 0;
                isGameActive = false;
                OnTimeUp();
            }
        }
    }

    // マテリアルの発光強度（Intensity）を書き換える関数
    void SetMaterialIntensity(float intensity)
    {
        if (neonMaterial != null)
        {
            // Unityの内部仕様に合わせて、元の色に intensity（2の乗数表現）を掛け合わせて適用する
            // 0の時は完全消灯、1の時は元の最大発光になります
            Color finalColor = baseColor * Mathf.LinearToGammaSpace(intensity);
            neonMaterial.SetColor("_EmissionColor", finalColor);
            
            // 反射などがリアルタイムに更新されるようにする命令
            DynamicGI.SetEmissive(GetComponent<Renderer>(), finalColor);
        }
        // ★【追加】2つ目のマテリアルも全く同じ明るさに同時にコントロールする
        if (secondNeonMaterial != null)
        {
            Color finalColor2 = secondBaseColor * Mathf.LinearToGammaSpace(intensity);
            secondNeonMaterial.SetColor("_EmissionColor", finalColor2);
            // ※もしGameManager自身にRendererがついている場合はそのままでOKですが、
            // 念のためリアルタイム更新の命令を2つ目にも優しく適用します
            DynamicGI.SetEmissive(GetComponent<Renderer>(), finalColor2);
        }
    }

    public void AddScore()
    {
        if (!isGameActive) return;
        score++;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (timerText != null) timerText.text = $"{Mathf.CeilToInt(timeRemaining)}";
        if (scoreText != null) scoreText.text = $"SCORE: {score}";
    }

    void OnTimeUp()
    {
        if (timerText != null) timerText.text = "TIME UP!";
        if (scoreText != null) scoreText.text = $"FINAL SCORE: {score}";
        
        // ★【ゲーム終了】タイムアップした瞬間に Intensity を 0 に戻して完全消灯
        SetMaterialIntensity(0f);

        Debug.Log("タイムアップ！ネオンを消灯しました。");
    }

    // ゲームを途中で止めた時などに、マテリアルの色を元の状態に戻す安全対策
    void OnDisable()
    {
        SetMaterialIntensity(1f);
    }
}
