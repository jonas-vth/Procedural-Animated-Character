using UnityEngine;

public class DebugCamera : MonoBehaviour
{
    Vector2 input;
    Vector2 mouse;

    public float accMove = 4;
    public float accMouse = 300;
    public Transform target;
    Vector3 eulers;
    Vector3 pos;

    void Start()
    {
        eulers = transform.eulerAngles;
        pos = transform.position;
    }


    void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        float speedmod = Input.GetKey(KeyCode.LeftShift) ? 3 : 1;
        pos += accMove * speedmod * Time.deltaTime * (transform.forward * input.y + transform.right * input.x);
        transform.localPosition = pos;

        if (Input.GetMouseButton(1))
        {
            mouse = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            eulers += accMouse * Time.deltaTime * (Vector3.right * -mouse.y + Vector3.up * mouse.x);
            eulers.x = Mathf.Clamp(eulers.x, -85f, 85f);
            transform.localEulerAngles = eulers;
        }



        if (Input.GetMouseButtonDown(0))
        {
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }


    }
}
