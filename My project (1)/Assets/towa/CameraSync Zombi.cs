using UnityEngine;
public class CameraRelativeFollow : MonoBehaviour
{
    [Header("カメラからのズレ（位置調整）")]
    public Vector3 offset;
    [Header("追従する軸の設定")]
    public bool followX = true;
    public bool followY = false;
    // Y軸（上下）もついて行かせたい場合はtrueにする
    private Transform cameraTransform;
    void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }
    void LateUpdate()
    {
        if (cameraTransform == null)
            return;
        // 現在の自分の位置をベースにする
        Vector3 targetPosition = transform.position;
        // X軸を追従する場合
        if (followX)
        {
            targetPosition.x = cameraTransform.position.x + offset.x;
        }
        // Y軸を追従する場合

        if (followY)
        {
            targetPosition.y = cameraTransform.position.y + offset.y;
        }
        // 座標を適用
        transform.position = targetPosition;
    }
}
