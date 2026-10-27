using System.Collections;

using UnityEngine;

public class bat : MonoBehaviour

{

    public bool player2 = false;

    [Header("攻撃範囲の設定（ボタンを離した/最大溜め時に判定）")]

    public float attackRangeX = 3f;

    public float attackRangeY = 1.5f;

    [Header("溜め段階と威力の調整")]

    public float mediumChargeThreshold = 2f; // 中溜めに必要な時間（秒）

    public float maxChargeTime = 4f;         // 強攻撃になる（解放される）時間（秒）

    public float autoReleaseTime = 6f;

    [Header("1. 通常攻撃の設定")]

    public float normalPowerX = 10f;

    public float normalPowerY = 4f;

    public float normalHitStun = 0.4f;

    public float normalAttackCooldown = 0.3f; // ★追加：通常攻撃の硬直時間

    [Header("2. 中溜め攻撃の設定")]

    public float mediumPowerX = 15f;

    public float mediumPowerY = 6f;

    public float mediumHitStun = 0.8f;

    public float mediumAttackCooldown = 0.6f; // ★追加：中溜め攻撃の硬直時間

    [Header("3. 強溜め攻撃の設定")]

    public float chargedPowerX = 25f;

    public float chargedPowerY = 9f;

    public float chargedHitStun = 1.5f;

    public float chargedAttackCooldown = 1.2f; // ★追加：強溜め攻撃の硬直時間

    [Header("★見た目（スプライト）の変更設定")]

    public SpriteRenderer spriteRenderer;

    public Sprite defaultSprite;

    public Sprite startChargeSprite;

    [Header("― 通常攻撃の画像 ―")]

    public Sprite normalAttackSprite;

    public Sprite normalChargeSprite;

    public Sprite normalRecoverySprite;

    [Header("― 中溜め攻撃の画像 ―")]

    public Sprite mediumChargeSprite;

    public Sprite mediumAttackSprite;

    public Sprite mediumRecoverySprite;

    [Header("― 強溜め攻撃の画像 ―")]

    public Sprite maxChargeSprite;

    public Sprite chargedAttackSprite;

    public Sprite chargedRecoverySprite;

    [Header("参照")]

    public Transform enemy;

    public bool isAttackCooldown = false;

    private float currentChargeTime = 0f;

    private bool isCharging = false;

    public bool isAttacking = false;

    private AudioSource audioSource;

    //攻撃サウンド

    public AudioClip weakAttacksound;

    public AudioClip mediumAttacksound;

    public AudioClip maxAttacksound;

    public AudioClip airAttacksound;



    void Start()

    {

        audioSource = GetComponent<AudioSource>();

        if (spriteRenderer == null)

        {

            spriteRenderer = GetComponent<SpriteRenderer>();

        }

    }

 
    void Update()
    {
        // 攻撃中・硬直中・敵がいないときは入力を受け付けない
        if (enemy == null || isAttackCooldown || isAttacking)
            return;

        KeyCode attackKey;
        KeyCode gamepadAttackKey;

        if (player2)
        {
            attackKey = KeyCode.DownArrow;
            gamepadAttackKey = KeyCode.Joystick2Button5;
        }
        else
        {
            attackKey = KeyCode.S;
            gamepadAttackKey = KeyCode.JoystickButton5;
        }

        // キーボードまたはゲームパッドの入力状態
        bool keyDown =
            Input.GetKeyDown(attackKey) ||
            Input.GetKeyDown(gamepadAttackKey);

        bool keyHeld =
            Input.GetKey(attackKey) ||
            Input.GetKey(gamepadAttackKey);

        bool keyUp =
            Input.GetKeyUp(attackKey) ||
            Input.GetKeyUp(gamepadAttackKey);

        // 1. ボタンを押した瞬間：溜め開始
        if (keyDown && !isCharging)
        {
            isCharging = true;
            currentChargeTime = 0f;

            if (spriteRenderer != null && startChargeSprite != null)
            {
                spriteRenderer.sprite = startChargeSprite;
            }
        }

        // 2. ボタンを押している間：溜め時間を増やす
        if (isCharging && keyHeld)
        {
            currentChargeTime += Time.deltaTime;

            UpdateChargeVisual();

            // 最大時間を超えたら自動で攻撃
            if (currentChargeTime >= autoReleaseTime)
            {
                currentChargeTime = autoReleaseTime;
                ReleaseAttack();
                return;
            }
        }

        // 3. ボタンを離した瞬間：攻撃
        if (isCharging && keyUp)
        {
            ReleaseAttack();
        }
    }


    void UpdateChargeVisual()

    {

        if (spriteRenderer == null) return;

        if (currentChargeTime >= maxChargeTime)

        {

            if (maxChargeSprite != null) spriteRenderer.sprite = maxChargeSprite;

        }

        else if (currentChargeTime >= mediumChargeThreshold)

        {

            if (mediumChargeSprite != null) spriteRenderer.sprite = mediumChargeSprite;

        }

        else

        {

            if (startChargeSprite != null)

            {

                spriteRenderer.sprite = startChargeSprite;

            }

            else if (defaultSprite != null)

            {

                spriteRenderer.sprite = defaultSprite;

            }

        }

    }

    // 攻撃の発動・判定を行う共通処理

    // 攻撃の発動・判定を行う共通処理

    void ReleaseAttack()

    {

        isCharging = false;

        float distanceX = Mathf.Abs(enemy.position.x - transform.position.x);

        float distanceY = Mathf.Abs(enemy.position.y - transform.position.y);

        bool inRange = (distanceX <= attackRangeX && distanceY <= attackRangeY);

        float releasedChargeTime = currentChargeTime;

        currentChargeTime = 0f;

        // ★1. 範囲外（空振り）の場合の処理を一番上に持ってくる

        if (!inRange)

        {

            if (airAttacksound != null && audioSource != null)

            {

                audioSource.PlayOneShot(airAttacksound);

            }

            // 空振りのときの硬直時間やモーションを設定したい場合はここで指定できます

            // 例として通常攻撃と同じ硬直・モーションにする場合：

            StartCoroutine(AttackCooldown(normalAttackCooldown));

            StartCoroutine(AttackAnimationRoutine(normalAttackSprite, normalRecoverySprite));

            return; // 処理をここで終了する

        }

        // ★2. 以降は敵が範囲内にいる場合の溜め段階ごとの処理

        float cooldownDuration = 0f;

        Sprite targetAttackSprite = null;

        Sprite targetRecoverySprite = null;

        if (releasedChargeTime >= maxChargeTime)

        {

            cooldownDuration = chargedAttackCooldown;

            targetAttackSprite = chargedAttackSprite;

            targetRecoverySprite = chargedRecoverySprite;

            ExecuteAttack(chargedPowerX, chargedPowerY, chargedHitStun);

            if (maxAttacksound != null && audioSource != null)

            {

                audioSource.PlayOneShot(maxAttacksound, 5.0f);

            }

        }

        else if (releasedChargeTime >= mediumChargeThreshold)

        {

            cooldownDuration = mediumAttackCooldown;

            targetAttackSprite = mediumAttackSprite;

            targetRecoverySprite = mediumRecoverySprite;

            ExecuteAttack(mediumPowerX, mediumPowerY, mediumHitStun);

            if (mediumAttacksound != null && audioSource != null)

            {

                audioSource.PlayOneShot(mediumAttacksound);

            }

        }

        else

        {
            cooldownDuration = normalAttackCooldown;

            targetAttackSprite = normalAttackSprite;

            targetRecoverySprite = normalRecoverySprite;

            ExecuteAttack(normalPowerX, normalPowerY, normalHitStun);

            if (weakAttacksound != null && audioSource != null)

            {

                audioSource.PlayOneShot(weakAttacksound);

            }

        }

        // 範囲内にヒットしたときの硬直とアニメーションを実行

        StartCoroutine(AttackCooldown(cooldownDuration));

        StartCoroutine(AttackAnimationRoutine(targetAttackSprite, targetRecoverySprite));

    }

    IEnumerator AttackAnimationRoutine(Sprite attackSprite, Sprite recoverySprite)

    {

        isAttacking = true;

        if (spriteRenderer != null && attackSprite != null)

        {

            spriteRenderer.sprite = attackSprite;

        }

        yield return new WaitForSeconds(0.1f);

        if (spriteRenderer != null && recoverySprite != null)

        {

            spriteRenderer.sprite = recoverySprite;

        }

        yield return new WaitForSeconds(0.15f);

        ResetVisual();

        isAttacking = false;

    }

    void ExecuteAttack(float power, float upPower, float hitStunDuration)

    {

        float directionX = 0f;

        if (enemy.position.x > transform.position.x)

        {

            directionX = power;

        }

        else

        {

            directionX = -power;

        }

        Controller enemyController = enemy.GetComponent<Controller>();

        if (enemyController != null)

        {

            enemyController.ReceiveKnockback(directionX, upPower, hitStunDuration);

        }

        // ★削除：ここでの一律0.5秒のコルーチン開始は削除し、ReleaseAttack側に統合しました

    }

    void ResetVisual()

    {

        if (spriteRenderer != null && defaultSprite != null)

        {

            spriteRenderer.sprite = defaultSprite;

        }

    }

    // ★変更：引数で硬直時間を受け取れるように変更

    IEnumerator AttackCooldown(float duration)

    {
        isAttackCooldown = true;

        // 自分のControllerを取得
        Controller myController = GetComponent<Controller>();

        if (myController != null)
        {
            myController.isAttackLocked = true;
        }

        yield return new WaitForSeconds(duration);

        if (myController != null)
        {
            myController.isAttackLocked = false;
        }

        isAttackCooldown = false;
    }

}