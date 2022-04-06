using UnityEngine;

public class LocalPlayerInputInfo : MonoBehaviour
{
    public bool IsPressingForwards { get; private set; }

    public bool IsPressingBack { get; private set; }

    public bool IsPressingRight { get; private set; }

    public bool IsPressingLeft { get; private set; }

    public bool IsPressingJump { get; private set; }

    public bool IsPressingSprint { get; private set; }

    public float HorizontalInput { get; private set; }

    public float VerticalInput { get; private set; }

    public float MouseX { get; private set; }

    public float MouseY { get; private set; }

    private void Update()
    {
        IsPressingForwards = Input.GetKey(KeyCode.W);
        IsPressingBack = Input.GetKey(KeyCode.S);
        IsPressingRight = Input.GetKey(KeyCode.D);
        IsPressingLeft = Input.GetKey(KeyCode.A);
        IsPressingSprint = Input.GetKey(KeyCode.LeftShift);

        HorizontalInput = Input.GetAxis("Horizontal");
        VerticalInput = Input.GetAxis("Vertical");

        MouseX = Input.GetAxis("Mouse X");
        MouseY = Input.GetAxis("Mouse Y");
    }
}