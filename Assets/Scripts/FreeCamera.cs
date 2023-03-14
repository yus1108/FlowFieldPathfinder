using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1.0f;
    [SerializeField] private float _acceleratedMoveSpeed = 2.0f;
    [SerializeField] private float _rotateSpeedInDegree = 360.0f;

    private Vector2 _rotation;

    private void Start()
    {
        _rotation.x = transform.eulerAngles.x;
        _rotation.x = Mathf.Clamp(_rotation.x, -90, 90);
    }

    // Update is called once per frame
    private void Update()
    {
        Vector3 moveVector = (transform.forward * Input.GetAxis("Vertical") + 
            transform.right * Input.GetAxis("Horizontal") +
            transform.up * Input.GetAxis("E") +
            transform.up * -Input.GetAxis("Q")) * 
            Time.deltaTime * 
            (Input.GetButton("Shift") ? _acceleratedMoveSpeed : _moveSpeed);
        transform.Translate(moveVector);

        if (Input.GetMouseButton(1))
        {
            float xRotateMove = -Input.GetAxis("Mouse Y") * Time.deltaTime * _rotateSpeedInDegree;
            float yRotateMove = Input.GetAxis("Mouse X") * Time.deltaTime * _rotateSpeedInDegree;
            _rotation.y = transform.eulerAngles.y + yRotateMove;
            _rotation.x = _rotation.x + xRotateMove;
            _rotation.x = Mathf.Clamp(_rotation.x, -90, 90); // 위, 아래 고정
            transform.eulerAngles = new Vector3(_rotation.x, _rotation.y, 0);
        }
    }
}
