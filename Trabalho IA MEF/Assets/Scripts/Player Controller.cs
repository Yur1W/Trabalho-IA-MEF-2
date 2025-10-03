using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

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

    [Header("Movimenta��o Base")]
    Vector3 moviment = new Vector3();
    [SerializeField] float speed = 3;

    [Header("Pulo")]
    [SerializeField] float forcapulo;
    [SerializeField] bool podepularemdobro;

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
    
    [Header("Checks")]
    bool inputpulo;
    [SerializeField] float rccheckachao;
    [SerializeField] float rccheckaescalada;
    float movehorizontalInput;
    float moveverticalInput;
    bool inputWallClimb;
    bool inputSlide;
    bool inputSlash;

    [SerializeField]
    enum playerState { idle, running, jumping, falling, attacking, sliding, firing, hurt, throwing, climbing} 
    [SerializeField]
    playerState state = playerState.idle;

    
    void Awake()
    {
        //determinando a variavel que ser� usada para:
        //fazer as transi��es entre as anima��es
        animator = GetComponent<Animator>();
        //aplicar a f�sica
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {  
        
       
      
    }

    private void FixedUpdate()
    {
        
        
        //vetor fixo de movimenta��o
        moviment = new Vector3(movehorizontalInput, moveverticalInput, 0f);
        moviment.Normalize();
        transform.position += moviment * speed * Time.deltaTime;
       
        

        switch (state)
        {
            case playerState.idle: Idle(); break;
            case playerState.jumping: Jump(); break;
            case playerState.falling: Fall(); break;
            case playerState.running: Movement(); break;
            case playerState.climbing: WallClimbing(); break;
        }

    }
    // Update is called once per frame
    void Update()
    {
       
        if(CheckaTaNoChao())
        {
            podepularemdobro = true;
        }
        //pega os inputs do jogador
        inputpulo = Input.GetKey(KeyCode.Space);
        movehorizontalInput = Input.GetAxisRaw("Horizontal");
        inputWallClimb = Input.GetKey(KeyCode.UpArrow);
        inputSlide = Input.GetKeyDown(KeyCode.LeftShift);
        inputSlash = Input.GetKeyDown(KeyCode.P);
        inputSlash = Input.GetKey(KeyCode.P);
        Debug.DrawRay(transform.position, Vector2.down * rccheckachao, Color.red);
        Debug.DrawRay(transform.position, -Vector2.left * rccheckaescalada,Color.blue);
    }
    void CheckSlide()
    {
        if (inputSlide && isRunning)
        {
            animator.Play("slided");
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
        //comportamento do estado
        animator.Play("Run");

        //transi��es do estado
        if (inputpulo && CheckaTaNoChao())
        {
            state = playerState.jumping;
        }
        else if (movehorizontalInput == 0)
        {
            state = playerState.idle;
        }
        else if (inputWallClimb && CheckaPodeEscalar())
        {
            state = playerState.climbing;
        }
        CheckSlide();
        CheckSlash();
    }

    void Idle()
    {
        //comportamento do estado
        animator.Play("Idle");
        Debug.Log("entrou no estado Idle");
        if (CheckaTaNoChao())
        {
            Debug.Log("ta no chao");
        }
        //transi��es
        if (inputpulo && CheckaTaNoChao())
        {
            state = playerState.jumping;
        }
        else if (movehorizontalInput != 0)
        {
            state = playerState.running;
        }
        else if (inputWallClimb && CheckaPodeEscalar())
        {
            state = playerState.climbing;
        }
        CheckSlash();
    }
    void CheckSlash()
    {
        
        if (inputSlash)
        {   
            switch (state)
            {   
                //slash de idle
                case playerState.idle:
                    //comportamento
                    animator.Play("Slash");
                    //transição pra idle
                    inputSlash = false;
                    state = playerState.idle;
                        break;
                // air slash
                case playerState.jumping:
                    animator.Play("Air Slash");
                    state = playerState.falling;
                    inputSlash = false;
                        break;
                case playerState.falling:
                    animator.Play("Air Slash");
                    state = playerState.falling;
                    inputSlash = false;
                        break;
                //running slash
                case playerState.running:
                     //comportamento
                    animator.Play("Run Slashing");
                    //transição
                    if (movehorizontalInput != 0)
                    {
                        state = playerState.running;
                    }
                    else
                    {
                        state = playerState.idle;
                    }
                    inputSlash = false;
                        break;
            }
            
        }
        else if (!inputSlash && state == playerState.attacking)
        {   
            if (movehorizontalInput != 0)
            {
                state = playerState.running;
            }
            else
            {
                state = playerState.idle;
            }
                   
        }
    }

    void Jump()
    {
        //comportamento do estado
        animator.PlayInFixedTime("Jump Start");
       
        Debug.Log("entrou no estado de pulo");

        rb.linearVelocity = Vector2.up * forcapulo;
       
        //rg.AddForce(new Vector2(0f, forcapulo), ForceMode2D.Impulse);

        //transi��es
        state = playerState.falling;

        CheckSlash();
    }

    void Fall()
    {
        //comportamento do estado
        if (rb.linearVelocity.y > 0f)
        {
            animator.Play("Jump Loop");
        }
        else
        {
            animator.Play("Falling");
        }
        //transi��es
        if(podepularemdobro)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                state = playerState.jumping;
                podepularemdobro = false;
            }
          
        }
        if (CheckaTaNoChao())
        {
            if(movehorizontalInput != 0)
            {
                state = playerState.running;
            }
            else if(movehorizontalInput == 0)
            {
                state = playerState.idle;
            }
                
        }
    }

    void WallClimbing()
    {
        //comportamento do estado
        Debug.Log("ta escalando");
        animator.Play("Rope Climb");
        rb.constraints = RigidbodyConstraints2D.FreezePositionX;
        moveverticalInput = Input.GetAxisRaw("Vertical");
        rb.gravityScale = 0f;


    }

    private bool CheckaPodeEscalar()
    {
        Debug.Log("check escalada");
        return Physics2D.Raycast(transform.position, - Vector2.left, rccheckaescalada, LayerMask.GetMask("parede"));
        

    }
    //metodo que verifica se o player
    private bool CheckaTaNoChao()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, rccheckachao, LayerMask.GetMask("chao"));
    }
    void SetAnimatorState()
    {
        switch (state)
        {
            case playerState.running: animator.SetBool("IsRunning", true); break;
            case playerState.jumping: animator.SetBool("IsJumping", true); break;
            case playerState.idle: animator.SetBool("IsuRunning", false);
             animator.SetBool("IsJumping", false); break;
            case playerState.falling: animator.SetBool("IsJumping",false); break;
        }

       
    }

}
