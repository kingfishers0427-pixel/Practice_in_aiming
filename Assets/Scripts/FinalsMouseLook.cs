using UnityEngine;

public class FinalsMouseLook : MonoBehaviour
{
    [Header("--- THE FINALS 設定 ---")]
    public float finalsSensitivity = 25f; 
    public float finalsFOV = 90f;

    [Header("--- プレイヤーの体（親オブジェクト） ---")]
    public Transform playerBody;

    private float xRotation = 0f;
    private float calculatedSensitivity;

    void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (cam != null) cam.fieldOfView = finalsFOV;

        calculatedSensitivity = finalsSensitivity * 0.04f;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * calculatedSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * calculatedSensitivity;

        // 通常のマウス移動による首振り
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    // 【追加機能】銃を撃ったときに外から呼ばれる、リコイル（反動）を与える関数
    public void AddRecoil(float verticalRecoil, float horizontalRecoil)
    {
        // 上方向にカメラを跳ね上げる（X回転をマイナスする）
        xRotation -= verticalRecoil;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 左右にランダムでカメラをブレさせる（プレイヤーの体ごと回転）
        playerBody.Rotate(Vector3.up * horizontalRecoil);
    }
}
