using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float Speed = 10f;
    void Update()
    {
        if(IsOwner)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            transform.position = transform.position + new Vector3(x, 0, z) * Speed * Time.deltaTime; 
        }
    }
}
