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
    [SerializeField] float quantidadePulos = 0f;

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
    [SerializeField]
    public static float playerHealth { get; set; } = 5f;

    [Header("Attack CDs")]
    [SerializeField]
    protected float attackDuration = 0.5f;
    [SerializeField]
    protected float fireDuration = 0.5f;
    [SerializeField]
    protected float hurtDuration = 0.4f;
    [SerializeField]
    protected float throwingDuration = 0.5f;
    protected float timer;

    [Header("Checks")]
    protected bool inputpulo;
    protected bool InputPuloDuplo;
    protected bool inputSlide;
    protected bool inputSlash;
    protected bool inputFiring;
    protected bool inputThrowing;

    [SerializeField] float rccheckachao;
    [SerializeField] float rccheckaescalada;
    protected Vector2 vetorescalada = new Vector3();
    protected bool podeescalar;
    protected float movehorizontalInput;
    protected float moveverticalInput;
    protected bool inputWallClimb;



    protected enum playerState { idle, running, jumping, doublejump, falling, fallingDJ, attacking, sliding, firing, hurt, throwing, climbing, death }
    [SerializeField]
    protected playerState state = playerState.idle;
    [SerializeField]
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


        movimentescalada = new Vector3(0f, moveverticalInput, 0f);
        //vetor fixo de movimenta��o
        if (state == playerState.climbing)
        {
            moviment = movimentescalada;

        }
        else { moviment = new Vector3(movehorizontalInput, 0f, 0f); }
        ;

        moviment.Normalize();
        transform.position += moviment * speed * Time.fixedDeltaTime;


        if (movehorizontalInput > 0f)
        {
            sprite.flipX = false;
        }
        else if (movehorizontalInput < 0f)
        {
            sprite.flipX = true;
        }


        switch (state)
        {
            case playerState.idle: Idle(); break;
            case playerState.jumping: Jump(); break;
            case playerState.doublejump: DoubleJump(); break;
            case playerState.falling: Fall(); break;
            case playerState.fallingDJ: FallDJ(); break;
            case playerState.running: Movement(); break;
            case playerState.climbing: WallClimbing(); break;
            case playerState.attacking: Slash(); break;
            case playerState.sliding: Slide(); break;
            case playerState.firing: Firing(); break;
            case playerState.throwing: Throwing(); break;
            case playerState.hurt: Hurt(); break;
            case playerState.death: Death(); break;
            default: Idle(); break;
        }





    }
    // Update is called once per frame
    void Update()
    {   //pega os inputs do jogador
        inputpulo = Input.GetKey(KeyCode.Space);
        InputPuloDuplo = Input.GetKeyDown(KeyCode.LeftShift);
        moveverticalInput = Input.GetAxisRaw("Vertical");
        movehorizontalInput = Input.GetAxisRaw("Horizontal");
        inputWallClimb = Input.GetKey(KeyCode.UpArrow);
        inputSlash = Input.GetKey(KeyCode.Y);
        inputSlide = Input.GetKey(KeyCode.C);
        inputFiring = Input.GetKey(KeyCode.U);
        inputThrowing = Input.GetKey(KeyCode.I);

        //define o vetor do raycast pra checkar se o jogador pode escalar
        vetorescalada = new Vector2(rccheckaescalada, 0);
        podeescalar = Physics2D.Raycast(transform.position, vetorescalada, rccheckaescalada, LayerMask.GetMask("parede"));
        if (CheckaTaNoChao())
        {
            podepularemdobro = true;
        }

        //desenha os raios para verifica��o dentro da unity
        Debug.DrawRay(transform.position, Vector2.down * rccheckachao, Color.red);
        Debug.DrawRay(transform.position, vetorescalada, Color.blue);


    }

    public void SetStateDamaged()
    {
        lastState = state;
        timer = 0f;
        state = playerState.hurt;
    }

    void Hurt()
    {
        animator.Play("Hurt");
        playerHealth--;
        timer += Time.fixedDeltaTime;
        if (timer >= hurtDuration)
        {
            if (playerHealth <= 0)
            {
                state = playerState.death;
            }
            else
            {
                state = lastState;
            }
        }
    }
    public void Death()
    {
        animator.Play("Dying");
        //desabilita o script de movimenta��o
        this.enabled = false;
        //para o movimento
        rb.linearVelocity = Vector2.zero;
    }
    //seta o estado do jogador
    void SetStateSlide()
    {
        isSliding = true;
        timer = 0f;
        state = playerState.sliding;
    }
    // ação do estado
    void Slide()
    {
        animator.Play("Slide");
        currentSpeed = slideSpeed;
        timer += Time.fixedDeltaTime;
        if (timer >= slideTime)
        {
            //transição do estado
            currentSpeed = playerSpeed;
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
        if (inputFiring)
        {
            SetStateFiring();
        }
        if (inputThrowing)
        {
            SetStateThrowing();
        }
        if (inputSlide && CheckaTaNoChao() && movehorizontalInput != 0)
        {
            SetStateSlide();
        }
        if (!CheckaTaNoChao())
        {
            state = playerState.falling;
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
        else if (inputFiring)
        {
            SetStateFiring();
        }
        else if (inputThrowing)
        {
            SetStateThrowing();
        }
    }
    void SetStateFiring()
    {
        lastState = state;
        timer = 0f;
        state = playerState.firing;
    }
    void Firing()
    {

        switch (lastState)
        {
            //firing de idle
            case playerState.idle:
                //comportamento
                animator.Play("Firing");
                //transição pra idle
                timer += Time.fixedDeltaTime;
                if (timer >= fireDuration)
                {
                    state = playerState.idle;
                    timer = 0f;
                }
                ;
                break;
            // air firing do jump
            case playerState.jumping:
                animator.Play("Air Firing");
                timer += Time.fixedDeltaTime;
                if (timer > fireDuration)
                {
                    state = playerState.falling;
                    timer = 0f;
                }
                break;
            //air firing do falling
            case playerState.falling:
                animator.Play("Air Firing");
                timer += Time.fixedDeltaTime;
                if (CheckaTaNoChao())
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
                if (timer > fireDuration)
                {
                    state = playerState.falling;
                    timer = 0f;
                }

                break;
            //running fire
            case playerState.running:
                //comportamento
                animator.Play("Run Firing");
                //transição
                if (movehorizontalInput != 0)
                {
                    timer += Time.fixedDeltaTime;
                    if (timer >= fireDuration)
                    {
                        state = playerState.running;
                        timer = 0f;
                    }
                }
                else
                {
                    timer += Time.fixedDeltaTime;
                    if (timer >= fireDuration)
                    {
                        state = playerState.idle;
                        timer = 0f;
                    }
                }
                break;

        }
    }
    void SetStateAttacking()
    {
        lastState = state;
        timer = 0f;
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
                timer += Time.fixedDeltaTime;
                if (timer >= attackDuration)
                {
                    state = playerState.idle;
                    timer = 0f;
                }
                ;
                break;
            // air slash do jump
            case playerState.jumping:
                animator.Play("Air Slash");
                timer += Time.fixedDeltaTime;
                if (timer >= attackDuration)
                {
                    state = playerState.falling;
                    timer = 0f;
                }
                break;
            //air slash do falling
            case playerState.falling:
                animator.Play("Air Slash");
                timer += Time.fixedDeltaTime;
                if (CheckaTaNoChao())
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
                if (timer >= attackDuration)
                {
                    state = playerState.falling;
                    timer = 0f;
                }
                break;
            //running slash
            case playerState.running:
                //comportamento
                animator.Play("Run Slashing");
                //transição
                if (movehorizontalInput != 0)
                {
                    timer += Time.fixedDeltaTime;
                    if (timer >= attackDuration)
                    {
                        state = playerState.running;
                        timer = 0f;
                    }
                }
                else
                {
                    timer += Time.fixedDeltaTime;
                    if (timer >= attackDuration)
                    {
                        state = playerState.idle;
                        timer = 0f;
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
        quantidadePulos++;

        //rg.AddForce(new Vector2(0f, forcapulo), ForceMode2D.Impulse);

        //transi��es
        state = playerState.falling;
        if (inputSlash)
        {
            SetStateAttacking();
        }
        if (inputFiring)
        {
            SetStateFiring();
        }
        if (inputThrowing)
        {
            SetStateThrowing();
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
        if (InputPuloDuplo)
        {
            SetStateDoubleJump();
        }
        if (inputSlash)
        {
            SetStateAttacking();
        }
        if (inputFiring)
        {
            SetStateFiring();
        }
        if (inputThrowing)
        {
            SetStateThrowing();
        }

        if (CheckaTaNoChao())
        {
            if (movehorizontalInput != 0)
            {
                state = playerState.running;
            }
            else if (movehorizontalInput == 0)
            {
                state = playerState.idle;
            }

        }
    }
    void SetStateDoubleJump()
    {
        if (podepularemdobro)
        {
            rb.linearVelocity = new Vector2(0, 0);
            state = playerState.doublejump;
            podepularemdobro = false;
        }
    }
    void DoubleJump()
    {
        //comportamento do estado

        Debug.Log("entrou no estado de pulo duplo");

        rb.linearVelocity = Vector2.up * forcapulo;

        //transi��es
        state = playerState.fallingDJ;
        if (inputSlash)
        {
            SetStateAttacking();
        }
        if (inputFiring)
        {
            SetStateFiring();
        }
        if (inputThrowing)
        {
            SetStateThrowing();
        }
        if (CheckaTaNoChao())
        {
            if (movehorizontalInput != 0)
            {
                state = playerState.running;
            }
            else if (movehorizontalInput == 0)
            {
                state = playerState.idle;
            }

        }
    }
    void FallDJ()
    {
        //comportamento do estado
        if (rb.linearVelocity.y > 0f)
        {
            animator.Play("Double Jump");
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
        if (inputFiring)
        {
            SetStateFiring();
        }
        if (inputThrowing)
        {
            SetStateThrowing();
        }

        if (CheckaTaNoChao())
        {
            if (movehorizontalInput != 0)
            {
                state = playerState.running;
            }
            else if (movehorizontalInput == 0)
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Spike"))
        {
            SetStateDamaged();
        }

    }

    void SetStateThrowing()
    {
        lastState = state;
        state = playerState.throwing;
        timer = 0f;
    }

    void Throwing()
    {
        switch (lastState)
        {
            //throw de idle
            case playerState.idle:
                //comportamento
                animator.Play("Throwing");
                //transição pra idle
                timer += Time.fixedDeltaTime;
                if (timer >= throwingDuration)
                {
                    state = playerState.idle;
                    timer = 0f;
                }
                ;
                break;
            // air throw do jump
            case playerState.jumping:
                animator.Play("Air Throwing");
                timer += Time.fixedDeltaTime;
                if (timer >= throwingDuration)
                {
                    state = playerState.falling;
                    timer = 0f;
                }
                break;
            //air Throw do falling
            case playerState.falling:
                animator.Play("Air Throwing");
                timer += Time.fixedDeltaTime;
                if (CheckaTaNoChao())
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
                if (timer >= throwingDuration)
                {
                    state = playerState.falling;
                    timer = 0f;
                }
                break;
            //running Throw
            case playerState.running:
                //comportamento
                animator.Play("Run Throwing");
                //transição
                if (movehorizontalInput != 0)
                {
                    timer += Time.fixedDeltaTime;
                    if (timer >= throwingDuration)
                    {
                        state = playerState.running;
                        timer = 0f;
                    }
                }
                else
                {
                    timer += Time.fixedDeltaTime;
                    if (timer >= throwingDuration)
                    {
                        state = playerState.idle;
                        timer = 0f;
                    }
                }
                break;

        }
    }
}
