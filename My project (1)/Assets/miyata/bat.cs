
using System.Collections;
using UnityEngine;

public class bat : MonoBehaviour
{
    public bool player2 = false;

    [Header("攻撃範囲")]
    public float attackRangeX = 2f;
    public float attackRangeY = 1.5f;

    [Header("溜め段階")]
    public float mediumChargeThreshold = 2f;
    public float maxChargeTime = 4f;
    public float autoReleaseTime = 6f;

    [Header("通常攻撃")]
    public float normalPowerX = 10f;
    public float normalPowerY = 4f;
    public float normalHitStun = 0.4f;
    public float normalAttackCooldown = 0.3f;

    [Header("中溜め攻撃")]
    public float mediumPowerX = 15f;
    public float mediumPowerY = 6f;
    public float mediumHitStun = 0.8f;
    public float mediumAttackCooldown = 0.6f;

    [Header("強溜め攻撃")]
    public float chargedPowerX = 25f;
    public float chargedPowerY = 9f;
    public float chargedHitStun = 1.5f;
    public float chargedAttackCooldown = 1.2f;

    [Header("見た目")]
    public SpriteRenderer spriteRenderer;
    public Sprite defaultSprite;
    public Sprite startChargeSprite;
    public Sprite normalAttackSprite;
    public Sprite normalChargeSprite;
    public Sprite normalRecoverySprite;
    public Sprite mediumChargeSprite;
    public Sprite mediumAttackSprite;
    public Sprite mediumRecoverySprite;
    public Sprite maxChargeSprite;
    public Sprite chargedAttackSprite;
    public Sprite chargedRecoverySprite;

    [Header("攻撃対象")]
    public Transform enemy;
    public Transform zombieEnemy;

    [Header("攻撃状態")]
    public bool isAttackCooldown = false;
    public bool isAttacking = false;

    private float currentChargeTime = 0f;
    private bool isCharging = false;
    private AudioSource audioSource;

    [Header("攻撃サウンド")]
    public AudioClip weakAttacksound;
    public AudioClip mediumAttacksound;
    public AudioClip maxAttacksound;
    public AudioClip airAttacksound;

    [Header("金属バット")]
    public bool hasMetalBat = false;
    public int metalBatDurability = 3;
    public float metalPowerMultiplier = 1.5f;

    private Sprite woodenBatSprite;
    private SpriteRenderer batSpriteRenderer;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        batSpriteRenderer = GetComponent<SpriteRenderer>();

        if (batSpriteRenderer != null)
            woodenBatSprite = batSpriteRenderer.sprite;
    }

    private void Update()
    {
        Controller myController = GetComponent<Controller>();

        if (isAttackCooldown || isAttacking)
            return;

        if (myController != null &&
            (myController.isItemStunned || myController.isDead))
            return;

        KeyCode attackKey = player2
            ? KeyCode.DownArrow
            : KeyCode.S;

        KeyCode gamepadAttackKey = player2
            ? KeyCode.Joystick2Button5
            : KeyCode.JoystickButton5;

        bool keyDown =
            Input.GetKeyDown(attackKey) ||
            Input.GetKeyDown(gamepadAttackKey);

        bool keyHeld =
            Input.GetKey(attackKey) ||
            Input.GetKey(gamepadAttackKey);

        bool keyUp =
            Input.GetKeyUp(attackKey) ||
            Input.GetKeyUp(gamepadAttackKey);

        if (keyDown && !isCharging)
        {
            isCharging = true;
            currentChargeTime = 0f;

            if (spriteRenderer != null && startChargeSprite != null)
                spriteRenderer.sprite = startChargeSprite;
        }

        if (isCharging && keyHeld)
        {
            currentChargeTime += Time.deltaTime;
            UpdateChargeVisual();

            if (currentChargeTime >= autoReleaseTime)
            {
                currentChargeTime = autoReleaseTime;
                ReleaseAttack();
                return;
            }
        }

        if (isCharging && keyUp)
            ReleaseAttack();
    }

    private void UpdateChargeVisual()
    {
        if (spriteRenderer == null)
            return;

        if (currentChargeTime >= maxChargeTime)
        {
            if (maxChargeSprite != null)
                spriteRenderer.sprite = maxChargeSprite;
        }
        else if (currentChargeTime >= mediumChargeThreshold)
        {
            if (mediumChargeSprite != null)
                spriteRenderer.sprite = mediumChargeSprite;
        }
        else
        {
            if (startChargeSprite != null)
                spriteRenderer.sprite = startChargeSprite;
            else if (defaultSprite != null)
                spriteRenderer.sprite = defaultSprite;
        }
    }

    private float GetFacingDirection()
    {
        if (enemy != null)
            return enemy.position.x >= transform.position.x ? 1f : -1f;

        if (zombieEnemy != null)
            return zombieEnemy.position.x >= transform.position.x ? 1f : -1f;

        return player2 ? -1f : 1f;
    }

    private bool IsWoodBoxInRange()
    {
        float direction = GetFacingDirection();

        Vector2 center = new Vector2(
            transform.position.x + direction * attackRangeX / 2f,
            transform.position.y
        );

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            center,
            new Vector2(attackRangeX, attackRangeY * 2f),
            0f,
            LayerMask.GetMask("WoodBox")
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.GetComponent<WoodBox>() != null)
                return true;
        }

        return false;
    }

    private bool IsTargetInRange()
    {
        if (IsWoodBoxInRange())
            return true;

        if (zombieEnemy != null)
        {
            float dx = Mathf.Abs(
                zombieEnemy.position.x - transform.position.x
            );

            float dy = Mathf.Abs(
                zombieEnemy.position.y - transform.position.y
            );

            if (dx <= attackRangeX && dy <= attackRangeY)
                return true;
        }

        if (enemy != null)
        {
            float dx = Mathf.Abs(
                enemy.position.x - transform.position.x
            );

            float dy = Mathf.Abs(
                enemy.position.y - transform.position.y
            );

            if (dx <= attackRangeX && dy <= attackRangeY)
                return true;
        }

        return false;
    }

    private void ReleaseAttack()
    {
        isCharging = false;

        float releasedChargeTime = currentChargeTime;
        currentChargeTime = 0f;

        if (!IsTargetInRange())
        {
            if (airAttacksound != null && audioSource != null)
                audioSource.PlayOneShot(airAttacksound);

            StartCoroutine(AttackCooldown(normalAttackCooldown));
            StartCoroutine(
                AttackAnimationRoutine(
                    normalAttackSprite,
                    normalRecoverySprite
                )
            );
            return;
        }

        float cooldownDuration;
        float powerX;
        float powerY;
        float hitStun;
        Sprite attackSprite;
        Sprite recoverySprite;
        AudioClip attackSound;
        float soundVolume = 1f;

        if (releasedChargeTime >= maxChargeTime)
        {
            cooldownDuration = chargedAttackCooldown;
            powerX = chargedPowerX;
            powerY = chargedPowerY;
            hitStun = chargedHitStun;
            attackSprite = chargedAttackSprite;
            recoverySprite = chargedRecoverySprite;
            attackSound = maxAttacksound;
            soundVolume = 5f;
        }
        else if (releasedChargeTime >= mediumChargeThreshold)
        {
            cooldownDuration = mediumAttackCooldown;
            powerX = mediumPowerX;
            powerY = mediumPowerY;
            hitStun = mediumHitStun;
            attackSprite = mediumAttackSprite;
            recoverySprite = mediumRecoverySprite;
            attackSound = mediumAttacksound;
        }
        else
        {
            cooldownDuration = normalAttackCooldown;
            powerX = normalPowerX;
            powerY = normalPowerY;
            hitStun = normalHitStun;
            attackSprite = normalAttackSprite;
            recoverySprite = normalRecoverySprite;
            attackSound = weakAttacksound;
        }

        bool hit = ExecuteAttack(powerX, powerY, hitStun);

        if (hit && attackSound != null && audioSource != null)
            audioSource.PlayOneShot(attackSound, soundVolume);

        StartCoroutine(AttackCooldown(cooldownDuration));
        StartCoroutine(
            AttackAnimationRoutine(attackSprite, recoverySprite)
        );
    }

    private IEnumerator AttackAnimationRoutine(
        Sprite attackSprite,
        Sprite recoverySprite)
    {
        isAttacking = true;

        if (spriteRenderer != null && attackSprite != null)
            spriteRenderer.sprite = attackSprite;

        yield return new WaitForSeconds(0.1f);

        if (spriteRenderer != null && recoverySprite != null)
            spriteRenderer.sprite = recoverySprite;

        yield return new WaitForSeconds(0.15f);

        ResetVisual();
        isAttacking = false;
    }

    private bool ExecuteAttack(
        float power,
        float upPower,
        float hitStunDuration)
    {
        float finalPowerX = power;
        float finalPowerY = upPower;

        if (hasMetalBat)
        {
            finalPowerX *= metalPowerMultiplier;
            finalPowerY *= metalPowerMultiplier;
        }

        // ① 木箱への攻撃
        float facingDirection = GetFacingDirection();

        Vector2 boxCenter = new Vector2(
            transform.position.x + facingDirection * attackRangeX / 2f,
            transform.position.y
        );

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            boxCenter,
            new Vector2(attackRangeX, attackRangeY * 2f),
            0f,
            LayerMask.GetMask("WoodBox")
        );

        foreach (Collider2D hit in hits)
        {
            WoodBox box = hit.GetComponent<WoodBox>();

            if (box != null)
            {
                box.TakeDamage(1, this);
                return true;
            }
        }

        // ② ゾンビへの攻撃
        if (zombieEnemy != null)
        {
            float dx = Mathf.Abs(
                zombieEnemy.position.x - transform.position.x
            );

            float dy = Mathf.Abs(
                zombieEnemy.position.y - transform.position.y
            );

            if (dx <= attackRangeX && dy <= attackRangeY)
            {
                ZombieKnockback zombie =
                    zombieEnemy.GetComponent<ZombieKnockback>();

                if (zombie != null)
                {
                    float directionX =
                        zombieEnemy.position.x > transform.position.x
                        ? 1f
                        : -1f;

                    zombie.GetKnockedBack(
                        directionX * finalPowerX,
                        finalPowerY
                    );

                    OnSuccessfulHit();
                    return true;
                }
            }
        }

        // ③ 相手プレイヤーへの攻撃
        if (enemy != null)
        {
            float dx = Mathf.Abs(
                enemy.position.x - transform.position.x
            );

            float dy = Mathf.Abs(
                enemy.position.y - transform.position.y
            );

            if (dx <= attackRangeX && dy <= attackRangeY)
            {
                Controller enemyController =
                    enemy.GetComponent<Controller>();

                if (enemyController == null)
                    return false;

                // シールド判定：既存の処理を維持
                AutoGuardItem guard = enemy.GetComponent<AutoGuardItem>();

                if (guard != null && guard.TryBlock(GetComponent<Controller>()))
                {
                    // シールドに防がれた攻撃側を硬直させる
                    Controller attackerController = GetComponent<Controller>();

                    if (attackerController != null)
                    {
                        attackerController.ApplyItemStun(1.5f);
                    }

                    return false;
                }

                float directionX =
                    enemy.position.x > transform.position.x
                    ? 1f
                    : -1f;

                enemyController.ReceiveKnockback(
                    directionX * finalPowerX,
                    finalPowerY,
                    hitStunDuration
                );

                OnSuccessfulHit();
                return true;
            }
        }

        return false;
    }

    private void ResetVisual()
    {
        if (spriteRenderer != null && defaultSprite != null)
            spriteRenderer.sprite = defaultSprite;
    }

    private IEnumerator AttackCooldown(float duration)
    {
        isAttackCooldown = true;

        Controller myController = GetComponent<Controller>();

        if (myController != null)
            myController.isAttackLocked = true;

        yield return new WaitForSeconds(duration);

        if (myController != null)
            myController.isAttackLocked = false;

        isAttackCooldown = false;
    }
    // 木箱を壊したプレイヤーにランダムでアイテムを付与
    public void GetRandomItem()
    {
        int itemNumber = Random.Range(0, 2);

        switch (itemNumber)
        {
            case 0:
                // 金属バット
                EquipMetalBat();
                Debug.Log("金属バットを入手！");
                break;

            case 1:
                // オートガード
                AutoGuardItem guard = GetComponent<AutoGuardItem>();

                if (guard != null)
                {
                    guard.hasItem = true;

                    if (guard.shieldObject != null)
                        guard.shieldObject.SetActive(true);

                    Debug.Log("オートガードを入手！");
                }
                else
                {
                    Debug.LogWarning("AutoGuardItemがプレイヤーに付いていません。");
                }
                break;
        }
    }
    // 木箱を壊したプレイヤーが金属バットを入手
    public void EquipMetalBat()
    {
        hasMetalBat = true;
        metalBatDurability = 3;

    }

    // 敵に攻撃が命中したときだけ耐久値を減らす
    public void OnSuccessfulHit()
    {
        if (!hasMetalBat)
            return;

        metalBatDurability--;

        Debug.Log("金属バット残り耐久：" + metalBatDurability);

        if (metalBatDurability <= 0)
            BreakMetalBat();
    }

    private void BreakMetalBat()
    {
        hasMetalBat = false;
        metalBatDurability = 3;

        if (batSpriteRenderer != null && woodenBatSprite != null)
            batSpriteRenderer.sprite = woodenBatSprite;

        Debug.Log("金属バットが壊れて木製バットに戻った！");
    }
  
}

