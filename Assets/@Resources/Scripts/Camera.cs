using UnityEngine;

public class Camera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform player;  // 플레이어 Transform
    [SerializeField] private Vector3 offset = new Vector3(0, 7, -6);  // 플레이어로부터의 거리 (base offset)
    [SerializeField] private Vector3 cameraRotation = new Vector3(37, 0, 0);  // 카메라 회전 각도
    [SerializeField] private float smoothSpeed = 10f;  // 부드러운 따라가기 속도

    [Header("Auto Zoom on Scale")]
    [Tooltip("How many world units the camera should move further back (negative Z) per +1.0 player scale above 1.0.")]
    [SerializeField] private float zoomZPerScale = 0.5f;
    [Tooltip("How many world units the camera should move up per +1.0 player scale above 1.0.")]
    [SerializeField] private float zoomYPerScale = 2f;
    [Tooltip("Maximum additional backward movement (positive number).")]
    [SerializeField] private float maxAdditionalZ = 30f;
    [Tooltip("Maximum additional upward movement (positive number).")]
    [SerializeField] private float maxAdditionalY = 30f;

    // store the original offsets so inspector edits are the base
    private Vector3 m_BaseOffset;
    private float m_PlayerBaseY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 카메라 초기 회전 설정
        transform.rotation = Quaternion.Euler(cameraRotation);
        m_BaseOffset = offset;

        // 플레이어가 설정되지 않았다면 자동으로 찾기
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        // store player's base Y so scaling (which may change model pivot) doesn't shift camera vertically
        if (player != null)
            m_PlayerBaseY = player.position.y;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (player == null) return;

        // base follow: keep X fixed at base offset.x, follow player's Z and Y with base offsets
        // compute scale-based extra zoom only when player is larger than base (scale > 1)
        float scaleFactor = (player.localScale.x + player.localScale.y + player.localScale.z) / 3f;
        float extraZ = 0f;
        float extraY = 0f;

        if (scaleFactor > 1f)
        {
            extraZ = Mathf.Clamp((scaleFactor - 1f) * zoomZPerScale, 0f, maxAdditionalZ);
            extraY = Mathf.Clamp((scaleFactor - 1f) * zoomYPerScale, 0f, maxAdditionalY);
        }

        // m_BaseOffset.z is expected to be negative (camera behind player). Move further back by subtracting extraZ.
        float dynamicZ = m_BaseOffset.z - extraZ;
        float dynamicY = m_BaseOffset.y + extraY;

        Vector3 desiredPosition = new Vector3(m_BaseOffset.x, m_PlayerBaseY + dynamicY, player.position.z + dynamicZ);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(cameraRotation);
    }
}
