
using UnityEngine;

public class MetalBatItem : MonoBehaviour
{
    [Header("アイテム設定")]
    public bool hasMetalBat = true;

    [Header("装備時の見た目")]
    public GameObject MetalObject;

    private void Start()
    {
        // 最初は金属バットの見た目を非表示
        if (MetalObject != null)
        {
            MetalObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (!hasMetalBat)
            return;

        bat playerBat = collision.GetComponent<bat>();

        if (playerBat == null)
            return;

        // 金属バットを装備
        playerBat.EquipMetalBat();

        // 金属バットの見た目を表示
        if (MetalObject != null)
        {
            MetalObject.SetActive(true);
        }

        // 拾ったアイテム本体を削除
        Destroy(gameObject);
    }
}
