using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    private CharacterController controller;

    [Header("--- 移動速度設定 ---")]
    public float baseSpeed = 4f;        // 初期状態の移動速度
    public float speedIncrease = 1.5f;   // 1回倒されるごとにアップする速度
    public float maxSpeed = 15f;        // スピードの上限値
    private float currentSpeed;

    [Header("--- ランダム移動の頻度 ---")]
    public float directionChangeInterval = 0.8f; // 何秒ごとに進路を変えるか（短いほど細かくブレる）
    private float timer;
    private Vector3 moveDirection;

    void Start()
    {
        // もしスタイリッシュターゲットにCharacterControllerがついていなければ、自動で取得
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }

        currentSpeed = baseSpeed;
        SetRandomDirection();
    }

    void Update()
    {
        // 1. 一定時間ごとに進路をランダムに変更する（不規則なレレレ動きの再現）
        timer += Time.deltaTime;
        if (timer >= directionChangeInterval)
        {
            SetRandomDirection();
            timer = 0f;
        }

        // 2. 地面移動の実行（Y軸は重力を考慮して固定。落ちないように安全に移動）
        Vector3 velocity = moveDirection * currentSpeed;
        velocity.y = -9.8f; // 簡易的な下向きの力（地面に接地させるため）

        controller.Move(velocity * Time.deltaTime);

        // 3. 進行方向を自動で向く（体の向きを歩く方向に合わせる）
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
        // GameManagerを取得し、今が「プレイ中」でなければ、敵の移動処理も一切せずにここで終了する
        GameManager gameManager = Object.FindAnyObjectByType<GameManager>();
        if (gameManager != null && !gameManager.IsPlaying()) return;
    }

    // ランダムな方向（360度）を決める関数
    void SetRandomDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDirection = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)).normalized;
    }

    // ★【重要】TargetHealthから、リスポーン（撃破）された瞬間に呼び出される関数
    public void IncreaseSpeed()
    {
        currentSpeed += speedIncrease;
        currentSpeed = Mathf.Min(currentSpeed, maxSpeed); // 上限を超えないように制御
        Debug.Log($"的の速度がアップしました！ 現在の速度: {currentSpeed}");
    }
}
