using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private const float MOVE_SPEED = 5.0f;

    [SerializeField] private Rigidbody rigidbody;

    private Vector3 moveDirection = Vector3.zero;

    public Vector3 CurrentVelocity { get; private set; }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector3(x, 0, z).normalized;
    }
    private void FixedUpdate()
    {
        Move();
    }
    private void Move()
    {
        if (rigidbody == null)
        {
            Debug.LogError("RigidbodyÇ™ê›íËÇ≥ÇÍÇƒÇ¢Ç‹ÇπÇÒÅB");
            return;
        }
        if (moveDirection == Vector3.zero)
        {
            rigidbody.linearVelocity = new Vector3(0f, rigidbody.linearVelocity.y, 0f);
            CurrentVelocity = Vector3.zero;
            return;
        }
        Vector3 targetVelocity = moveDirection * MOVE_SPEED;

        rigidbody.linearVelocity = new Vector3(targetVelocity.x, rigidbody.linearVelocity.y, targetVelocity.z);

        CurrentVelocity = rigidbody.linearVelocity;
    }
}
