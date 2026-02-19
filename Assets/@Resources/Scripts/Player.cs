using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 3f;  // 전진 속도 (units per second)
    [SerializeField] private float horizontalSpeed = 15f;  // 좌우 이동 민감도 (units relative to screen width)
    [SerializeField] private float horizontalLimit = 100f;  // 좌우 이동 제한

    private Vector2 lastPointerPosition;
    private bool isDragging = false;

    private const float inputDeadzone = 0.5f; // 픽셀 단위

    // Knockback
    private Coroutine knockbackCoroutine;

    [Header("Item Pickup")]
    [Tooltip("Layer name for item objects that cause the player to grow when picked up")]
    [SerializeField] private string itemLayerName = "B_Item";
    [SerializeField] private float scaleMultiplier = 1.1f; // multiplicative scale when picking up an item
    [SerializeField] private float scaleSmoothDuration = 0.2f; // smooth scaling time; 0 => instant

    private Coroutine scaleCoroutine;

    void Update()
    {
        // 전진 이동 (월드 좌표계)
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.World);

        float pointerDeltaX = 0f;

        // 터치 입력
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastPointerPosition = touch.position;
                isDragging = true;
            }
            else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && isDragging)
            {
                // deltaPosition은 이번 프레임의 픽셀 이동량
                pointerDeltaX = touch.deltaPosition.x;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
        else // 마우스 입력 (에디터 테스트용)
        {
            if (Input.GetMouseButtonDown(0))
            {
                lastPointerPosition = Input.mousePosition;
                isDragging = true;
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                Vector2 current = Input.mousePosition;
                pointerDeltaX = current.x - lastPointerPosition.x;
                lastPointerPosition = current;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }

        // 데드존 필터
        if (Mathf.Abs(pointerDeltaX) <= inputDeadzone) return;

        // 화면 비율 기반 수평 이동량 계산
        float moveDistance = (pointerDeltaX / Screen.width) * horizontalSpeed;

        if (moveDistance != 0f)
        {
            Vector3 pos = transform.position;
            pos.x += moveDistance;
            pos.x = Mathf.Clamp(pos.x, -horizontalLimit, horizontalLimit);
            transform.position = pos;
        }
    }

    // Public method to perform a non-physics knockback animation.
    // 'displacement' is the world-space offset to apply (typically away from the collider),
    // 'duration' is how long the knockback animation lasts.
    public void ApplyKnockback(Vector3 displacement, float duration)
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = null;
        }

        // Prevent input while knockback plays
        isDragging = false;
        knockbackCoroutine = StartCoroutine(KnockbackCoroutine(displacement, duration));
    }

    private System.Collections.IEnumerator KnockbackCoroutine(Vector3 displacement, float duration)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + displacement;

        // Clamp horizontal target to limits
        targetPos.x = Mathf.Clamp(targetPos.x, -horizontalLimit, horizontalLimit);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // ease out cubic for nicer feel
            float ease = 1f - Mathf.Pow(1f - t, 3f);

            transform.position = Vector3.Lerp(startPos, targetPos, ease);
            yield return null;
        }

        transform.position = targetPos;
        knockbackCoroutine = null;
    }

    // Handle trigger pickups from items on the specified layer and obstacles on "Obstacle" layer
    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        int itemLayer = LayerMask.NameToLayer(itemLayerName);
        int obstacleLayer = LayerMask.NameToLayer("Obstacle");

        // Grow when picking up item
        if (itemLayer != -1 && other.gameObject.layer == itemLayer)
        {
            Vector3 targetScale = transform.localScale * scaleMultiplier;

            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
                scaleCoroutine = null;
            }

            if (scaleSmoothDuration > 0f)
                scaleCoroutine = StartCoroutine(ScaleCoroutine(transform.localScale, targetScale, scaleSmoothDuration));
            else
                transform.localScale = targetScale;

            Destroy(other.gameObject);
            return;
        }

        // Shrink when hitting obstacle
        if (obstacleLayer != -1 && other.gameObject.layer == obstacleLayer)
        {
            // Shrink by dividing by scaleMultiplier so same parameter works for grow/shrink
            Vector3 targetScale = transform.localScale / scaleMultiplier;

            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
                scaleCoroutine = null;
            }

            if (scaleSmoothDuration > 0f)
                scaleCoroutine = StartCoroutine(ScaleCoroutine(transform.localScale, targetScale, scaleSmoothDuration));
            else
                transform.localScale = targetScale;

            return;
        }
    }

    private System.Collections.IEnumerator ScaleCoroutine(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }
        transform.localScale = to;
        scaleCoroutine = null;
    }
}
