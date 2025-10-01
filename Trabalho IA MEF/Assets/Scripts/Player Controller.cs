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
    [SerializeField]
    protected bool isAttacking = false;
    [SerializeField]
    protected bool isSliding = false;
    [SerializeField]
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
    

    void Start()
    {   currentSpeed = playerSpeed;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
       
         AnimatorStateInfo playerState = animator.GetCurrentAnimatorStateInfo(0);    
       
    }

    void FixedUpdate()
    {
        
    }
    void Slide()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && isRunning)
        {
            animator.SetTrigger("slided");
            currentSpeed = slideSpeed;
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
        isAttacking = Input.GetKey(KeyCode.P);
        if (isAttacking && !isSliding && !isRunning)
        {   
            animator.SetTrigger("slashed");
            
        }
    }
}
