using UnityEngine;

public class GroundMove : MonoBehaviour
{
    private float speed = 0f;
    private float distance = 10f;

    private float startX;

    void Start()
    {
        startX = transform.position.x;
    }

    void Update()
    {
        float x = Mathf.PingPong(Time.time * speed, distance * 2) - distance;

        transform.position = new Vector3(
            startX + x,
            transform.position.y,
            transform.position.z
        );
    }
}