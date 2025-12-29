# Unity 프로젝트 코드 스타일 가이드

이 문서는 Unity 프로젝트 개발 시 따라야 할 코드 스타일과 규칙을 정의합니다.

## 프로젝트 환경
- Unity 버전: Unity 2021.3 LTS 이상
- C# 버전: `9.0`
- .NET Target: `.NET Standard 2.1`
- 스크립트 경로: `Assets/@Resources/Scripts/`

## 코드 스타일

### 네이밍 규칙
```csharp
// 클래스, 구조체, 열거형, 인터페이스
public class PlayerController { }
public struct PlayerData { }
public enum GameState { }
public interface IMovable { }

// 메서드 (동사로 시작)
public void MovePlayer() { }
public bool IsGrounded() { }

// 필드와 프로퍼티
private int _health;         // private 필드
[SerializeField] private float _speed;  // Inspector 노출 필드
public int Health { get; set; } // 프로퍼티
private const float MaxSpeed = 10f;     // 상수

// 로컬 변수와 매개변수
void Calculate(int damage)
{
    int currentHealth = 100;
}
```

### 파일 구조
```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RunnerGame.Player  // 네임스페이스 사용
{
    /// <summary>
    /// 플레이어 컨트롤러 클래스
  /// </summary>
    public class PlayerController : MonoBehaviour
    {
 #region Inspector Fields
        [Header("Movement Settings")]
   [SerializeField] private float _moveSpeed = 5f;
     [SerializeField] private float _jumpForce = 10f;
        #endregion

     #region Private Fields
        private Rigidbody _rb;
        private bool _isGrounded;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Start() { }
        private void Update() { }
     private void FixedUpdate() { }
 #endregion

        #region Public Methods
        public void Jump() { }
        #endregion

        #region Private Methods
        private void HandleMovement() { }
        #endregion
    }
}
```

## Unity 최적화 규칙

### 컴포넌트 캐싱
```csharp
// 좋은 예
private Transform _transform;
private void Awake()
{
    _transform = transform;  // 캐싱
}

// 나쁜 예
private void Update()
{
    transform.position += Vector3.up;  // 매 프레임 접근
}
```

### 메모리 관리
- `Update`에서 `new` 키워드 사용 금지
- 문자열 연결 시 `StringBuilder` 사용
- 컬렉션은 Clear() 후 재사용
- 오브젝트 풀링 적극 활용

### 물리 처리
```csharp
// 물리 연산은 FixedUpdate에서
private void FixedUpdate()
{
_rb.AddForce(Vector3.up * _jumpForce);
}

// Input은 Update에서 받고 플래그로 처리
private bool _shouldJump;
private void Update()
{
    if (Input.GetKeyDown(KeyCode.Space))
        _shouldJump = true;
}
```

## 폴더 구조
```
Assets/
├── @Resources/   # 런타임 로드 리소스
│   ├── Prefabs/
│   ├── Materials/
│   └── Audio/
├── Scripts/
│   ├── Player/
│   ├── Enemy/
│   ├── UI/
│   ├── Managers/
│   └── Utils/
├── Animations/
├── Models/
├── Textures/
└── Settings/
```

## Unity Inspector 규칙

### Attribute 사용
```csharp
[Header("Player Settings")]
[SerializeField] private float _health = 100f;

[Space(10)]
[Header("Movement")]
[SerializeField, Range(0, 10)] private float _speed = 5f;
[SerializeField, Tooltip("점프 높이")] private float _jumpHeight = 2f;

[HideInInspector] public bool isActive;
```

### ScriptableObject 활용
```csharp
[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    public float maxHealth = 100f;
    public float moveSpeed = 5f;
}
```

## 성능 최적화 체크리스트

### 반드시 피해야 할 것들
- [ ] Update에서 Find, GetComponent 호출
- [ ] 매 프레임 Instantiate/Destroy
- [ ] Update에서 문자열 연결
- [ ] Camera.main 반복 접근
- [ ] 불필요한 SendMessage 사용

### 권장 사항
- [ ] 오브젝트 풀 시스템 구현
- [ ] LOD(Level of Detail) 설정
- [ ] 오클루전 컬링 활용
- [ ] 배칭(Batching) 최적화
- [ ] 텍스처 아틀라스 사용

## 디버그와 로깅

### Debug 래퍼 클래스 사용
```csharp
public static class GameDebug
{
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Log(string message)
    {
        Debug.Log($"[Game] {message}");
    }
}
```

## Git 규칙

### .gitignore 필수 항목
```
/Library/
/Temp/
/Obj/
/Build/
/Builds/
*.pidb.meta
*.pdb.meta
*.mdb.meta
```

### 커밋 메시지 형식
```
[Type] 간단한 설명

Type:
- feat: 새 기능
- fix: 버그 수정
- docs: 문서 수정
- style: 코드 스타일 변경
- refactor: 리팩토링
- perf: 성능 개선
- test: 테스트 추가

예시: [feat] 플레이어 점프 기능 추가
```

## 코드 리뷰 체크포인트

### 필수 확인 사항
- [ ] null 체크 수행 여부
- [ ] 캐싱 가능한 컴포넌트 캐싱 여부
- [ ] 적절한 region 구분
- [ ] Inspector 필드 [SerializeField] 사용
- [ ] 네임스페이스 사용
- [ ] 불필요한 public 제거

## 테스트

### Unity Test Framework
```csharp
[Test]
public void PlayerHealth_WhenDamaged_ShouldDecrease()
{
    // Arrange
    var player = new GameObject().AddComponent<Player>();
    
 // Act
    player.TakeDamage(10);
    
    // Assert
    Assert.AreEqual(90, player.Health);
}
```

## 빌드 설정

### 플랫폼별 설정
- **PC/Mac/Linux**: IL2CPP 빌드
- **Mobile**: 코드 스트리핑 레벨 조정
- **WebGL**: 압축 설정 최적화

### 빌드 전 체크리스트
- [ ] Development Build 해제
- [ ] 불필요한 씬 제거
- [ ] 텍스처 압축 설정
- [ ] 오디오 압축 설정
- [ ] 빌드 세팅 최적화

---

**참고**: 이 가이드는 프로젝트의 성장에 따라 지속적으로 업데이트됩니다.
**최종 수정일**: 2024년