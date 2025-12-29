using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3.0f;
    public float runSpeed = 6.0f;
    public float jumpForce = 5.0f;

    [Header("References")]
    public Animator _animator;
    public SpriteRenderer _spriteRenderer;

    private Rigidbody rb;
    private Vector3 moveInput;
    private Vector3 lastMoveInput = Vector3.zero;

    private bool isGrounded = true;
    private bool runInput = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (_animator == null)
            _animator = GetComponent<Animator>();

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        HandleInput();

        UpdateRunInput();
        UpdateSpriteFlip();

        JumpInput();

        UpdateAnimation();

        // if (Input.anyKeyDown) Debug.Log("KeyDown detected");
    }

    private void FixedUpdate()
    {
        Move();
        ApplyFallMultiplier();
    }

    // ─────────────────
    void HandleInput()
    {
        float _ho = Input.GetAxisRaw("Horizontal");
        float _ver = Input.GetAxisRaw("Vertical");

        moveInput = new Vector3(_ho, 0, _ver).normalized;
    }

    void Move()
    {
        // 유효한 입력이 없다면 이동 시키지않음
        if (moveInput.magnitude < 0.01f) return;

        float speed = runInput ? runSpeed : walkSpeed;
        Vector3 velocity = moveInput * speed; // runSpeed 또는 walkSpeed 적용

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    void UpdateRunInput()
    {
        runInput = Input.GetKey(KeyCode.LeftShift);
    }

    void JumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            isGrounded = false;
            _animator.SetTrigger("isJump");
        }
    }

    public void DoJumpForce()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }

    void ApplyFallMultiplier()
    {
        float fallMultiplier = 2.5f; 

        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        // Ground tag가 붙어있는 바닥(Plane 혹은 Prefab)에 닿으면 착지
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void UpdateSpriteFlip()
    {
        // 좌우 방향에만 반응
        if (moveInput.x > 0)
            _spriteRenderer.flipX = false;
        else if (moveInput.x < 0)
            _spriteRenderer.flipX = true;
    }

    void UpdateAnimation()
    {
        // moveInput이 0에 가까우면 마지막 입력 유지
        Vector3 effectiveInput = moveInput.sqrMagnitude > 0.001f ? moveInput : lastMoveInput;
        lastMoveInput = effectiveInput;

        bool isMoving = moveInput.sqrMagnitude > 0.001f;
        bool isRunning = runInput && isMoving;

        //Debug.Log($"isMoving: {isMoving}, LeftShift: {Input.GetKey(KeyCode.LeftShift)}, isGrounded: {isGrounded}, isRunning: {isRunning}");

        if (_animator != null)
        {
            _animator.SetBool("isIdle", !isMoving && isGrounded);
            _animator.SetBool("isWalk", isMoving && !isRunning && isGrounded);
            _animator.SetBool("isRun", isRunning && isGrounded);
        }
    }

}