using UnityEngine;
public class FallZone : MonoBehaviour
{
    // トリガー（見えない床）に何かが触れた時に実行される
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 触れたオブジェクトのタグが "Enemy" だったら
        if (collision.CompareTag("Enemy"))
        {
            // そのオブジェクトをゲーム内から消去する
            Destroy(collision.gameObject);
        }
    }
}