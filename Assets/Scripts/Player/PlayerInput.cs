using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float keyboardPanSpeed = 5f;
    [SerializeField] private float minZoomDistance = 7.5f;
    [SerializeField] private CinemachineCamera cinemachineCamera;

    [SerializeField] private CinemachineFollow cinemachineFollow;
    private Vector3 startFollowOffset;
    private float zoomStartTime;

    private void Awake()
    {
        if (!cinemachineCamera.TryGetComponent(out cinemachineFollow))
        {
            Debug.LogError("Cinemachine Camera did not have CinemachineFollow. Zoom functionality will not work!");
        }

        //cinemachineFollow = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFollow>();
        startFollowOffset = cinemachineFollow.FollowOffset;
    }
    private void Update()
    {
        HandlePanning();
        HandleZooming();    
    }

    private void HandlePanning()
    {
        Vector2 moveAmount = Vector2.zero;
        if (Keyboard.current.upArrowKey.isPressed)
        {
            moveAmount.x -= 1;
            Debug.Log("Up Arrow Pressed");
        }
        if (Keyboard.current.downArrowKey.isPressed)
        {
            moveAmount.x += 1;
            Debug.Log("Down Arrow Pressed");
        }
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            moveAmount.y -= 1;
            Debug.Log("Left Arrow Pressed");
        }
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            moveAmount.y += 1;
            Debug.Log("Right Arrow Pressed");
        }
        
        moveAmount *= Time.deltaTime * keyboardPanSpeed;
        cameraTarget.position += new Vector3(moveAmount.x, 0, moveAmount.y);
    }

    private void HandleZooming()
    {
        if (Keyboard.current.endKey.wasPressedThisFrame)
        {
            zoomStartTime = Time.time;
            Debug.Log("End Key Was Pressed");
        }

        Vector3 targetOffset = new Vector3(
            cinemachineFollow.FollowOffset.x,
            minZoomDistance,
            cinemachineFollow.FollowOffset.z
        );



        if (Keyboard.current.endKey.isPressed)
        {
            cinemachineFollow.FollowOffset = new Vector3(
                startFollowOffset.x,
                
                (Time.time - zoomStartTime) * 0.5f
                );
            Debug.Log("End Key Is Pressed");
        }
    }
}
