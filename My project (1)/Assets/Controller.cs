using UnityEngine;

using System.Collections;

public class Controller : MonoBehaviour

{

    public bool player2 = false;

    //public float knockbackPower = 8f;

    //public float knockbackUp = 4f;

    public bool isGrounded = false;

    public float speed = 10f;

    // 相手プレイヤー

    public Transform enemy;

    // 相手との最低距離

    public float stopDistance = 0.8f;

    // 前ジャンプの強さ

    public float jumpForwardSpeed = 10f;

    // 近づいたときに減速を始める距離

    public float slowDistance = 3f;

    private Rigidbody2D rb;

    private bool isKnockback = false;

    // ジャンプを始めたとき、自分が相手のどちら側にいたか

    private int jumpSide = 0;

    // ジャンプ中か

    private bool isJumping = false;

    //ジャンプ中維持する速度

    private float jumpHorizontalspeed = 0f;

    //ジャンプ中入れ替え防止

    private bool jumpBlocked = false;

    //死亡判定

    public bool isDead = false;

    //リスポーン

    private Transform lastAttacker;

    //ジャンプ音

    public AudioClip jumpsound;

    //リスポーン音

    public AudioClip respawnsound;

    //死亡音

    public AudioClip deadsound;

    //サウンド

    private AudioSource audioSource;

    //攻撃

    public bat Bat;

    public bool isAttackLocked = false;

    // 攻撃を受けてから着地するまでの硬直

    private bool isHitStun = false;

    // ノックバック後、一度地面から離れたか

    private bool hasLeftGround = false;

    void Start()

    {

        Bat = GetComponent<bat>();

        audioSource = GetComponent<AudioSource>();

        rb = GetComponent<Rigidbody2D>();

        // プレイヤー同士が物理的に押し合わないようにする

        if (enemy != null)

        {

            Collider2D myCollider = GetComponent<Collider2D>();

            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();

            if (myCollider != null && enemyCollider != null)

            {

                Physics2D.IgnoreCollision(myCollider, enemyCollider);

            }

        }

    }

    void Update()

    {

        // =========================

        // 死亡判定

        // =========================

        if (isDead == false)

        {

            Dead();

        }


        //// =========================

        //// 吹き飛ばし

        //// =========================

        //if (!isKnockback && !isDead)

        //{

        //    //距離

        //    float distance = Mathf.Abs(

        //        enemy.position.x - transform.position.x);

        //    //P1

        //    if (!player2 && Input.GetKeyDown(KeyCode.Joystick2Button1) && distance <= 2f)

        //    {

        //        rb.linearVelocity = new Vector2(

        //            -knockbackPower, knockbackUp);

        //        StartCoroutine(Knockback());

        //    }

        //    //P2

        //    if (player2 && Input.GetKeyDown(KeyCode.Joystick1Button1) && distance <= 2f)

        //    {

        //        rb.linearVelocity = new Vector2(

        //            knockbackPower, knockbackUp);

        //        StartCoroutine(Knockback());

        //    }

        //}
        //とわ追加ノックバック中なら、以下の移動処理をすべてスキップする
        if (GetComponent<PlayerKnockback>().IsKnockbacked)
        {
            return;
        }
        // =========================

        // 通常操作

        // =========================

        if (isAttackLocked && !isDead && !isKnockback)

        {

            if (!isGrounded)

            {

                float moveX = jumpBlocked

                    ? 0f

                    : LimitJumpMovement(jumpHorizontalspeed);

                rb.linearVelocity = new Vector2(moveX, rb.linearVelocity.y);

            }

            else

            {

                rb.linearVelocity = new Vector2(

                    0f,

                    rb.linearVelocity.y

                    );

            }

            return;

        }

        if (!isKnockback && !isDead && !isAttackLocked)

        {

            float inputX = 0;

            // =========================

            // P1

            // =========================

            if (!player2)

            {

                if (Input.GetKey(KeyCode.A))

                    inputX = -1;

                if (Input.GetKey(KeyCode.D))

                    inputX = 1;

                if (Input.GetKeyDown(KeyCode.W) && isGrounded)

                {

                    Jump(inputX);

                }

            }

            // =========================

            // P2

            // =========================

            else

            {

                if (Input.GetKey(KeyCode.LeftArrow))

                    inputX = -1;

                if (Input.GetKey(KeyCode.RightArrow))

                    inputX = 1;

                if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)

                {

                    Jump(inputX);

                }

            }

            // =========================

            // 横移動

            // =========================

            if (isGrounded)//地上

            {

                float moveSpeed = inputX * speed;

                // 相手に近づくときだけ自然に減速

                moveSpeed = LimitMovement(moveSpeed);

                rb.linearVelocity = new Vector2(

               moveSpeed,

               rb.linearVelocity.y

               );

            }

            else //空中

            {

                if (jumpBlocked)

                {

                    rb.linearVelocity = new Vector2(

                        0f,

                        rb.linearVelocity.y

                        );

                }

                else

                {

                    rb.linearVelocity = new Vector2(

                       LimitJumpMovement(jumpHorizontalspeed),

                       rb.linearVelocity.y

                   );

                }

            }

        }

    }

    // =========================

    // 外部（bat.csなど）から呼ばれるノックバック受付口

    // 攻撃の種類ごとに硬直時間（duration）を指定できるように変更

    // =========================

    // 攻撃を受けたときに呼ばれる

    public void ReceiveKnockback(float powerX, float powerY, float duration)

    {

        StopCoroutine(nameof(KnockbackRoutine));

        StartCoroutine(KnockbackRoutine(powerX, powerY, duration));

    }

    IEnumerator KnockbackRoutine(float powerX, float powerY, float duration)

    {

        // 操作をロック

        isKnockback = true;

        isJumping = false;

        jumpBlocked = false;

        // 攻撃を受けた瞬間に地面にいるか確認

        hasLeftGround = !isGrounded;

        // ノックバックで吹き飛ばす

        rb.linearVelocity = Vector2.zero;

        rb.linearVelocity = new Vector2(powerX, powerY);

        // 一度地面から離れ、再び着地するまで待つ

        while (!hasLeftGround || !isGrounded)

        {

            yield return null;

        }

        // 着地したので操作を解禁

        isKnockback = false;

    }

    // =========================

    // ジャンプ

    // =========================

    void Jump(float inputX)

    {

        if (enemy != null)

        {

            // ジャンプ開始時の自分の位置を記録

            if (transform.position.x < enemy.position.x)

                jumpSide = -1;

            else

                jumpSide = 1;

            isJumping = true;

            jumpBlocked = false;

        }

        //ジャンプ時の入力で方向を決定

        if (inputX > 0.2f)

            jumpHorizontalspeed = jumpForwardSpeed;

        else if (inputX < -0.2f)

            jumpHorizontalspeed = -jumpForwardSpeed;

        else

            jumpHorizontalspeed = 0f;

        rb.linearVelocity = new Vector2(

            jumpHorizontalspeed,

            10f

        );

        //ジャンプサウンド

        if (jumpsound != null && audioSource != null)

        {

            audioSource.PlayOneShot(jumpsound);

        }

    }

    // =========================

    // 通常移動の制限

    // =========================

    float LimitMovement(float moveSpeed)

    {

        if (isDead)

            return moveSpeed;

        if (enemy == null)

            return moveSpeed;

        Controller enemyController = enemy.GetComponent<Controller>();

        if (enemyController != null && enemyController.isDead)

            return moveSpeed;

        float distance = Mathf.Abs(

            enemy.position.x - transform.position.x

        );

        // 相手から遠いなら普通に動く

        if (distance >= slowDistance)

            return moveSpeed;

        // 自分が相手の左側

        if (transform.position.x < enemy.position.x)

        {

            // 右に進むときだけ減速

            if (moveSpeed > 0)

            {

                return SlowDown(moveSpeed, distance);

            }

        }

        // 自分が相手の右側

        else if (transform.position.x > enemy.position.x)

        {

            // 左に進むときだけ減速

            if (moveSpeed < 0)

            {

                return SlowDown(moveSpeed, distance);

            }

        }

        return moveSpeed;

    }

    // =========================

    // ジャンプの制限

    // =========================

    float LimitJumpMovement(float jumpSpeed)

    {

        if (isDead)

            return jumpSpeed;

        if (enemy == null)

            return jumpSpeed;

        Controller enemyController = enemy.GetComponent<Controller>();

        if (enemyController != null && enemyController.isDead)

            return jumpSpeed;

        float distance = Mathf.Abs(

            enemy.position.x - transform.position.x

        );

        // 遠ければ普通にジャンプ

        if (distance >= slowDistance)

            return jumpSpeed;

        // 自分がジャンプ開始時に左側だった

        if (jumpSide == -1)

        {

            // 右方向へのジャンプだけ減速

            if (jumpSpeed > 0)

            {

                return SlowDown(jumpSpeed, distance);

            }

        }

        // 自分がジャンプ開始時に右側だった

        else if (jumpSide == 1)

        {

            // 左方向へのジャンプだけ減速

            if (jumpSpeed < 0)

            {

                return SlowDown(jumpSpeed, distance);

            }

        }

        return jumpSpeed;

    }

    // =========================

    // 距離に応じて自然に減速

    // =========================

    float SlowDown(float moveSpeed, float distance)

    {

        if (isDead == true)

            return moveSpeed;

        if (distance <= stopDistance)

        {

            return 0;

        }

        // 0～1

        float ratio =

            (distance - stopDistance) /

            (slowDistance - stopDistance);

        // なめらかに減速

        ratio = Mathf.Clamp01(ratio);

        // SmoothStepで自然にする

        ratio = ratio * ratio * (3f - 2f * ratio);

        return moveSpeed * ratio;

    }

    // =========================

    // 物理処理

    // =========================

    void FixedUpdate()

    {
        PreventJumpPassing();
        PreventPlayerCrossing();
    }

    // =========================

    // 万が一の入れ替わり防止

    // =========================

    void PreventJumpPassing()

    {

        if (isDead == true || enemy == null || !isJumping)

            return;

        Controller enemyController = enemy.GetComponent<Controller>();

        if (enemyController != null && enemyController.isDead)

            return;

        float stopX;

        if (!player2)

        {

            //P1

            if (jumpHorizontalspeed <= 0f)

                return;

            stopX = enemy.position.x - stopDistance;

            if (rb.position.x >= stopX)

            {

                jumpBlocked = true;

                jumpHorizontalspeed = 0f;

                rb.position = new Vector2(stopX, rb.position.y);

                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            }

        }

        else

        {

            //P2

            if (jumpHorizontalspeed >= 0f)

                return;

            stopX = enemy.position.x + stopDistance;

            if (rb.position.x <= stopX)

            {

                jumpBlocked = true;

                jumpHorizontalspeed = 0f;

                rb.position = new Vector2(stopX, rb.position.y);

                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            }

        }

    }

    //ジャンプ地面にいるか検知     

    void OnCollisionEnter2D(Collision2D collision)

    {

        if (collision.gameObject.CompareTag("Ground"))

        {

            foreach (ContactPoint2D contact in collision.contacts)

            {

                if (contact.normal.y > 0.5f)

                {

                    isGrounded = true;

                    isJumping = false;

                    jumpBlocked = false;

                    jumpHorizontalspeed = 0f;

                }

            }

        }

        foreach (ContactPoint2D contact in collision.contacts)

        {

            //側面にぶつかった場合

            if (Mathf.Abs(contact.normal.x) > 0.5f)

            {

                jumpHorizontalspeed = 0f;

                rb.linearVelocity = new Vector2(

                    0f,

                    rb.linearVelocity.y

                    );

                break;

            }

        }

    }

    void OnCollisionExit2D(Collision2D collision)

    {

        if (collision.gameObject.CompareTag("Ground"))

        {

            isGrounded = false;

            // ノックバック中に地面から離れたことを記録

            if (isKnockback)

            {

                hasLeftGround = true;

            }

        }

    }

    //死亡判定

    void Dead()

    {

        if (transform.position.y < -10f)

        {

            isDead = true;

            isJumping = false;

            if (deadsound != null && audioSource != null)

            {

                audioSource.PlayOneShot(deadsound);

            }

            // 横方向の移動制限を解除

            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            StartCoroutine(Respawn());

        }

    }

    //リスポーン

    IEnumerator Respawn()

    {

        if (isDead == true)

        {

            yield return new WaitForSeconds(3f);

            float spawnX;

            // P1は相手の左

            if (!player2)

            {

                spawnX = enemy.position.x - 20f;

            }

            // P2は相手の右

            else

            {

                spawnX = enemy.position.x + 20f;

            }

            // 地面を探す

            float searchDistance = 0f;

            float searchStep = 1f;

            RaycastHit2D hit = Physics2D.Raycast(

                new Vector2(spawnX, 10f),

                Vector2.down,

                30f

            );

            // 最初の場所に地面がなかったら左右を探す

            while (hit.collider == null || !hit.collider.CompareTag("Ground"))

            {

                searchDistance += searchStep;

                // 右側を探す

                hit = Physics2D.Raycast(

                    new Vector2(spawnX + searchDistance, 10f),

                    Vector2.down,

                    30f

                );

                // 右になければ左側を探す

                if (hit.collider == null || !hit.collider.CompareTag("Ground"))

                {

                    hit = Physics2D.Raycast(

                        new Vector2(spawnX - searchDistance, 10f),

                        Vector2.down,

                        30f

                    );

                }

                // これ以上探さない

                if (searchDistance > 20f)

                {

                    yield break;

                }

            }

            // 見つけた地面の上にリスポーン

            transform.position = new Vector3(

                hit.point.x,

                hit.point.y + 1f,

                transform.position.z

            );

            // 速度などをリセット

            rb.linearVelocity = Vector2.zero;

            isKnockback = false;

            isJumping = false;

            isDead = false;

            //リスポーンサウンド

            if (respawnsound != null && audioSource != null)

            {

                audioSource.PlayOneShot(respawnsound, 0.3f);

            }

        }

    }

    private void PreventPlayerCrossing()
    {
        if (enemy == null || rb == null || isDead)
            return;

        Controller enemyController = enemy.GetComponent<Controller>();

        if (enemyController != null && enemyController.isDead)
            return;

        float boundary;

        if (!player2)
        {
            // P1はP2の左側を維持する
            boundary = enemy.position.x - stopDistance;

            if (rb.position.x > boundary)
            {
                rb.position = new Vector2(boundary, rb.position.y);

                if (rb.linearVelocity.x > 0f)
                {
                    rb.linearVelocity = new Vector2(
                        0f,
                        rb.linearVelocity.y
                    );
                }
            }
        }
        else
        {
            // P2はP1の右側を維持する
            boundary = enemy.position.x + stopDistance;

            if (rb.position.x < boundary)
            {
                rb.position = new Vector2(boundary, rb.position.y);

                if (rb.linearVelocity.x < 0f)
                {
                    rb.linearVelocity = new Vector2(
                        0f,
                        rb.linearVelocity.y
                    );
                }
            }
        }
    }

    //// =========================

    //// 吹き飛ばし

    //// =========================

    //IEnumerator Knockback()

    //{

    //    isKnockback = true;

    //    yield return new WaitForSeconds(1.2f);

    //    isKnockback = false;

    //}

}
