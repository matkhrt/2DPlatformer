using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField]
    private float speed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
}
