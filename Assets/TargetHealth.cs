using UnityEngine;

public class TargetHealth : MonoBehaviour
{
    [Header("--- ステータス設定 ---")]
    public float maxHP = 100f;
    private float currentHP;

    [Header("--- 効果音設定 ---")]
    public AudioSource audioSource;    // MainCameraのAudioSourceをここに紐付けます
    public AudioClip hitSound;         // 「パシッ」という効果音ファイルをここに紐付けます
    public AudioClip defeatSound;

    [Header("--- リスポーン（再出現）範囲設定 ---")]
    // ターゲットが再出現するステージの範囲（床の広さ）です
    public float minX = -15f;
    public float maxX = 15f;
    public float minZ = -15f;
    public float maxZ = 15f;
    public float spawnY = 1.0f;        // 床にめり込まないための固定の高さ

    void Start()
    {
        // ゲーム開始時にHPを満タンにする
        currentHP = maxHP;
    }

    // プレイヤーが弾を当てたときに外から呼び出されるダメージ関数
    public void TakeDamage(float damageAmount)
    {
        // すでにHPが0（死亡中）なら何もしない
        if (currentHP <= 0) return;

        // ダメージを適用
        currentHP -= damageAmount;

        // HPが0以下（倒した時）の処理
        if (currentHP <= 0)
        {
            // 倒した瞬間の専用音（キルサウンド）を鳴らす
            if (audioSource != null && defeatSound != null)
            {
                audioSource.PlayOneShot(defeatSound);
            }

            // ★【ここに注目！】すでに作成済みの GameManager を探して、スコアを増やす関数を呼び出す
            GameManager gameManager = Object.FindAnyObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.AddScore();
            }

            // リスポーン（復活）処理を呼び出す
            Respawn();
        }
        else
        {
            // まだ生きている場合は通常の着弾音を鳴らす
            if (audioSource != null && hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
        }
    }


    // 別のランダムな場所に瞬間移動し、HPを全回復する処理
    void Respawn()
    {
        // 1. キャラクターコントローラーを取得する
        CharacterController cc = GetComponent<CharacterController>();
        
        // 2. 【最重要】瞬間移動の邪魔にならないよう、一時的にコンポーネントをオフにする
        if (cc != null) cc.enabled = false;

        // 3. ランダムな座標を計算して瞬間移動させる
        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);
        transform.position = new Vector3(randomX, spawnY, randomZ);

        // 4. 移動が終わったら、コンポーネントをオンに戻して再び動けるようにする
        if (cc != null) cc.enabled = true;

        // 体力を満タンにリセット
        currentHP = maxHP;

        // 先ほど合体した「スピードアップ」の命令
        TargetMovement movement = GetComponent<TargetMovement>();
        if (movement != null) movement.IncreaseSpeed();

        Debug.Log("stylish_target が新しい位置にリスポーンしました！: " + transform.position);
    }

}
