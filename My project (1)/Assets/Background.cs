
using UnityEngine;

public class BackgroundLoop : MonoBehaviour
{
    public Camera targetCamera;
    public SpriteRenderer background;

    private Transform copy;
    private float width;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        width = background.bounds.size.x;

        // îwåiÇé©ìÆÇ≈1Ç¬ï°êªÇ∑ÇÈ
        GameObject copyObject = Instantiate(
            background.gameObject,
            background.transform.position + Vector3.right * width,
            background.transform.rotation
        );

        copyObject.name = "Background_Copy";
        copy = copyObject.transform;
    }

    void LateUpdate()
    {
        float cameraX = targetCamera.transform.position.x;
        float halfView = targetCamera.orthographicSize
                         * targetCamera.aspect;

        // å≥ÇÃîwåiÇ™âÊñ ÇÃç∂ë§Ç©ÇÁäOÇÍÇΩÇÁâEÇ÷à⁄ìÆ
        if (background.transform.position.x + width / 2
            < cameraX - halfView)
        {
            background.transform.position =
                new Vector3(
                    copy.position.x + width,
                    background.transform.position.y,
                    background.transform.position.z
                );
        }

        // å≥ÇÃîwåiÇ™âÊñ ÇÃâEë§Ç©ÇÁäOÇÍÇΩÇÁç∂Ç÷à⁄ìÆ
        if (background.transform.position.x - width / 2
            > cameraX + halfView)
        {
            background.transform.position =
                new Vector3(
                    copy.position.x - width,
                    background.transform.position.y,
                    background.transform.position.z
                );
        }

        // ï°êªÇµÇΩîwåiÇ‡ìØÇ∂ÇÊÇ§Ç…à⁄ìÆÇ∑ÇÈ
        if (copy.position.x + width / 2 < cameraX - halfView)
        {
            copy.position = new Vector3(
                background.transform.position.x + width,
                copy.position.y,
                copy.position.z
            );
        }

        if (copy.position.x - width / 2 > cameraX + halfView)
        {
            copy.position = new Vector3(
                background.transform.position.x - width,
                copy.position.y,
                copy.position.z
            );
        }
    }
}