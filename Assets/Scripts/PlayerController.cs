using UnityEngine;

// [필수] 이 스크립트는 CharacterController가 없으면 작동 안 함 (자동 추가됨)
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6.0f;
    [SerializeField] private float rotationSpeed = 1.0f; // 몸통 회전 속도
    [SerializeField] private float gravity = -9.81f;

    [Header("Camera")]
    [SerializeField] private Transform cameraRoot;  

    private IInputProvider _inputProvider;
    private CharacterController _characterController;
    private float _verticalVelocity;

    private void Awake() {
        _characterController = GetComponent<CharacterController>();
        _inputProvider = GetComponent<IInputProvider>();
    }

    private void Update() {
        if (_inputProvider == null) return;
        
        PlayerInputPacket input = _inputProvider.GetInput();

        cameraRoot.rotation = Quaternion.Euler(input.lookPitch, input.lookYaw, 0f);

        // 2-1. 시점 회전 (Look)
        Vector3 camForward = cameraRoot.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = cameraRoot.right;
        camRight.y = 0;
        camRight.Normalize();

        //Vector3 moveDirection = transform.forward * input.moveDir.y + transform.right * input.moveDir.x;
        //Vector3 moveDirection = (camForward * input.lookYaw + camRight * input.lookPitch).normalized;
        Vector3 moveDirection = (camForward * input.moveDir.y + camRight * input.moveDir.x).normalized;

        // 2-3. 실제로 이동할 때만 몸통 회전시키기
        if (moveDirection != Vector3.zero) {
            // 몸통이 나아갈 방향을 바라보게 함 (부드럽게 회전)
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }


        // 2-3. 중력 처리 (Gravity)
        if (_characterController.isGrounded && _verticalVelocity < 0) {
            _verticalVelocity = -2f; // 바닥에 붙어있을 때 살짝 눌러줌
        }
        _verticalVelocity += gravity * Time.deltaTime;

        // ====================================================
        // 3단계: 서빙하기 (실제 움직임 적용)
        // ====================================================
        Vector3 finalVelocity = (moveDirection * moveSpeed) + Vector3.up * _verticalVelocity;

        // CharacterController가 물리 충돌 계산해서 움직여줌
        _characterController.Move(finalVelocity * Time.deltaTime);

        // [디버그] 버튼 잘 눌리나 확인
        if (input.HasFlag(InputFlag.Dash)) Debug.Log("대시(Shift) 누름!");
        if (input.HasFlag(InputFlag.Attack)) Debug.Log("공격(클릭) 누름!");
    }
}