using UnityEngine;

public class EnemyTouchDamage : MonoBehaviour
{
    [SerializeField] private float knockbackX = 8f;
    [SerializeField] private float knockbackY = 10f;
    [SerializeField] private float hitStunDuration = 0f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        Controller player = collision.GetComponent<Controller>();
        if (player == null) return;

        // ゾンビから離れる方向に吹っ飛ばす（元の処理）
        float direction =
            collision.transform.position.x >= transform.position.x
            ? 1f
            : -1f;

        player.ReceiveKnockback(
            direction * knockbackX,
            knockbackY,
            hitStunDuration
        );
    }
}