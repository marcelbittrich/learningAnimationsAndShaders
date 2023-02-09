using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class characterMovementAndAnimation : MonoBehaviour
{
    Animator _animator;
    CharacterController _characterController;

    public Camera CharacterCamera;

    // Movement Values
    PlayerInputs _playerInput;
    Vector2 _inputMovement;
    Vector2 _inputLook;
    Vector3 _currentVelocity;
    bool _isRunPressed;

    public float RotationSpeed = 90.0f;
    public float TransitionSpeed = 3.0f;
    public float MaxWalkVelocity = 1.4f;
    public float MaxRunVelocity = 5.0f;

    bool _isMoving;

    // Animation values
    int _animatorVelocityXHash;
    int _animatorVelocityZHash;
    int _animatorIsMovingHash;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        _playerInput = new PlayerInputs();

        _playerInput.CharacterControls.Move.started += onMovementInput;
        _playerInput.CharacterControls.Move.performed += onMovementInput;
        _playerInput.CharacterControls.Move.canceled += onMovementInput;

        _playerInput.CharacterControls.Run.started += onRunInput;
        _playerInput.CharacterControls.Run.canceled += onRunInput;

        _playerInput.CharacterControls.Look.started += onLookInput;
        _playerInput.CharacterControls.Look.performed += onLookInput;
        _playerInput.CharacterControls.Look.canceled += onLookInput;

        _animatorVelocityXHash = Animator.StringToHash("VelocityX");
        _animatorVelocityZHash = Animator.StringToHash("VelocityZ");
        _animatorIsMovingHash = Animator.StringToHash("IsMoving");
    }

    private void Start()
    {
        //CharacterCamera = GetComponent<Camera>();
    }

    void onMovementInput(InputAction.CallbackContext context)
    {
        _inputMovement = context.ReadValue<Vector2>();
    }

    void onRunInput(InputAction.CallbackContext context)
    {
        _isRunPressed = context.ReadValueAsButton();
    }

    void onLookInput(InputAction.CallbackContext context) 
    {
        _inputLook = context.ReadValue<Vector2>();
    }

    void handleMovement()
    {
        // Get camera relative axis.
        Vector3 forward = CharacterCamera.transform.forward;
        Vector3 right = CharacterCamera.transform.right;

        // Cleanup y component of vector.
        forward = NormalizeIntoXZPlane(forward);
        right = NormalizeIntoXZPlane(right);

        Debug.Log(right);

        // Calculate current target velocity.
        float currentMaxVelocity;
        currentMaxVelocity = _isRunPressed ? MaxRunVelocity : MaxWalkVelocity;

        Vector3 targetVelocity = Vector3.zero;

        targetVelocity += _inputMovement.y * forward * currentMaxVelocity;
        targetVelocity += _inputMovement.x * right * currentMaxVelocity;

        // Interpolate between current and target velocity.
        if (_currentVelocity.z != targetVelocity.z || _currentVelocity.x != targetVelocity.x)
        {
            _currentVelocity = Vector3.Lerp(_currentVelocity, targetVelocity, TransitionSpeed * Time.deltaTime);
            clampWalkingVelocity(targetVelocity);
        }
             
        _isMoving = Mathf.Abs(_currentVelocity.z) > 0.1 || Mathf.Abs(_currentVelocity.x) > 0.1;
    }

    Vector3 NormalizeIntoXZPlane(Vector3 targetVector)
    {
        targetVector.y = 0;
        targetVector.Normalize();
        return targetVector;
    }

    void clampWalkingVelocity(Vector3 targetVelocity)
    {
        if (Mathf.Abs(_currentVelocity.z) <= 0.01f) _currentVelocity.z = 0;
        if (Mathf.Abs(_currentVelocity.x) <= 0.01f) _currentVelocity.x = 0;
        if (Mathf.Abs(targetVelocity.z - _currentVelocity.z) < 0.01f) _currentVelocity.z = targetVelocity.z;
        if (Mathf.Abs(targetVelocity.x - _currentVelocity.x) < 0.01f) _currentVelocity.x = targetVelocity.x;
    }

    void handleGravity()
    {
        float gravityOnGround = -0.05f;
        float gravity = -9.81f;

        if (_characterController.isGrounded)
        {
            _currentVelocity.y = gravityOnGround;
        }
        else
        {
            _currentVelocity.y = gravity;
        }
    }


    void handleRotation()
    {
        if (_isMoving) 
        {
            Vector3 positionToLookAt = transform.position + new Vector3(_currentVelocity.x, 0.0f, _currentVelocity.z);
            transform.LookAt(positionToLookAt);
        }   
    }


    void handleAnimation() 
    {
        Vector3 localVelocity = transform.InverseTransformDirection (_currentVelocity);
        _animator.SetFloat(_animatorVelocityXHash, localVelocity.x);
        _animator.SetFloat(_animatorVelocityZHash, localVelocity.z);
        _animator.SetBool(_animatorIsMovingHash, _isMoving);
    }

    // Update is called once per frame
    void Update()
    {
        handleMovement();
        handleGravity();
        handleRotation();
        // Animation drives movement.
        handleAnimation();  
    }

    private void OnAnimatorMove()
    {
        if (_isMoving) { 
            Vector3 velocity = _animator.deltaPosition;
            velocity.y = _currentVelocity.y * Time.deltaTime;
            _characterController.Move(velocity);
        }
    }

    void OnEnable()
    {
        _playerInput.CharacterControls.Enable();
    }

    void OnDisable()
    {
        _playerInput.CharacterControls.Disable();
    }
}
