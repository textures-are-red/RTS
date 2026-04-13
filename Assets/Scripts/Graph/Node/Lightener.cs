using System.Collections;
using UnityEngine;

public class Lightener : MonoBehaviour
{
    [SerializeField] private float _lightFactor = 1.3f;
    [SerializeField] private float _transitionSpeed = 2f;

    private float _currentBrightness = 1f;
    private float _targetBrightness = 1f;

    private Material _material;
    private Coroutine _lightTransition;

    private Color _defaultColor;

    public void Awake()
    {
        _material = GetComponent<Renderer>().material;
        _defaultColor = _material.color;
    }

    public void UpdateOriginalColor(Color newColor)
    {
        _defaultColor = newColor;
        ApplyBrightness();
    }

    public void BecomeDefault()
    {
        if (_lightTransition is not null)
        {
            _material.color = _defaultColor;
            StopCoroutine(_lightTransition);
            _lightTransition = null;
        }
    }
    
    private void OnMouseEnter()
    {
        _targetBrightness = _lightFactor;
        _lightTransition ??= StartCoroutine(LightTransition());
    }

    private IEnumerator LightTransition()
    {
        while(_currentBrightness < _targetBrightness ? _currentBrightness < _targetBrightness : _currentBrightness > _targetBrightness)
        {
            _currentBrightness = Mathf.Lerp(_currentBrightness, _targetBrightness, Time.deltaTime * _transitionSpeed);
            ApplyBrightness();

            yield return null;
        }

        _currentBrightness = _targetBrightness;
        ApplyBrightness();
        _lightTransition = null;
    }

    private void OnMouseExit()
    {
        _targetBrightness = 1f;
        _lightTransition ??= StartCoroutine(LightTransition());
    }

    private void ApplyBrightness()
    {
        Color adjustedColor = new Color(
            _defaultColor.r * _currentBrightness,
            _defaultColor.g * _currentBrightness,
            _defaultColor.b * _currentBrightness,
            _defaultColor.a);
        
        _material.color = adjustedColor;
    }
}
