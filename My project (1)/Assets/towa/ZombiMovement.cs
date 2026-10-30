using UnityEngine;
public class ZombieMovement : MonoBehaviour
{
    public float moveSpeed = 3.0f; 
    // 移動速度
    public float changeDirectionTime = 4.0f;
    // 向きを変える時間（秒）

    [Header("地面から出る設定")]
    public float riseSpeed = 2.0f;
    private float targetY;
    private bool isRising = true;

    private Rigidbody2D rb; 
    private float timer; 
    private int direction = 1;
    // 1 = 右, -1 = 左
    private int moveDirection = 1;
    private ZombieKnockback knockback;
    void Start() 
    {
        knockback = GetComponent<ZombieKnockback>();
        rb = GetComponent<Rigidbody2D>(); 
        timer = changeDirectionTime;
        // 1. 本来の地面のY座標を覚えておく
        targetY = transform.position.y; 
        // 2. ゾンビの初期位置を「自分の身長分（約1.5メートル）」だけ地面の下に沈める
        transform.position = new Vector3(transform.position.x, targetY - 1.5f, transform.position.z);
        // 3. せり上がる間は物理（重力など）を一時的に無効化する
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        // 生成時にランダムな向き（右か左）にする
        direction = Random.Range(0, 2) == 0 ? 1 : -1; 
        UpdateSpriteDirection(); 
    } 
    void Update() 
    { 
        //地面から出ている最中の処理
        if(isRising)
        {
            RiseFromGround();
            return;
        }
        if (knockback != null && knockback.IsKnockedBack())
            return;
        // 一定時間ごとに自動で向きを変える
        timer -= Time.deltaTime; 
        if (timer <= 0) 
        { 
            ChangeDirection(); 
        } 
    }

    void FixedUpdate()
    {
        if (isRising) return;

        // 吹っ飛び中は通常移動を停止
        if (knockback != null && knockback.IsKnockedBack())
            return;

        rb.linearVelocity = new Vector2(
            direction * moveSpeed,
            rb.linearVelocity.y
        );
    }

    // 地面からせり上がる処理
    void RiseFromGround()
    { 
        // ターゲットのY座標に向かって少しずつ移動させる
        float newY = Mathf.MoveTowards(transform.position.y, targetY, riseSpeed * Time.deltaTime); 
        transform.position = new Vector3(transform.position.x, newY, transform.position.z); 
        // 目標の高さ（地面）に到達したら
        if (Mathf.Approximately(transform.position.y, targetY)) 
        { 
            isRising = false; 
            // 出現完了！
            rb.bodyType = RigidbodyType2D.Dynamic; 
            // 物理演算を元に戻して歩き出せるようにする
        } 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ノックバック中は向きを変えない
        if (knockback != null && knockback.IsKnockedBack())
            return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            ChangeDirection();
        }
    }
    void ChangeDirection() 
    {
        direction *= -1;
        UpdateSpriteDirection(); 
        timer = changeDirectionTime; 
    } 
    void UpdateSpriteDirection() 
    { 
        // スプライトの向きを進行方向に合わせる（X軸のスケールを反転）
        Vector3 localScale = transform.localScale; 
        if ((direction > 0 && localScale.x < 0) || (direction < 0 && localScale.x > 0)) 
        { 
            localScale.x *= -1;
            transform.localScale = localScale; 
        } 
    }
    void ReverseDirection()
    { 
        // 方向を反転（1なら-1に、-1なら1にする）
        moveDirection *= -1; 
        // 見た目（スプライト）も反転させる場合
        Vector3 newScale = transform.localScale; 
        newScale.x *= -1; 
        transform.localScale = newScale; 
    }
}


