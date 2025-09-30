using UnityEngine;

public class PlayerController : MonoBehaviour
{
    protected Animator animator;
    protected Rigidbody2D rb;
    protected Collider2D coll;
    protected bool isRunning = false;

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
        if (Input.GetKeyUp(KeyCode.LeftShift) && isRunning)
        {
            animator.SetTrigger("isSliding");
        }
        
    }
}
