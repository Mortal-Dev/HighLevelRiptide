using HLRiptide;
using UnityEngine;

public class PlayerCameraController : NetworkedBehaviour
{
    private Camera mainCamera;
    public Transform playerCameraTransform;
    public PlayerController playerController;

    public float mouseSensitivity;

    private LocalPlayerInputInfo localPlayerInputInfo;

    private float xRotation = 0f;

    public override void OnStart()
    {
        if (!IsLocalPlayerWithAuthority) return;

        localPlayerInputInfo = GetComponentInParent<LocalPlayerInputInfo>();

        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (!IsLocalPlayerWithAuthority) return;

        if (mainCamera != null && playerCameraTransform != null && playerController != null)
        {
            mainCamera.transform.position = playerCameraTransform.position;
            mainCamera.transform.rotation = playerCameraTransform.rotation;

            xRotation -= localPlayerInputInfo.MouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
            playerController.transform.Rotate(Vector3.up * localPlayerInputInfo.MouseX * mouseSensitivity * Time.deltaTime);
        }
    }

    public override void OnRegisterCommands()
    {
        
    }
}