using UnityEngine;

public class TrackingTarget : MonoBehaviour
{
    [Header("--- 移動スピード設定 ---")]
    [Tooltip("左右に動くスピード")]
    public float moveSpeed = 5f;
    
    [Tooltip("左右に折り返す幅（中心からの距離）")]
    public float moveRange = 4f;

    [Header("--- ジャンプ設定 ---")]
    [Tooltip("ジャンプする上方向への力")]
    public float jumpForce = 7f;
    
    [Tooltip("どのくらいの頻度でジャンプするか（秒）")]
    public float jumpInterval = 2f;

    private Vector3 startPosition;
    private int direction = 1; // 1 = 右, -1 = 左
    private Rigidbody rb;
    private float jumpTimer;

    void Start()
    {
        // 開始位置を記録
        startPosition = transform.position;
        jumpTimer = jumpInterval;

        // 物理挙動（ジャンプ）のためにRigidbodyを取得、なければ自動追加
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // 人形が倒れないように回転を固定し、物理で勝手に横移動しないように設定
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
    }

    void Update()
    {
        // 1. 左右のジグザグ移動（X軸）
        float currentX = transform.position.x;
        
        // 折り返し地点に到達したら方向を反転
        if (currentX > startPosition.x + moveRange)
        {
            direction = -1;
        }
        else if (currentX < startPosition.x - moveRange)
        {
            direction = 1;
        }

        // 横移動の処理（物理ではなくトランスフォームで制御してキレのある動きに）
        transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime, Space.World);

        // 2. ランダムジャンプの処理（一定時間ごとに確率でジャンプ）
        jumpTimer -= Time.deltaTime;
        if (jumpTimer <= 0f)
        {
            jumpTimer = jumpInterval;
            
            // 地面にいる時（Y座標が初期位置に近い時）だけジャンプ
            if (Mathf.Abs(transform.position.y - startPosition.y) < 0.1f)
            {
                // 70%の確率でジャンプを発動（THE FINALSの不規則な回避運動の再現）
                if (Random.value < 0.7f)
                {
                    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                }
            }
        }
    }
}
