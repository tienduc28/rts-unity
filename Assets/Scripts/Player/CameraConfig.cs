using UnityEngine;

namespace RTS.Player
{
    [System.Serializable]
    public class  CameraConfig 
    {
        [Header("Movement Settings")]
        [field: SerializeField] public bool enableEdgeScrolling { get; set; } = true;
        [field: SerializeField] public float edgeScrollThickness { get; set; } = 50f;
        [field: SerializeField] public float keyboardPanSpeed { get; set; } = 1f;
        [field: SerializeField] public float mousePanSpeed { get; set; } = 5f;

        [Header("Zoom Settings")]
        [field: SerializeField] public float minZoomDistance { get; set; } = 7.5f;
        [field: SerializeField] public float zoomSpeed { get; set; } = 10f;

        [Header("Rotation Settings")]
        [field: SerializeField] public float rotationAmount { get; set; } = 90f; // Amount to rotate per key press
        [field: SerializeField] public float rotationSpeed { get; set; } = 10f;
    }
}
