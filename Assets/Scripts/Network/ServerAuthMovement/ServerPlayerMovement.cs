using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class ServerPlayerMovement : NetworkBehaviour
{
    [SerializeField] private float playerSpeed;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private AudioListener audioListener;

    [SerializeField] private float _rotationSpeed = .1f;
    [SerializeField] private float _accumulateRotation;

    public CharacterController characterController;
    public MyPlayerInput playerInput;

    // Start is called before the first frame update
    void Start()
    {
        playerInput = new();  
        playerInput.Enable();
    }

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            // enable the listener
            audioListener.enabled = true;

            // set camera priority
            virtualCamera.Priority = 1;
        } else {
            // set camera priority
            virtualCamera.Priority = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // read our players movement
        Vector2 moveInput = playerInput.Player.Movement.ReadValue<Vector2>();

        // determine if we are player or server
        if(IsServer && IsLocalPlayer)
        {
            // move if server
            Move(moveInput);
        } else if (IsClient && IsLocalPlayer)
        {
            // request move if player
            MoveServerRPC(moveInput);
        }

        if(playerInput.Player.RightClick.ReadValue<float>() > 0)
        {
            Vector2 mouseDetal = playerInput.Player.LookAround.ReadValue<Vector2>();

            if (IsServer && IsLocalPlayer)
            {
                // move if server
                LookAround(mouseDetal);
            }
            else if (IsClient && IsLocalPlayer)
            {
                // request move if player
                LookAroundServerRPC(mouseDetal);
            }
        }
    }

    private void Move(Vector2 _input)
    {
        Vector3 calcMove = _input.x * playerTransform.right + _input.y * playerTransform.forward;

        characterController.Move(calcMove * playerSpeed * Time.deltaTime);
    }

    private void LookAround(Vector2 _input)
    {
        // calculating the rotation amount base on the mouse delta + rotation speed
        float rotationAmount = _input.x * _rotationSpeed;

        // accumulate that rotation
        _accumulateRotation += rotationAmount;

        // apply that rotation
        transform.rotation = Quaternion.Euler(0, _accumulateRotation, 0);
    }

    [ServerRpc] 
    private void MoveServerRPC(Vector2 _input)
    {
        Move(_input);
    }

    [ServerRpc] 
    private void LookAroundServerRPC(Vector2 _input)
    {
        LookAround(_input);
    }
}
