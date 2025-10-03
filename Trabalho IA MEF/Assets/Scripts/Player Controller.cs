using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{   
    //componentes
    protected Animator animator;
    protected Rigidbody2D rb;
    protected Collider2D coll;
    protected SpriteRenderer sprite;
    [Header("Player States")]
    [SerializeField]
    protected bool isRunning = false;
    protected bool isAttacking = false;
    protected bool isSliding = false;
    protected bool isGrounded = true;

    [Header("Movimenta��o Base")]
    Vector3 moviment = new Vector3();
    [SerializeField] float velocidadehorizontal;
    [SerializeField] float speed = 3;

    [Header("Pulo")]
    [SerializeField] float forcapulo;
    [SerializeField] bool podepularemdobro;

    [Header("escalada")]
    Vector3 movimentescalada = new Vector3();

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

    protected float attackDuration = 0.5f;
    protected float attackTime;
    
    [Header("Checks")]
    protected bool inputpulo;
    
    [SerializeField] float rccheckachao;
    [SerializeField] float rccheckaescalada;
    protected Vector2 vetorescalada = new Vector3();
    protected bool podeescalar;
    protected float movehorizontalInput;
    protected float moveverticalInput;
    protected bool inputWallClimb;
    protected bool inputSlide;
    protected bool inputSlash;


    [SerializeField]
    protected enum playerState { idle, running, jumping, falling, attacking, sliding, firing, hurt, throwing, climbing}
    protected playerState state = playerState.idle;
    protected playerState lastState;

    
    void Awake()
    {
        //determinando a variavel que ser� usada para:
        //fazer as transi��es entre as anima��es
        animator = GetComponent<Animator>();
        //aplicar a f�sica
        rb = GetComponent<Rigidbody2D>();

        sprite = GetComponent<SpriteRenderer>();

    }

    void Start()
    {  
        
       
      
    }

    private void FixedUpdate()
    {


        movimentescalada = new Vector3(0f,moveverticalInput , 0f);
        //vetor fixo de movimenta��o
        if (state == playerState.climbing)
        {
            moviment = movimentescalada;

        }
        else { moviment = new Vector3(movehorizontalInput, 0f, 0f); };

        moviment.Normalize();
        transform.position += moviment * speed * Time.deltaTime;


        if (movehorizontalInput > 0f)
        {
            sprite.flipX = false;
        }
        else if (movehorizontalInput < 0f)
        {
            sprite.flipX = true;
        }
        //pega os inputs do jogador
        inputpulo = Input.GetKey(KeyCode.Space);
        moveverticalInput = Input.GetAxisRaw("Vertical");
        movehorizontalInput = Input.GetAxisRaw("Horizontal");
        inputWallClimb = Input.GetKey(KeyCode.UpArrow);
        inputSlash = Input.GetKeyDown(KeyCode.E);
        inputSlide = Input.GetKeyDown(KeyCode.LeftShift);

        switch (state)
        {
            case playerState.idle: Idle(); break;
            case playerState.jumping: Jump(); break;
            case playerState.falling: Fall(); break;
            case playerState.running: Movement(); break;
            case playerState.climbing: WallClimbing(); break;
            case playerState.attacking: Slash(); break;
            case playerState.sliding: Slide(); break;
        }





    }
    // Update is called once per frame
    void Update()
    {
        //define o vetor do raycast pra checkar se o jogador pode escalar
        vetorescalada = new Vector2(rccheckaescalada, 0);
        podeescalar = Physics2D.Raycast(transform.position, vetorescalada, rccheckaescalada, LayerMask.GetMask("parede"));
        if (CheckaTaNoChao())
        {
            podepularemdobro = true;
        }
        
        //desenha os raios para verifica��o dentro da unity
        Debug.DrawRay(transform.position, Vector2.down * rccheckachao, Color.red);
        Debug.DrawRay(transform.position, vetorescalada ,Color.blue);


    }
    void CheckSlide()
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
    void Slide()
    {
        
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
        if (inputSlash)
        {
            SetStateAttacking();
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
        else if (inputSlash)
        {
            SetStateAttacking();
        }
    }
    void SetStateAttacking()
    {
        lastState = state;
        state = playerState.attacking;
    }
    void Slash()
    {
        
            switch (lastState)
            {
                //slash de idle
                case playerState.idle:
                    //comportamento
                    animator.Play("Slash");
                    //transição pra idle
                    attackTime += Time.fixedDeltaTime;
                    if (attackTime >= attackDuration)
                    {
                        state = playerState.idle;
                        attackTime = 0f;
                    };
                    break;
                // air slash do jump
                case playerState.jumping:
                    animator.Play("Air Slash");
                    attackTime += Time.fixedDeltaTime;
                    if (attackTime >= attackDuration)
                    {
                        state = playerState.falling;
                        attackTime = 0f;
                    }
                    break;
                //air slash do falling
                case playerState.falling:
                    animator.Play("Air Slash");
                    attackTime += Time.fixedDeltaTime;
                    if (attackTime >= attackDuration)
                    {
                        state = playerState.falling;
                        attackTime = 0f;
                    }
                    break;
                //running slash
                case playerState.running:
                    //comportamento
                    animator.Play("Run Slashing");
                    //transição
                    if (movehorizontalInput != 0)
                    {
                        attackTime += Time.fixedDeltaTime;
                        if (attackTime >= attackDuration)
                        {
                            state = playerState.running;
                            attackTime = 0f;
                        }
                    }
                    else
                    {
                        attackTime += Time.fixedDeltaTime;
                        if (attackTime >= attackDuration)
                        {
                            state = playerState.idle;
                            attackTime = 0f;
                        }
                    }
                    break;

        }
        /*else if (!inputSlash && state == playerState.attacking)
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
        */
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
       if (inputSlash)
        {
            SetStateAttacking();
        }

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
        if (inputSlash)
        {
            SetStateAttacking();
        }
        if (podepularemdobro)
        {
            if (Input.GetKeyDown(KeyCode.Space))
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
        
        
        if (moveverticalInput != 0 && podeescalar)
        {
            animator.Play("Rope Climb");
            moviment = movimentescalada;
            rb.gravityScale = 0f;
        }


        // transi��o do estado

        else
        {
            rb.gravityScale = 1f;
            moviment = moviment = new Vector3(movehorizontalInput, 0f, 0f);
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
