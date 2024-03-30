using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using System;

public class PredictiveServerPlayerMovement : NetworkBehaviour
{
    [SerializeField] private float playerSpeed;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private AudioListener audioListener;

    [SerializeField] private float _rotationSpeed = .1f;
    [SerializeField] private float _accumulateRotation;

    public CharacterController characterController;
    public MyPlayerInput playerInput;

    [SerializeField] private int tick = 0;
    private float tickRate = 1f / 60f;
    [SerializeField] private float tickDeltaTime = 0;

    private const int buffer = 1024;

    private HandleStates.InputState[] _inputStates = new HandleStates.InputState[buffer];   
    private HandleStates.TransformStateRW[] _transformStates = new HandleStates.TransformStateRW[buffer];

    public NetworkVariable<HandleStates.TransformStateRW> currentServerTransformState = new();
    public HandleStates.TransformStateRW previousTransformState;


    // Start is called before the first frame update
    void Start()
    {
        playerInput = new();  
        playerInput.Enable();
    }

    private void OnServerStateChanged(HandleStates.TransformStateRW previousValue,  HandleStates.TransformStateRW newValue)
    {
        previousTransformState = previousValue;
    }

    private void OnEnable()
    {
        currentServerTransformState.OnValueChanged += OnServerStateChanged;
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

    public void ProcessLocalPlayerMovement(Vector2 _moveInput, Vector2 _lookAround)
    {
        tickDeltaTime += Time.deltaTime;

        if(tickDeltaTime > tickRate)
        {
            int bufferIndex = tick % buffer;

            // Move player with server tick RPC 
            MovePlayerWithServerTickServerRPC(tick, _moveInput, _lookAround);
            Move(_moveInput);
            LookAround(_lookAround);

            HandleStates.InputState inputState = new()
            {
                tick = tick,
                moveInput = _moveInput,
                lookAround = _lookAround
            };

            HandleStates.TransformStateRW transformState = new()
            {
                tick = tick,
                finalPoss = transform.position,
                finalRot = transform.rotation,
                isMoving = true
            };

            _inputStates[bufferIndex] = inputState;
            _transformStates[bufferIndex] = transformState;

            tickDeltaTime -= tickRate;
            if (tick > buffer)
                tick = 0;
            else
                tick++;
        }
    }

    [ServerRpc]
    private void MovePlayerWithServerTickServerRPC(int tick, Vector2 moveInput, Vector2 lookAround)
    {
        Move(moveInput);
        LookAround(lookAround);

        HandleStates.TransformStateRW transformState = new()
        {
            tick = tick,
            finalPoss = transform.position,
            finalRot = transform.rotation,
            isMoving = true
        };

        previousTransformState = currentServerTransformState.Value;
        currentServerTransformState.Value = transformState;
    }

    private void SimulateOtherPlayers()
    {
        tickDeltaTime += Time.deltaTime;

        if(tickDeltaTime > tickRate)
        {
            if(currentServerTransformState.Value.isMoving)
            {
                transform.position = currentServerTransformState.Value.finalPoss;
                transform.rotation = currentServerTransformState.Value.finalRot;
            }

            tickDeltaTime -= tickRate;
            if (tick > buffer)
                tick = 0;
            else
                tick++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // read our players movement
        Vector2 moveInput = playerInput.Player.Movement.ReadValue<Vector2>();
        Vector2 mouseDetal = playerInput.Player.LookAround.ReadValue<Vector2>();
        if (IsClient && IsLocalPlayer)
        {
            ProcessLocalPlayerMovement(moveInput, mouseDetal);                       
        } else
        {
            SimulateOtherPlayers(); 
        }
    }

    private void Move(Vector2 _input)
    {
        Vector3 calcMove = _input.x * playerTransform.right + _input.y * playerTransform.forward;

        characterController.Move(calcMove * playerSpeed * tickRate);
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
}
