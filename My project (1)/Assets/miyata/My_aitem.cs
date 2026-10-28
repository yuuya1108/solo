
using UnityEngine;

public class MetalBatItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        bat playerBat = collision.GetComponent<bat>();

        if (playerBat == null)
            return;

        playerBat.EquipMetalBat();

        Destroy(gameObject);
    }
}
