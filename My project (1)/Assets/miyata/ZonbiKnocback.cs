
using UnityEngine;

public class ZombieKnockback : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void GetKnockedBack(float powerX, float powerY)
    {
        if (rb == null) return;

        rb.linearVelocity = new Vector2(powerX, powerY);
    }
}
