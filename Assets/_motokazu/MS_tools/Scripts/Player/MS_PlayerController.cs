using UnityEngine;
using UnityEngine.InputSystem; // Make sure to include this namespace

[RequireComponent(typeof(CharacterController))]
public class MS_PlayerController : MonoBehaviour
{
    [SerializeField] InputActionReference move;
    [SerializeField] InputActionReference speedUp;
    [SerializeField] float speedUpMuliplyer=2;
    private Vector2 moveInput;
    
    // Add other variables like speed, CharacterController reference, etc.
    [SerializeField] float speed = 0.1f;
    [SerializeField] float acceleration = 50f;
    [SerializeField] float rotationSpeed = 1.0f;
    [SerializeField] float gravity = -9.8f;

    // Animator
    [SerializeField] Animator animator;
    [SerializeField] float smoothMotion = 0.1f;
    float motionSpeed;

    private CharacterController characterController;
    private Vector3 currentMove = Vector3.zero;
    private float verticalVelocity;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        Mover();
        MotionControl();
    }


    private void Mover()
    {
        // get input
        moveInput = move.action.ReadValue<Vector2>();


        // カメラの方向から、X-Z平面の単位ベクトルを取得
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;

        // Basic movement
        //Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        // 方向キーの入力値とカメラの向きから、移動方向を決定
        Vector3 movement = Camera.main.transform.forward * moveInput.y + Camera.main.transform.right * moveInput.x;
        movement.y = 0;//一旦縦方向の移動を０に、ローテーションを安定させるため。
        
        //rotation
        if (movement.magnitude > 0)
        {
            Quaternion toRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * rotationSpeed);
        }

        //gravity
        if (!characterController.isGrounded)
        {
            verticalVelocity = verticalVelocity + gravity * Time.deltaTime;
            movement.y = verticalVelocity;//ここで重力を代入
        }

        // multiply speed
        Vector3 targetVelocity = movement * speed;
        // speedUp
        if (speedUp.action.IsPressed()) { targetVelocity *= speedUpMuliplyer; }

        // acceleration
        currentMove = Vector3.Lerp(currentMove, targetVelocity, Time.deltaTime * acceleration);
       

        // move
        characterController.Move(currentMove);

    }


    private void MotionControl()
    {
        motionSpeed = Mathf.Lerp(motionSpeed, characterController.velocity.magnitude, Time.deltaTime * smoothMotion);
        animator.SetFloat("Speed", motionSpeed);
    }
    public void SetTransform(Vector3 pos)
    {
        characterController.enabled = false;          // ← これが重要
        transform.position = pos;          // ここで直接設定
        transform.rotation = Quaternion.identity;
        characterController.enabled = true;           // 再有効化
    }
}
