using UnityEngine;

public class RotateTarget : MonoBehaviour
{
    [Header("--- 回転速度設定 ---")]
    [Tooltip("1秒間に何度回転するか（数値が大きいほど高速）")]
    public float rotateSpeed = 90f;

    [Tooltip("回転させる軸（基本はY軸の自転がおすすめ）")]
    public Vector3 rotationAxis = Vector3.up; // (0, 1, 0) ＝ Y軸を中心に回る

    void Update()
    {
        // フレームレートに関わらず、一定の速度でくるくる回転させる
        transform.Rotate(rotationAxis * rotateSpeed * Time.deltaTime, Space.Self);
    }
}
