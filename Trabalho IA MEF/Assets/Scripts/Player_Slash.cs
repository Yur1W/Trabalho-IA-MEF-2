using UnityEngine;

public class Player_Slash : PlayerController
{   
    bool isAttacking = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       if (Input.GetKeyDown(KeyCode.P))
       {    
            isAttacking = true;
            animator.SetTrigger("isSlashing");
       } 
    }
}
