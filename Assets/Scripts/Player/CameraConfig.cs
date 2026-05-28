using UnityEngine;

namespace RTS.Player
{
    [System.Serializable]
    public class  CameraConfig 
    {
        [Header("Movement Settings")]
        [field: SerializeField] public bool EnableEdgeScrolling { get; set; } = true;
        [field: SerializeField] public float EdgeScrollThickness { get; set; } = 50f;
        [field: SerializeField] public float KeyboardPanSpeed { get; set; } = 1f;
        [field: SerializeField] public float MousePanSpeed { get; set; } = 5f;

        [Header("Zoom Settings")]
        [field: SerializeField] public float MinZoomDistance { get; set; } = 7.5f;
        [field: SerializeField] public float ZoomSpeed { get; set; } = 10f;

        [Header("Rotation Settings")]
        [field: SerializeField] public float RotationAmount { get; set; } = 90f; // Amount to rotate per key press
        [field: SerializeField] public float RotationSpeed { get; set; } = 10f;
    }
}
