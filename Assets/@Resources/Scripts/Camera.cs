using UnityEngine;

public class Camera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform player;  // 플레이어 Transform
    [SerializeField] private Vector3 offset = new Vector3(0, 7, -6);  // 플레이어로부터의 거리
    [SerializeField] private Vector3 cameraRotation = new Vector3(37, 0, 0);  // 카메라 회전 각도
    [SerializeField] private float smoothSpeed = 10f;  // 부드러운 따라가기 속도

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 카메라 초기 회전 설정
        transform.rotation = Quaternion.Euler(cameraRotation);
        
        // 플레이어가 설정되지 않았다면 자동으로 찾기
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (player != null)
        {
            // 플레이어의 z 위치만 따라가고, x는 고정 (좌우 움직임 무시)
            Vector3 desiredPosition = new Vector3(0, player.position.y + offset.y, player.position.z + offset.z);
            
            // 부드럽게 이동
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
            
            // 회전은 고정 유지
            transform.rotation = Quaternion.Euler(cameraRotation);
        }
    }
}
