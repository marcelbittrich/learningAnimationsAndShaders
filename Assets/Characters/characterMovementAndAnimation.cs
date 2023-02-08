using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class characterMovementAndAnimation : MonoBehaviour
{
    Animator _animator;
    CharacterController _characterController;

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

    // Animation values
    int _animatorVelocityXHash;
    int _animatorVelocityZHash;

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
        Vector3 targetVelocity = Vector3.zero;
        float currentMaxVelocity;

        currentMaxVelocity = _isRunPressed ? MaxRunVelocity : MaxWalkVelocity;
        targetVelocity += _inputMovement.y * transform.forward * currentMaxVelocity;
        targetVelocity += _inputMovement.x * transform.right * currentMaxVelocity;

        if (_currentVelocity.z != targetVelocity.z || _currentVelocity.x != targetVelocity.x)
        {
            _currentVelocity = Vector3.Lerp(_currentVelocity, targetVelocity, TransitionSpeed * Time.deltaTime);
            clampWalkingVelocity(targetVelocity);
        }
    }

    void clampWalkingVelocity(Vector3 targetVelocity)
    {
        if (Mathf.Abs(_currentVelocity.z) <= 0.001f) _currentVelocity.z = 0;
        if (Mathf.Abs(_currentVelocity.x) <= 0.001f) _currentVelocity.x = 0;
        if (Mathf.Abs(targetVelocity.z - _currentVelocity.z) < 0.001f) _currentVelocity.z = targetVelocity.z;
        if (Mathf.Abs(targetVelocity.x - _currentVelocity.x) < 0.001f) _currentVelocity.x = targetVelocity.x;
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
        float horizontalRotation = RotationSpeed * _inputLook.x * Time.deltaTime;
        Vector3 rotationDelta = new Vector3 ( 0.0f, horizontalRotation, 0.0f );

        transform.Rotate(rotationDelta);
    }


    void handleAnimation() 
    {
        Vector3 localVelocity = transform.InverseTransformDirection (_currentVelocity);
        _animator.SetFloat(_animatorVelocityXHash, localVelocity.x);
        _animator.SetFloat(_animatorVelocityZHash, localVelocity.z);
    }

    // Update is called once per frame
    void Update()
    {
        handleMovement();
        handleGravity();

        _characterController.Move(_currentVelocity * Time.deltaTime);

        handleRotation();
        handleAnimation();  
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
