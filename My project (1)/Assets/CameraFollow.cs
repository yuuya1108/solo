using UnityEngine;

public class CameraFollow2DFight : MonoBehaviour
{
    public Transform player1;
    public Transform player2;

    [Header("追従のなめらかさ")]
    public float smoothSpeed = 5f;

    [Header("画面端の余白（プレイヤーの幅に合わせる）")]
    public float screenPadding = 0.5f;

    [Header("カメラを押し出せる画面端からの距離")]
    [Range(0.1f, 2.0f)]
    public float pushZoneWidth = 0.8f;

    private Camera cam;

    // プレイヤーのControllerスクリプトへの参照
    private Controller p1Controller;
    private Controller p2Controller;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        // 各プレイヤーのControllerスクリプトを取得
        if (player1 != null) p1Controller = player1.GetComponent<Controller>();
        if (player2 != null) p2Controller = player2.GetComponent<Controller>();
    }

    void LateUpdate()
    {
        if (player1 == null || player2 == null) return;

        // 呼び出し先（Controller）のisDeadフラグから生存判定を取得
        bool p1Alive = p1Controller != null ? !p1Controller.isDead : true;
        bool p2Alive = p2Controller != null ? !p2Controller.isDead : true;

        // 1. 通常の目標X座標を計算（ベースとなる追従処理）
        float targetX = transform.position.x;

        if (p1Alive && p2Alive)
        {
            targetX = (player1.position.x + player2.position.x) / 2f;
        }
        else if (!p1Alive && p2Alive)
        {
            targetX = player2.position.x;
        }
        else if (p1Alive && !p2Alive)
        {
            targetX = player1.position.x;
        }

        // なめらかな目標座標への移動（ベース）
        Vector3 targetPosition = new Vector3(targetX, transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // 2. 画面端を押し出す処理（生存しているプレイヤーが端にいたらカメラを強制移動）
        if (cam != null)
        {
            float screenHalfWidth = cam.orthographicSize * ((float)Screen.width / Screen.height);

            // カメラがこれ以上外にいったらプレイヤーを押し出すと判定する境界（プッシュゾーン）
            float leftPushBound = transform.position.x - screenHalfWidth + pushZoneWidth;
            float rightPushBound = transform.position.x + screenHalfWidth - pushZoneWidth;

            // プレイヤー1の押し出し判定
            if (p1Alive)
            {
                if (player1.position.x > rightPushBound)
                {
                    float overlap = player1.position.x - rightPushBound;
                    transform.position += new Vector3(overlap, 0, 0);
                }
                else if (player1.position.x < leftPushBound)
                {
                    float overlap = player1.position.x - leftPushBound;
                    transform.position += new Vector3(overlap, 0, 0);
                }
            }

            // プレイヤー2の押し出し判定
            if (p2Alive)
            {
                if (player2.position.x > rightPushBound)
                {
                    float overlap = player2.position.x - rightPushBound;
                    transform.position += new Vector3(overlap, 0, 0);
                }
                else if (player2.position.x < leftPushBound)
                {
                    float overlap = player2.position.x - leftPushBound;
                    transform.position += new Vector3(overlap, 0, 0);
                }
            }
        }

        // 3. 最終的なカメラ位置を基準に、プレイヤーを画面内に閉じ込める
        // ※ただし、死亡している（リスポーン待機中の）プレイヤーは閉じ込め処理から除外する
        RestrictPlayersInScreen(p1Alive, p2Alive);
    }

    /// <summary>
    /// 生存しているプレイヤーがカメラの外に出ないように位置を制限する
    /// </summary>
    void RestrictPlayersInScreen(bool p1Alive, bool p2Alive)
    {
        if (cam == null) return;

        float screenHalfWidth = cam.orthographicSize * ((float)Screen.width / Screen.height);
        float minX = transform.position.x - screenHalfWidth + screenPadding;
        float maxX = transform.position.x + screenHalfWidth - screenPadding;

        // プレイヤー1が生存している場合のみ画面内に閉じ込める
        if (p1Alive)
        {
            Vector3 p1Pos = player1.position;
            p1Pos.x = Mathf.Clamp(p1Pos.x, minX, maxX);
            player1.position = p1Pos;
        }

        // プレイヤー2が生存している場合のみ画面内に閉じ込める
        if (p2Alive)
        {
            Vector3 p2Pos = player2.position;
            p2Pos.x = Mathf.Clamp(p2Pos.x, minX, maxX);
            player2.position = p2Pos;
        }
    }
}