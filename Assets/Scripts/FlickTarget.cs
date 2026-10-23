using UnityEngine;

public class FlickTarget : MonoBehaviour
{
    [Header("--- 出現エリアの設定 ---")]
    [Tooltip("中心からの左右の移動幅")]
    public float minX = -4f;
    public float maxX = 4f;

    [Tooltip("床からの高さの範囲")]
    public float minY = 0.5f;
    public float maxY = 3.0f;

    [Tooltip("プレイヤーからの奥行き（距離）")]
    public float fixedZ = 10f;

    // この関数が呼ばれると、オブジェクトがランダムな位置へワープします
    public void MoveToRandomPosition()
    {
        // 範囲内からランダムなX座標とY座標を計算
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        // 新しい位置を設定（Z軸は10メートル先で固定）
        transform.position = new Vector3(randomX, randomY, fixedZ);
    }
}
