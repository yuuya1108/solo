using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Controller controller = collision.GetComponent<Controller>();

            if (controller != null)
            {
                controller.StartCoroutine(controller.SpeedUp());

                Debug.Log("10秒間スピードが1.05倍になりました！");
            }

            Destroy(gameObject);
        }
    }
}