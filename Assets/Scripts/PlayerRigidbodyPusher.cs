using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerRigidbodyPusher : MonoBehaviour
{
    [SerializeField] private float pushPower = 2.5f;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic || hit.moveDirection.y < -0.3f)
        {
            return;
        }

        Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
        body.AddForce(pushDirection * pushPower, ForceMode.Impulse);
    }
}
