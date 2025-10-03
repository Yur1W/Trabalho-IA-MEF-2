using System.Collections;
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

    [Header("Movimentação Base")]
    Vector3 moviment = new Vector3();
    [SerializeField] float velocidadehorizontal;
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
    Vector2 vetorescalada = new Vector2();
    bool taescalando;
    float movehorizontalInput;
    float moveverticalInput;
    bool inputWallClimb;


    [SerializeField]
    enum playerState { idle, running, jumping, falling, attacking, sliding, firing, hurt, throwing, climbing}
    playerState state = playerState.idle;

    
    void Awake()
    {
        //determinando a variavel que será usada para:
        //fazer as transições entre as animações
        animator = GetComponent<Animator>();
        //aplicar a física
        rb = GetComponent<Rigidbody2D>();

        
    }

    void Start()
    {  
        
       
      
    }

    private void FixedUpdate()
    {
        
        
        //vetor fixo de movimentação
        if(state != playerState.climbing)
        {
            moviment = new Vector3(movehorizontalInput, moveverticalInput, 0f);
            moviment.Normalize();
            transform.position += moviment * speed * Time.deltaTime;
        }
   
       
        

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
        //define o vetor do raycast pra checkar se o jogador pode escalar
        vetorescalada = new Vector2(rccheckaescalada, 0);
        taescalando = Physics2D.Raycast(transform.position, vetorescalada, rccheckaescalada, LayerMask.GetMask("parede"));
        if (CheckaTaNoChao())
        {
            podepularemdobro = true;
        }
        //pega os inputs do jogador
        inputpulo = Input.GetKey(KeyCode.Space);
        movehorizontalInput = Input.GetAxisRaw("Horizontal");
        inputWallClimb = Input.GetKey(KeyCode.UpArrow);

        //desenha os raios para verificação dentro da unity
        Debug.DrawRay(transform.position, Vector2.down * rccheckachao, Color.red);
        Debug.DrawRay(transform.position, vetorescalada ,Color.blue);


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
        //comportamento do estado
        animator.Play("Run");

        //transições do estado
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
        //transições
        if (inputpulo && CheckaTaNoChao())
        {
            state = playerState.jumping;
        }
        else if (movehorizontalInput != 0)
        {
            state = playerState.running;
        }
        else if(inputWallClimb && CheckaPodeEscalar())
        {
            state = playerState.climbing;
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

    void Jump()
    {
        //comportamento do estado
        animator.PlayInFixedTime("Jump Start");
       
        Debug.Log("entrou no estado de pulo");

        rb.linearVelocity = Vector2.up * forcapulo;
       
        //rg.AddForce(new Vector2(0f, forcapulo), ForceMode2D.Impulse);

        //transições
        state = playerState.falling;
       

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
        //transições
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
        moveverticalInput = Input.GetAxisRaw("Vertical");
        if (moveverticalInput != 0 && taescalando)
        {
            
            moviment = new Vector3(0, moveverticalInput, 0);
            rb.gravityScale = 0f;
        }


        // transição do estado

        else
        {
            rb.gravityScale = 1f;
            state = playerState.falling;
        }


    }

    private bool CheckaPodeEscalar()
    {
        Debug.Log("check escalada");
        return Physics2D.Raycast(transform.position, vetorescalada, rccheckaescalada, LayerMask.GetMask("parede"));
        

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
