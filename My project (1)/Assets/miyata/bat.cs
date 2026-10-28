
using System.Collections;
using UnityEngine;

public class bat : MonoBehaviour
{
    public bool player2 = false;

    [Header("攻撃範囲の設定")]
    public float attackRangeX = 3f;
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
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        batSpriteRenderer = GetComponent<SpriteRenderer>();

        if (batSpriteRenderer != null)
        {
            woodenBatSprite = batSpriteRenderer.sprite;
        }
    }

    private void Update()
    {
        if (isAttackCooldown || isAttacking)
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
            {
                spriteRenderer.sprite = startChargeSprite;
            }
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
        {
            ReleaseAttack();
        }
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

    // 攻撃方向
    private float GetFacingDirection()
    {
        if (enemy != null)
        {
            return enemy.position.x >= transform.position.x ? 1f : -1f;
        }

        if (zombieEnemy != null)
        {
            return zombieEnemy.position.x >= transform.position.x ? 1f : -1f;
        }

        return player2 ? -1f : 1f;
    }

    // 木箱が攻撃範囲にあるか確認
    private bool IsWoodBoxInRange()
    {
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
            if (hit.GetComponent<WoodBox>() != null)
                return true;
        }

        return false;
    }

    // 攻撃対象が範囲内にいるか確認
    private bool IsTargetInRange()
    {
        // 木箱
        if (IsWoodBoxInRange())
            return true;

        // ゾンビ
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

        // 相手プレイヤー
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

        // 木箱・ゾンビ・プレイヤーのいずれかが範囲内なら攻撃
        if (!IsTargetInRange())
        {
            if (airAttacksound != null && audioSource != null)
            {
                audioSource.PlayOneShot(airAttacksound);
            }

            StartCoroutine(AttackCooldown(normalAttackCooldown));
            StartCoroutine(
                AttackAnimationRoutine(normalAttackSprite, normalRecoverySprite)
            );
            return;
        }

        float cooldownDuration;
        Sprite targetAttackSprite;
        Sprite targetRecoverySprite;

        if (releasedChargeTime >= maxChargeTime)
        {
            cooldownDuration = chargedAttackCooldown;
            targetAttackSprite = chargedAttackSprite;
            targetRecoverySprite = chargedRecoverySprite;

            ExecuteAttack(chargedPowerX, chargedPowerY, chargedHitStun);

            if (maxAttacksound != null && audioSource != null)
                audioSource.PlayOneShot(maxAttacksound, 5f);
        }
        else if (releasedChargeTime >= mediumChargeThreshold)
        {
            cooldownDuration = mediumAttackCooldown;
            targetAttackSprite = mediumAttackSprite;
            targetRecoverySprite = mediumRecoverySprite;

            ExecuteAttack(mediumPowerX, mediumPowerY, mediumHitStun);

            if (mediumAttacksound != null && audioSource != null)
                audioSource.PlayOneShot(mediumAttacksound);
        }
        else
        {
            cooldownDuration = normalAttackCooldown;
            targetAttackSprite = normalAttackSprite;
            targetRecoverySprite = normalRecoverySprite;

            ExecuteAttack(normalPowerX, normalPowerY, normalHitStun);

            if (weakAttacksound != null && audioSource != null)
                audioSource.PlayOneShot(weakAttacksound);
        }

        StartCoroutine(AttackCooldown(cooldownDuration));
        StartCoroutine(
            AttackAnimationRoutine(targetAttackSprite, targetRecoverySprite)
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

    private void ExecuteAttack(
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

        // ① 木箱を攻撃
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
                // このプレイヤーを木箱に渡す
                box.TakeDamage(1, this);
                return;
            }
        }

        // ② ゾンビを攻撃
        if (zombieEnemy != null)
        {
            float zombieDistanceX =
                Mathf.Abs(zombieEnemy.position.x - transform.position.x);

            float zombieDistanceY =
                Mathf.Abs(zombieEnemy.position.y - transform.position.y);

            if (zombieDistanceX <= attackRangeX &&
                zombieDistanceY <= attackRangeY)
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
                    return;
                }
            }
        }

        // ③ 相手プレイヤーを攻撃
        if (enemy != null)
        {
            float directionX =
                enemy.position.x > transform.position.x
                ? 1f
                : -1f;

            Controller enemyController =
                enemy.GetComponent<Controller>();

            if (enemyController != null)
            {
                enemyController.ReceiveKnockback(
                    directionX * finalPowerX,
                    finalPowerY,
                    hitStunDuration
                );

                OnSuccessfulHit();
            }
        }
    }

    private void ResetVisual()
    {
        if (spriteRenderer != null && defaultSprite != null)
        {
            spriteRenderer.sprite = defaultSprite;
        }
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

    // 木箱を壊したプレイヤーが呼び出す入手処理
    public void EquipMetalBat()
    {
        hasMetalBat = true;
        metalBatDurability = 3;

        Debug.Log("金属バットを入手！");
    }

    // 金属バットが敵などに命中したときだけ耐久値を減らす
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
        {
            batSpriteRenderer.sprite = woodenBatSprite;
        }

        Debug.Log("金属バットが壊れて木製バットに戻った！");
    }
}
