using UnityEngine;

public class PlayerController : MonoBehaviour
{
    protected Animator animator;
    protected Rigidbody2D rb;
    protected Collider2D coll;
    [Header("Player States")]
    [SerializeField]
    protected bool isRunning = false;

    [Header("Player Values")]
    [SerializeField]
    protected float playerSpeed = 5f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Slide();
    }
    void Slide()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && isRunning)
        {
            animator.SetTrigger("slided");
        }

    }

    void Movement()
    {
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * playerSpeed, rb.linearVelocityY);

        if (moveInput != 0)
        {
            isRunning = true;
            animator.SetBool("isRunning", true);
        }
        else
        {
            isRunning = false;
            animator.SetBool("isRunning", false);
        }
    }
}
