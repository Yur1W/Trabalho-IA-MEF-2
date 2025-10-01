using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{   
    //componentes
    protected Animator animator;
    protected Rigidbody2D rb;
    protected Collider2D coll;
    [Header("Player States")]
    [SerializeField]
    protected bool isRunning = false;
    protected bool isAttacking = false;
    protected bool isSliding = false;
    protected bool isGrounded = true;

    [Header("Player Values")]
    [SerializeField]
    protected float playerSpeed = 5f;
    [SerializeField]
    protected float currentSpeed;
    protected float JumpForce = 14f;
    [SerializeField]
    protected float slideSpeed = 8f;
    [SerializeField]
    protected float slideTime = 0.5f;

    [SerializeField]
    protected enum playerState { idle, running, jumping, falling, attacking, sliding, firing, hurt, throwing }
    protected playerState state = playerState.idle; 

    void Start()
    {   currentSpeed = playerSpeed;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
       switch(state)
       {
            case playerState.idle:
                Movement();
                break;
       }
    }
    void Slide()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && isRunning)
        {
            animator.SetTrigger("slided");
            currentSpeed = slideSpeed;
            state = playerState.sliding;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            currentSpeed = playerSpeed;
        }

    }

    void Movement()
    {

        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
        float moveInput = Input.GetAxisRaw("Horizontal");
        transform.position += movement * currentSpeed * Time.deltaTime;

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
    void slash()
    {
        isAttacking = Input.GetKeyDown(KeyCode.P);
        if (isAttacking && !isSliding && !isRunning)
        {
            isAttacking = true;
            animator.SetTrigger("slashed");
            state = playerState.attacking;
        }
    }
}
