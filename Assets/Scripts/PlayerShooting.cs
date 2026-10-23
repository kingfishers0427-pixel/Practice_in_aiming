using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("--- 射撃設定 ---")]
    public float range = 100f;
    public GameObject sparkPrefab; // SparkEffect火花

    [Header("--- フルオート設定 ---")]
    public bool isFullAuto = true; 
    public float fireRate = 10f; 
    private float nextTimeToFire = 0f;

    [Header("--- マガジン・リロード設定 ---")]
    [Tooltip("マガジンに入る最大弾数（例: AKMなら36発）")]
    public int maxAmmo = 36;
    [Tooltip("リロードにかかる時間（秒）")]
    public float reloadTime = 2.0f;
    
    private int currentAmmo;
    private bool isReloading = false;

    [Header("--- UI設定 ---")]
    [Tooltip("残弾数を表示するTextMesh Pro UI")]
    public TMPro.TextMeshProUGUI ammoText;

    [Header("--- リコイル（反動）設定 ---")]
    public float upRecoilForce = 1.2f;
    public float sideRecoilForce = 0.4f;

    [Header("--- サウンド設定（SE） ---")]
    public AudioClip fireSound;
    public AudioClip hitSound;
    [Tooltip("リロード中に鳴らす効果音")]
    public AudioClip reloadSound;

    private Camera cam;
    private AudioSource audioSource;
    private GameManager gameManager;
    private FinalsMouseLook mouseLook;

    void Start()
    {
        cam = GetComponent<Camera>();
        audioSource = GetComponent<AudioSource>();
        gameManager = Object.FindAnyObjectByType<GameManager>();
        mouseLook = GetComponent<FinalsMouseLook>();

        // 開始時はマガジンを満タンにする
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    void Update()
    {
        if (gameManager != null && !gameManager.IsPlaying()) return;

        // リロード中は一切の射撃・重複リロードを入力を受け付けない
        if (isReloading) return;

        // 【リロード入力】Rキーが押された、または弾が空の状態で撃とうとしたらリロード開始
        if (Input.GetKeyDown(KeyCode.R) || (currentAmmo <= 0 && Input.GetMouseButtonDown(0)))
        {
            // すでに満タンならリロードしない
            if (currentAmmo < maxAmmo)
            {
                StartCoroutine(Reload());
                return;
            }
        }

        // 残弾がある時だけ射撃可能
        if (currentAmmo > 0)
        {
            if (isFullAuto)
            {
                if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire)
                {
                    nextTimeToFire = Time.time + 1f / fireRate;
                    Shoot();
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Shoot();
                }
            }
        }
    }

    void Shoot()
    {
        // 弾数を1発減らす
        currentAmmo--;
        UpdateAmmoUI();

        // 1. 撃った瞬間にカメラにリコイル（反動）を与える
        if (mouseLook != null)
        {
            float randomHorizontal = Random.Range(-sideRecoilForce, sideRecoilForce);
            mouseLook.AddRecoil(upRecoilForce, randomHorizontal);
        }

        // 2. 銃声（発射音）を鳴らす
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        // 3. 画面中央からのレイキャスト（弾の当たり判定）
        Vector3 rayOrigin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range))
        {
            // 1. まず、当たった相手が「的の体力（TargetHealth）」を持っているか調べる
            TargetHealth target = hit.transform.GetComponent<TargetHealth>();
            
            if (target != null)
            {
                // 【的（stylish_target）に当たった時の処理】
                target.TakeDamage(25f);

                // 的用の火花を生成
                if (sparkPrefab != null)
                {
                    GameObject spark = Instantiate(sparkPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(spark, 0.5f); 
                }
            }
            else
            {
                // 2. 【的以外（床、障害物、見えない壁など）に当たった時の処理】
                // 何もない空中（虚空）ではなく、何かしらの物理オブジェクトに衝突した時だけ火花を散らす
                if (sparkPrefab != null)
                {
                    GameObject spark = Instantiate(sparkPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(spark, 0.5f); 
                }
            }
}

    }

    // 時間の経過（待機）を伴うリロード処理（コルーチン）
    System.Collections.IEnumerator Reload()
    {
        isReloading = true;
        if (ammoText != null) ammoText.text = "RELOADING...";

        // リロード音を鳴らす
        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        // 指定したリロード時間（秒）だけ待つ
        yield return new WaitForSeconds(reloadTime);

        // マガジンを最大まで回復
        currentAmmo = maxAmmo;
        isReloading = false;
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = $"AMMO: {currentAmmo} / {maxAmmo}";
        }
    }
}
