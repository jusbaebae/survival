using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //스피드 조정 변수
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;

    private float applySpeed;

    [SerializeField]
    private float jumpForce;

    //상태 변수
    private bool isRun = false;
    private bool isCrouch = false;
    private bool isGround = true;

    //앉았을 때 얼마나 앉을지 변수
    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;

    //땅 착지 여부
    private CapsuleCollider capsuleCollider;

    //카메라 민감도
    [SerializeField]
    private float lookSensitivity;

    //카메라 한계
    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotationX = 0f;

    //컴포넌트
    [SerializeField]
    private Camera theCamera;

    private Rigidbody myRigid;


    void Start() {
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        applySpeed = walkSpeed;
        originPosY = theCamera.transform.localPosition.y;
        applyCrouchPosY = originPosY;
    }

    void Update()
    {
        IsGround();
        TryJump();
        TryRun();
        TryCrouch();
        CameraRotation();
        CharacterRotation();
    }
    void FixedUpdate() {
        Move();
    }

    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal");
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
    }

    private void CameraRotation()
    {
        //상하 카메라 회전
        float _xRotation = Input.GetAxisRaw("Mouse Y");
        float _cameraRotationX = _xRotation * lookSensitivity;
        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }

    private void CharacterRotation()
    {
        //좌우 캐릭터 회전
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
    }

    private void TryRun()
    {
        if (!isCrouch)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                Running();
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                RunningCancel();
            }
        }
        
    }

    private void Running()
    {
        isRun = true;
        applySpeed = runSpeed;
    }

    private void RunningCancel ()
    {
        isRun = false;
        applySpeed = walkSpeed;
    }

    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround && !isCrouch)
        {
            Jump();
        }
    }

    private void Jump()
    {
        myRigid.velocity = transform.up * jumpForce;
    }

    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
    }

    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && isGround)
        {
            Crouch();
        }
    }

    private void Crouch()
    {
        isCrouch = !isCrouch;

        if (isCrouch)
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }

        StartCoroutine("CrouchCoroutine");

    }

    IEnumerator CrouchCoroutine()
    {
        float currentY = theCamera.transform.localPosition.y;

        while (Mathf.Abs(currentY - applyCrouchPosY) > 0.01f)
        {
            currentY = Mathf.Lerp(currentY, applyCrouchPosY, 0.05f);
            Vector3 camPos = theCamera.transform.localPosition;
            theCamera.transform.localPosition = new Vector3(camPos.x, currentY, camPos.z);
            yield return null;
        }

        // 보간이 끝나면 정확하게 위치 고정
        Vector3 finalPos = theCamera.transform.localPosition;
        theCamera.transform.localPosition = new Vector3(finalPos.x, applyCrouchPosY, finalPos.z);

    }
}
