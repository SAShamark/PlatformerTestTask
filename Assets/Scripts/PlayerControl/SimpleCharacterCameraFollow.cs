using UnityEngine;

public class SimpleCharacterCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target;

    [Header("Camera Settings")]
    [SerializeField] private Vector3 _offset = new Vector3(0f, 5f, -10f);
    [SerializeField] private float _smoothSpeed = 5f;
    

    void LateUpdate()
    {
        if (_target == null)
            return;

        Vector3 targetPosition = _target.position + _offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, _smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        transform.LookAt(_target);
    }
}
