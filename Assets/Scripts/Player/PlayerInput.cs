using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RTS.Player
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private CameraConfig cameraConfig;

        private CinemachineFollow cinemachineFollow;

        // Original states
        private Vector3 startFollowOffset;

        // Target states
        private bool isZoomedIn = false;
        private Vector3 targetRotationEuler; // Tracks the current target angle

        private void Awake()
        {
            if (!cinemachineCamera.TryGetComponent(out cinemachineFollow))
            {
                Debug.LogError("Cinemachine Camera did not have CinemachineFollow. Zoom functionality will not work!");
            }

            // Store original values when the game starts
            startFollowOffset = cinemachineFollow.FollowOffset;

            // Initialize our rotation target to whatever the camera starts at
            targetRotationEuler = cameraTarget.eulerAngles;
        }

        private void Update()
        {
            HandlePanning();
            HandleZooming();
            HandleRotation();

            GetMouseMoveAmount();
        }

        private void HandlePanning()
        {
            // Get input from 2 separated methods
            Vector2 keyboardMove = GetKeyboardMoveAmount();   
            Vector2 mouseMove = GetMouseMoveAmount();

            Vector3 finalMove = Vector3.zero;

            // Apply whichever input is being used
            if (keyboardMove != Vector2.zero)
            {
                keyboardMove.Normalize();
                Debug.Log("Keyboard Move: " + keyboardMove);
                finalMove += new Vector3(keyboardMove.x, 0, keyboardMove.y) * cameraConfig.keyboardPanSpeed * Time.deltaTime;
            }

            if (mouseMove != Vector2.zero)
            {
                mouseMove.Normalize();
                Debug.Log("Mouse Move: " + mouseMove);
                finalMove += new Vector3(mouseMove.x, 0, mouseMove.y) * cameraConfig.mousePanSpeed * Time.deltaTime;
            }

            // Apply movement
            cameraTarget.position += finalMove;
        }

        private Vector2 GetKeyboardMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;
            if (Keyboard.current.upArrowKey.isPressed)
            {
                moveAmount.y += cameraConfig.keyboardPanSpeed;
            }
            if (Keyboard.current.downArrowKey.isPressed)
            {
                moveAmount.y -= cameraConfig.keyboardPanSpeed;
            }
            if (Keyboard.current.leftArrowKey.isPressed)
            {
                moveAmount.x -= cameraConfig.keyboardPanSpeed;
            }
            if (Keyboard.current.rightArrowKey.isPressed)
            {
                moveAmount.x += cameraConfig.keyboardPanSpeed;
            }
            return moveAmount;
        }

        private Vector2 GetMouseMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;
            //Debug.Log("Mouse Position: " + Mouse.current.position.ReadValue());
            Vector2 mousePos = Mouse.current.position.ReadValue();
            // Safety check: ensure the mouse is actually inside the game window.
            // (Prevents accidental scrolling when switching to a second monitor)
            if (mousePos.x < 0 || mousePos.x > Screen.width || mousePos.y < 0 || mousePos.y > Screen.height)
            {
                return moveAmount;
            }

            // Left Edge
            if (mousePos.x < cameraConfig.edgeScrollThickness)
            {
                moveAmount.x -= cameraConfig.mousePanSpeed;
            }
            // Right Edge
            else if (mousePos.x > Screen.width - cameraConfig.edgeScrollThickness)
            {
                moveAmount.x += cameraConfig.mousePanSpeed;
            }

            // Bottom Edge
            if (mousePos.y < cameraConfig.edgeScrollThickness)
            {
                moveAmount.y -= cameraConfig.mousePanSpeed;
            }
            // Top Edge
            else if (mousePos.y > Screen.height - cameraConfig.edgeScrollThickness)
            {
                moveAmount.y += cameraConfig.mousePanSpeed;
            }

            return moveAmount;
        }

        private void HandleZooming()
        {
            // 1. Check for a single button press to toggle zooming in and out
            if (Keyboard.current.endKey.wasPressedThisFrame)
            {
                isZoomedIn = !isZoomedIn;
            }

            // 2. Determine where the camera should be based on the state of zooming
            Vector3 targetOffset;
            if (isZoomedIn)
            {
                // Target position for Zoom in
                targetOffset = new Vector3(
                    cinemachineFollow.FollowOffset.x,
                    cameraConfig.minZoomDistance,
                    cinemachineFollow.FollowOffset.z
                );
            }
            else
            {
                // Target position for Zoom out
                targetOffset = new Vector3(
                    cinemachineFollow.FollowOffset.x,
                    startFollowOffset.y,
                    cinemachineFollow.FollowOffset.z
                );
            }

            //  3. Move the camera towards the target position smoothly
            cinemachineFollow.FollowOffset = Vector3.Lerp(
                cinemachineFollow.FollowOffset,
                targetOffset,
                Time.deltaTime * cameraConfig.zoomSpeed
            );

            // Once it is close enough to the target, snap exactly to it
            if (Vector3.Distance(cinemachineFollow.FollowOffset, targetOffset) < 0.01f)
            {
                cinemachineFollow.FollowOffset = targetOffset;
            }
        }

        private void HandleRotation()
        {
            // 1. Check for PageDown (Rotate Right/Positive)
            if (Keyboard.current.pageDownKey.wasPressedThisFrame)
            {
                targetRotationEuler.y += cameraConfig.rotationAmount;
            }

            // 2. Check for PageUp (Rotate Left/Negative)
            if (Keyboard.current.pageUpKey.wasPressedThisFrame)
            {
                targetRotationEuler.y -= cameraConfig.rotationAmount;
            }

            // Convert our Euler angles target into a Quaternion
            Quaternion targetRot = Quaternion.Euler(targetRotationEuler);

            // 3. Smoothly rotate towards the target using Slerp
            cameraTarget.rotation = Quaternion.Slerp(
                cameraTarget.rotation,
                targetRot,
                Time.deltaTime * cameraConfig.rotationSpeed
            );

            // Optional: Snap exactly to target when very close
            if (Quaternion.Angle(cameraTarget.rotation, targetRot) < 0.1f)
            {
                cameraTarget.rotation = targetRot;
            }
        }
    }

}
