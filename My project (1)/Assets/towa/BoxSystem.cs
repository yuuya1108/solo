
using System.Collections;
using UnityEngine;

public class WoodBox : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private int maxHp = 2;
    [SerializeField] private float respawnTime = 5f;

    [Header("画像の設定")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite crackedSprite;

    private int currentHp;
    private SpriteRenderer spriteRenderer;
    private Collider2D boxCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<Collider2D>();

        // 通常画像が未設定なら、現在の画像を登録
        if (normalSprite == null && spriteRenderer != null)
        {
            normalSprite = spriteRenderer.sprite;
        }

        ResetBox();
    }

    private IEnumerator DestroyAndRespawnRoutine()
    {
        // 木箱を一時的に非表示にする
        spriteRenderer.enabled = false;

        if (boxCollider != null)
            boxCollider.enabled = false;

        // 復活まで待つ
        yield return new WaitForSeconds(respawnTime);

        // 木箱を復活させる
        ResetBox();
    }

    // bat.cs から呼び出されるダメージ処理
    public void TakeDamage(int damage, bat attacker)
    {
        if (currentHp <= 0 || !spriteRenderer.enabled)
            return;

        currentHp -= damage;

        // ヒビの入った画像に変更
        if (currentHp == 1 && crackedSprite != null)
        {
            spriteRenderer.sprite = crackedSprite;
        }

        // 木箱が壊れたとき
        if (currentHp <= 0)
        {
            if (attacker != null)
            {
                // 壊したプレイヤーにランダムでアイテムを付与
                attacker.GetRandomItem();
            }

            StartCoroutine(DestroyAndRespawnRoutine());
        }
    }

    // 木箱の状態を初期化
    private void ResetBox()
    {
        currentHp = maxHp;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;

            if (normalSprite != null)
                spriteRenderer.sprite = normalSprite;
        }

        if (boxCollider != null)
            boxCollider.enabled = true;
    }
}
