using Unity.VisualScripting;
using UnityEngine;

public class Spike : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    PlayerController playerController;
    void Start()
    {
        playerController = GameObject.Find("Jogador").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    
}
