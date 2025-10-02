using UnityEngine;

public class DmageTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
       Player player = collision.gameObject.GetComponent<Player>();

        if (player != null)
        player.KnockBack();
    }
}
