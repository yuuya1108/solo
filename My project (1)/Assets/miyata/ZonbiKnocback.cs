
using UnityEngine;

public class ZombieKnockback : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isKnockedBack = false;

    [Header("’…’n”»’è")]
    public LayerMask groundLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void GetKnockedBack(float powerX, float powerY)
    {
        if (rb == null) return;

        isKnockedBack = true;

        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = new Vector2(powerX, powerY);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isKnockedBack) return;

        // ’n–Ê‚É’…’n‚µ‚½‚çd’¼‰ğœ
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isKnockedBack = false;
        }
    }

    public bool IsKnockedBack()
    {
        return isKnockedBack;
    }
}
