using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class MouseMoveEnable : MonoBehaviour
{
    [SerializeField] private float _toEnableMouseMove = 0.5f;
    [SerializeField] private float _interactionDistance = 100f;
    [SerializeField] private LayerMask _clickRayLayerMask;

    public event Action Enable;
    public event Action Disable;

    public bool CanMouseMove { get; private set; }
    public Node ClickedNode { get; private set; }

    private InputSystem _input;
    private Camera _mainCamera;
    
    private Coroutine _mouseMoveEnableCoroutine;
    private float _elapsedFromMoveEnableStarted;

    public void Initialize(InputSystem input)
    {
        _input = input;
        _mainCamera = Camera.main;

        _input.Default.MouseMoveEnable.performed += OnMouseEnablePerformed;
        _input.Default.MouseMoveEnable.canceled += OnMouseEnableCanceled;
    }

    private void OnMouseEnablePerformed(InputAction.CallbackContext ctx) => _mouseMoveEnableCoroutine ??= StartCoroutine(WaitForEnable());

    private IEnumerator WaitForEnable()
    {
        while(_elapsedFromMoveEnableStarted < _toEnableMouseMove)
        {
            _elapsedFromMoveEnableStarted += Time.deltaTime;
            yield return null;
        }

        CanMouseMove = true;
        _mouseMoveEnableCoroutine = null;
        Enable?.Invoke();
    }

    private void OnMouseEnableCanceled(InputAction.CallbackContext ctx)
    {
        if (CanMouseMove is false)
            StartCoroutine(ActivateInteractables());

        CanMouseMove = false;
        _elapsedFromMoveEnableStarted = 0f;

        if (_mouseMoveEnableCoroutine is not null)
        {
            StopCoroutine(_mouseMoveEnableCoroutine);
            _mouseMoveEnableCoroutine = null;
        }

        Disable?.Invoke();
    }

    private IEnumerator ActivateInteractables()
    {
        yield return new WaitForEndOfFrame();

        if (EventSystem.current.IsPointerOverGameObject()) yield break;

        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            
        if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _clickRayLayerMask))
        {
            if (hit.collider.TryGetComponent<IClickable>(out var clickable))
                clickable.OnClick();
            
            if (hit.collider.TryGetComponent<Node>(out var node))
                ClickedNode = node;
            else
                ClickedNode = null;
        }
    }

    private void OnDisable()
    {
        _input.Default.MouseMoveEnable.performed -= OnMouseEnablePerformed;
        _input.Default.MouseMoveEnable.canceled -= OnMouseEnableCanceled;
    }
}
