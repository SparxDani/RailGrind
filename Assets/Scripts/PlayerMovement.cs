using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Vector3 input;
    [SerializeField] Transform cameraObj;
    [SerializeField] float rotSpeed = 5f;
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] CharacterController charController;

    private UDPManager udpManager;
    private PlayerGrind grindScript;

    private void Start()
    {
        cameraObj = FindObjectOfType<Camera>().transform;
        charController = GetComponent<CharacterController>();
        grindScript = GetComponent<PlayerGrind>();

        udpManager = FindObjectOfType<UDPManager>();
        if (udpManager == null)
        {
            Debug.LogError("No se encontró un UDPManager en la escena.");
        }
    }

    public void HandleMovement(InputAction.CallbackContext context)
    {
        Vector2 rawInput = context.ReadValue<Vector2>();
        input.x = rawInput.x;
        input.z = rawInput.y;

        string hexX = BitConverter.ToString(BitConverter.GetBytes(rawInput.x));
        string hexZ = BitConverter.ToString(BitConverter.GetBytes(rawInput.y));

        //string message = $"Input X (Hex): {hexX}, Input Z (Hex): {hexZ}";
        //Debug.Log(message);
        //udpManager?.sendString(message);
    }

    private void Update()
    {
        if (!grindScript.onRail)
        {
            HandleRotation();
            Vector3 forward = cameraObj.forward;
            forward.y = 0;
            transform.forward = forward;
            Vector3 movement = input * (moveSpeed * Time.deltaTime);
            charController.Move(transform.TransformDirection(movement));
        }
    }

    void HandleRotation()
    {
        Vector3 targetDir = Vector3.zero;
        targetDir = cameraObj.forward * input.z;
        targetDir += cameraObj.right * input.x;
        targetDir.Normalize();
        targetDir.y = 0;

        if (targetDir == Vector3.zero)
        {
            targetDir = transform.forward;
        }

        Quaternion targetRot = Quaternion.LookRotation(targetDir);
        Quaternion playerRot = Quaternion.Slerp(transform.rotation, targetRot, rotSpeed * Time.deltaTime);

        transform.rotation = playerRot;
    }
}
