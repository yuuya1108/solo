using UnityEngine;
using UnityEngine.UI;

public class StageProgressBar : MonoBehaviour
{
    [Header("参照設定")]
    [SerializeField] private Transform mainCamera;   // メインカメラのTransform
    [SerializeField] private Slider progressBar;    // UIのSlider

    [Header("ステージ全体の範囲（X座標）")]
    [Tooltip("ステージのスタート位置、またはカメラの初期X座標")]
    [SerializeField] private float startX;
    [Tooltip("ステージのゴール位置、またはカメラの最終停止X座標")]
    [SerializeField] private float endX;

    void Start()
    {
        // カメラが未設定ならMain Cameraを自動取得
        if (mainCamera == null && Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }
    }

    void Update()
    {
        if (mainCamera == null || progressBar == null) return;

        // 1. 現在のカメラのX座標を取得
        float cameraX = mainCamera.position.x;

        // 2. スタート(startX)からゴール(endX)の間で、カメラがどこにいるかを割合(0〜1)で計算
        float currentProgress = Mathf.InverseLerp(startX, endX, cameraX);

        // 3. 値を0〜1の間にクランプ（制限）してSliderに適用
        progressBar.value = Mathf.Clamp01(currentProgress);
    }
}