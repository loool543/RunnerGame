using UnityEngine;

// Moves the rock toward z = 0 and applies a rolling rotation. Destroys itself when close to z=0.
public class Rock : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f; // units per second
    [SerializeField] private float destroyDistanceToZero = 0.5f; // distance to z=0 at which the rock is destroyed

    [Header("Rotation")]
    [SerializeField] private Vector3 rotationAxis = new Vector3(1f, 0f, 0f);
    [SerializeField] private float rotationSpeed = 360f; // degrees per second

    [Header("Impact")]
    [SerializeField] private float knockbackHorizontal = 1f; // sideways displacement applied to player
    [SerializeField] private float knockbackBackward = 2f;   // backward (negative z) displacement applied to player
    [SerializeField] private float knockbackDuration = 0.3f;

    // Expose read-only properties so Player can query knockback values when using trigger-based handling
    public float KnockbackHorizontal => knockbackHorizontal;
    public float KnockbackBackward => knockbackBackward;
    public float KnockbackDuration => knockbackDuration;

    private Rigidbody rb;

    void Start()
    {
        // Ensure there's a Collider on the rock; warn if missing
        var col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning("Rock: No Collider found on rock prefab. Collision detection requires a Collider.");
        }

        // If the collider is a trigger, do not add a Rigidbody here; trigger events will work if the player has a Rigidbody.
        if (col != null && col.isTrigger)
        {
            rb = GetComponent<Rigidbody>();
            // If there's a Rigidbody on a trigger rock, keep it but ensure gravity is disabled
            if (rb != null)
            {
                rb.useGravity = false;
            }
            return;
        }

        // For non-trigger colliders, ensure we have a non-kinematic Rigidbody so collision callbacks work reliably
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        else
        {
            rb.useGravity = false;
            if (rb.isKinematic)
                rb.isKinematic = false;
        }
    }

    void Update()
    {
        // Move only along Z toward 0
        float step = moveSpeed * Time.deltaTime;
        Vector3 pos = transform.position;
        float newZ = Mathf.MoveTowards(pos.z, 0f, step);
        Vector3 targetPos = new Vector3(pos.x, pos.y, newZ);

        if (rb != null)
        {
            rb.MovePosition(targetPos);
        }
        else
        {
            transform.position = targetPos;
        }

        // Simple rolling rotation (visual)
        transform.Rotate(rotationAxis.normalized, rotationSpeed * Time.deltaTime, Space.Self);

        // Destroy when close enough to z = 0
        if (Mathf.Abs(newZ) <= destroyDistanceToZero)
        {
            Destroy(gameObject);
        }
    }

    private void HandlePlayerHit(GameObject other)
    {
        if (other == null) return;

        // Try to find Player script on the collided object or its parents
        Player player = other.GetComponent<Player>() ?? other.GetComponentInParent<Player>();
        if (player == null) return;

        // Compute displacement: push sideways away from rock and backward along -z
        float side = Mathf.Sign(other.transform.position.x - transform.position.x);
        Vector3 displacement = new Vector3(side * knockbackHorizontal, 0f, -Mathf.Abs(knockbackBackward));

        player.ApplyKnockback(displacement, knockbackDuration);

        // Destroy rock after applying knockback
        Destroy(gameObject);
    }

    // If collider is set as trigger on the prefab
    void OnTriggerEnter(Collider other)
    {
        HandlePlayerHit(other.gameObject);
    }

    // If colliders are non-trigger
    void OnCollisionEnter(Collision collision)
    {
        HandlePlayerHit(collision.gameObject);
    }
}
