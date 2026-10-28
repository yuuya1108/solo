using System.Collections;
using UnityEngine;

public class WoodBox : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private int maxHp = 2;          // 最大耐久値
    [SerializeField] private float respawnTime = 5f; // 復活までの時間（秒）

    [Header("画像の設定")]
    [SerializeField] private Sprite normalSprite;    // 通常時の画像
    [SerializeField] private Sprite crackedSprite;   // ヒビが入った時の画像

    private int currentHp;
    private SpriteRenderer spriteRenderer;
    private Collider2D boxCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<Collider2D>();

        // 最初にもし通常画像が未設定なら、現在の画像を登録しておく
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
        boxCollider.enabled = false;

        // 指定した秒数だけ待つ
        yield return new WaitForSeconds(respawnTime);

        // 木箱を復活させる
        ResetBox();
    }
    // 外部（bat.cs）から呼び出されるダメージ関数
    public void TakeDamage(int damage, bat attacker)
{
    if (currentHp <= 0 || !spriteRenderer.enabled)
        return;

    currentHp -= damage;

    if (currentHp == 1 && crackedSprite != null)
    {
        spriteRenderer.sprite = crackedSprite;
    }

    if (currentHp <= 0)
    {
        if (attacker != null)
        {
            attacker.EquipMetalBat();
        }

        StartCoroutine(DestroyAndRespawnRoutine());
    }
}

    // 木箱の状態を初期化する（復活時にも呼ばれる）
    private void ResetBox()
    {
        currentHp = maxHp;
        spriteRenderer.enabled = true;
        if (boxCollider != null) boxCollider.enabled = true;

        // 【★追加】復活したときは綺麗な通常画像に戻す
        if (normalSprite != null)
        {
            spriteRenderer.sprite = normalSprite;
        }
    }
}
