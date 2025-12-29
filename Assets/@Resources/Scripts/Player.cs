using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 10f;  // 전진 속도
    [SerializeField] private float horizontalSpeed = 15f;  // 좌우 이동 속도
    [SerializeField] private float horizontalLimit = 100f;  // 좌우 이동 제한

    private Vector3 touchStartPosition;
    private bool isDragging = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 전진 이동
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);

        // 터치/마우스 입력 처리
        HandleInput();
    }

    private void HandleInput()
    {
        // 모바일 터치 입력
        if (Input.touchCount > 0)
        {

            Touch touch = Input.GetTouch(0);
       
           if (touch.phase == TouchPhase.Began)
            {
               touchStartPosition = touch.position;
               isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                float deltaX = touch.position.x - touchStartPosition.x;
                // 화면 너비에 대한 비율로 계산하여 이동 거리 결정
                 float moveDistance = (deltaX / Screen.width) * horizontalSpeed;
   
                Vector3 newPosition = transform.position;
                newPosition.x += moveDistance;
                newPosition.x = Mathf.Clamp(newPosition.x, -horizontalLimit, horizontalLimit);
                transform.position = newPosition;
            
                touchStartPosition = touch.position;
             }
             else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
             { 
                isDragging = false;
             }
        }
        // 에디터에서 테스트용 마우스 입력
        else
        {
            if (Input.GetMouseButtonDown(0))
             {
                touchStartPosition = Input.mousePosition;
                isDragging = true;
             }
            else if (Input.GetMouseButton(0) && isDragging)
            {

                float deltaX = Input.mousePosition.x - touchStartPosition.x;
                // 화면 너비에 대한 비율로 계산하여 이동 거리 결정
                float moveDistance = (deltaX / Screen.width) * horizontalSpeed;
       
                Vector3 newPosition = transform.position;
                newPosition.x += moveDistance;
                newPosition.x = Mathf.Clamp(newPosition.x, -horizontalLimit, horizontalLimit);
                transform.position = newPosition;
    
                touchStartPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }
    }
}
