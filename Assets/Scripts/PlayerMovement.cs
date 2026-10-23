using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;

    [Header("--- 移動速度設定 ---")]
    public float walkSpeed = 5f;
    public float runSpeed = 8.5f;      
    public float crouchSpeed = 2.5f;   
    private float currentSpeed;

    [Header("--- 重力・ジャンプ設定 ---")]
    public float gravity = -25f;       
    public float jumpHeight = 2.5f;    
    private Vector3 velocity;
    private bool isGrounded;

    [Header("--- しゃがみ設定（高さ固定版） ---")]
    public float standHeight = 2.0f;    
    public float crouchHeight = 1.0f;   
    
    public Transform cameraTransform;   
    public float standCameraY = 1.6f;   
    public float crouchCameraY = 0.8f;  

    [Header("--- 足音（フットステップ）設定 ---")]
    public AudioSource audioSource;     // MainCameraのAudioSourceをここに紐付けます
    public AudioClip walkSound;         // 歩く時の足音ファイル
    public AudioClip runSound;          // 走る時の足音ファイル
    
    public float walkStepInterval = 0.5f; // 歩く時の足音の間隔（秒）
    public float runStepInterval = 0.3f;  // 走る時の足音の間隔（短いほどハイテンポになります）
    private float stepTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;

        controller.height = standHeight;
        controller.center = new Vector3(0, standHeight / 2f, 0);
        if (cameraTransform != null)
        {
            cameraTransform.localPosition = new Vector3(0, standCameraY, 0);
        }
    }

    void Update()
    {
        // 1. 地面判定
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // 2. Cキーの入力による「しゃがみ」判定
        if (Input.GetKey(KeyCode.C))
        {
            controller.height = crouchHeight;
            controller.center = new Vector3(0, crouchHeight / 2f, 0);
            currentSpeed = crouchSpeed;

            if (cameraTransform != null)
            {
                cameraTransform.localPosition = new Vector3(0, crouchCameraY, 0);
            }
        }
        else
        {
            controller.height = standHeight;
            controller.center = new Vector3(0, standHeight / 2f, 0);

            // 速度の決定（Shift押しでダッシュ、それ以外は通常歩き）
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Vertical") > 0)
            {
                currentSpeed = runSpeed;
            }
            else
            {
                currentSpeed = walkSpeed;
            }

            if (cameraTransform != null)
            {
                cameraTransform.localPosition = new Vector3(0, standCameraY, 0);
            }
        }

        // 3. 通常の移動計算
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // ★【追加】足音の再生ロジック
        HandleFootsteps(x, z);

        // 4. ジャンプ処理
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 5. 重力適用
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // 長い足音ファイルを最初の一歩だけでブツ切りにして綺麗に鳴らす関数
    void HandleFootsteps(float x, float z)
    {
        // 地面に接していて、かつキーボード入力（移動）がある場合
        if (isGrounded && (x != 0 || z != 0))
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0)
            {
                if (audioSource != null)
                {
                    // 1. 一旦、現在鳴っている足音をリセットして重なりを防ぐ
                    audioSource.Stop(); 

                    // 2. 状態に合わせてクリップをセットして再生
                    if (currentSpeed == runSpeed && runSound != null)
                    {
                        audioSource.clip = runSound;
                        audioSource.Play();
                        stepTimer = runStepInterval; // 走るテンポ
                    }
                    else if (walkSound != null)
                    {
                        audioSource.clip = walkSound;
                        audioSource.Play();
                        // しゃがみ歩きの時はテンポを少し遅く
                        stepTimer = (currentSpeed == crouchSpeed) ? walkStepInterval * 1.5f : walkStepInterval;
                    }

                    // 3. 【ここが最重要！】
                    // 次の足音が鳴るタイミング（stepTimer秒後）が来たら、音声が途中でも強制的にブツッと止めるようにします。
                    // これにより、10歩入っている長いファイルでも「最初の一歩（タッ）」しか画面に流れなくなります。
                    Invoke("StopAudioSafely", stepTimer - 0.02f);
                }
            }
        }
        else
        {
            // 立ち止まったらタイマーをリセットし、音も即座に止める
            if (audioSource != null && (audioSource.clip == walkSound || audioSource.clip == runSound))
            {
                audioSource.Stop();
                CancelInvoke("StopAudioSafely");
            }
            stepTimer = 0f;
        }
    }

    // 安全に音声をカットするための補助関数
    void StopAudioSafely()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

}
