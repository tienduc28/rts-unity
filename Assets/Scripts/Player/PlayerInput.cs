using System.Collections.Generic;
using RTS.EventBus;
using RTS.Units;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using RTS.Events;

namespace RTS.Player
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private Rigidbody cameraTarget;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private new Camera camera;
        [SerializeField] private CameraConfig cameraConfig;
        [SerializeField] private LayerMask selectableUnitLayers;
        [SerializeField] private LayerMask floorLayers;
        [SerializeField] private RectTransform selectionBox;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference leftClickAction;
        [SerializeField] private InputActionReference rightClickAction;
        [SerializeField] private InputActionReference shiftAction;
        

        private Vector2 _startingMousePosition;
        
        private CinemachineFollow _cinemachineFollow;
        private float _zoomStartTime;
        private float _rotationStartTime;
        private Vector3 _startingFollowOffset;
        private float _maxRotationAmount;
        private HashSet<AbstractUnit> _aliveUnits = new(100);
        private HashSet<AbstractUnit> _addedUnits = new(24);
        private List<ISelectable> _selectedUnits = new(12);
        

        private void Awake()
        {
            if (!cinemachineCamera.TryGetComponent(out _cinemachineFollow))
            {
                Debug.LogError("Cinemachine Camera did not have CinemachineFollow. Zoom functionality will not work!");
            }

            _startingFollowOffset = _cinemachineFollow.FollowOffset;
            _maxRotationAmount = Mathf.Abs(_cinemachineFollow.FollowOffset.z);
            
            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
            Bus<UnitSpawnEvent>.OnEvent += HandleUnitSpawn;
        }

        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
            Bus<UnitSpawnEvent>.OnEvent -= HandleUnitSpawn;
        }
        
        private void HandleUnitSpawn(UnitSpawnEvent args)
        {
            _aliveUnits.Add(args.Unit);
        }
        
        private void HandleUnitSelected(UnitSelectedEvent args)
        {
            _selectedUnits.Add(args.Unit);
        }
        
        private void HandleUnitDeselected(UnitDeselectedEvent args)
        {
            _selectedUnits.Remove(args.Unit);
        }

        private void Update()
        {
            HandlePanning();
            HandleZooming();
            HandleRotation();
            HandleRightClick();
            HandleDragSelect();
        }

        private void HandleDragSelect()
        {
            if (selectionBox == null)
            {
                return;
            }
            if (leftClickAction.action.WasPressedThisFrame())
            {       
                HandleMouseDown();
            }
            else if (leftClickAction.action.IsPressed() && !leftClickAction.action.WasReleasedThisFrame())
            {
                HandleMouseDrag();
            }
            else if (leftClickAction.action.WasReleasedThisFrame())
            {
                HandleMouseUp();
            }
        }

        private void HandleMouseDown()
        {
            selectionBox.sizeDelta = Vector2.zero;                              // Reset the selection box
            selectionBox.gameObject.SetActive(true);                            // Enable UI
            _startingMousePosition = Mouse.current.position.ReadValue();        // Store start position
            //Debug.Log("Drag select started" + _startingMousePosition);
            _addedUnits.Clear();
        }

        private void HandleMouseDrag()
        {
            Bounds selectionBoxBounds = ResizeSelectionBox();

            foreach (AbstractUnit unit in _aliveUnits)
            {
                Vector2 unitPosition = camera.WorldToScreenPoint(unit.transform.position);
                if (selectionBoxBounds.Contains(unitPosition))
                {
                    _addedUnits.Add(unit);
                }
            }
        }

        private void HandleMouseUp()
        {
            // select unit
            // deselect non-included units
            if (!shiftAction.action.IsPressed())
            {
                DeselectedAllUnits();
            }
            HandleLeftClick();
            foreach (AbstractUnit unit in _addedUnits)  
            {
                unit.Select();
            }
            // disable the ui
            selectionBox.gameObject.SetActive(false);
        }

        private void DeselectedAllUnits()
        {
            ISelectable[] selectedUnits = _selectedUnits.ToArray();
            foreach (ISelectable unit in selectedUnits)
            {
                unit.Deselect();
            }
        }

        private Bounds ResizeSelectionBox()
        {
            // Resize the box
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            float width = mousePosition.x - _startingMousePosition.x;
            float height = mousePosition.y - _startingMousePosition.y;
            
            //Debug.Log($"Resizing selection box to width: {width}, height: {height}");
                
            selectionBox.anchoredPosition = _startingMousePosition + new Vector2(width / 2, height / 2);
            selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
            
            return new Bounds(selectionBox.anchoredPosition, selectionBox.sizeDelta);
        }

        private void HandleRightClick()
        {
            if (_selectedUnits.Count == 0)
            {
                return;
            }
            
            if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, floorLayers))
                { 
                    List<AbstractUnit> abtractUnits = new (_selectedUnits.Count);
                    foreach (ISelectable selectable in _selectedUnits)
                    {
                        if (selectable is AbstractUnit unit)
                        {
                            abtractUnits.Add(unit);
                        }
                    }

                    int unitsOnLayer = 0;
                    int maxUnitsOnLayer = 1;
                    float circleRadius = 0;
                    float radialOffset = 0;

                    foreach (AbstractUnit unit in abtractUnits)
                    {
                        Vector3 targetPosition = new Vector3(
                            hit.point.x + circleRadius * Mathf.Cos(radialOffset * unitsOnLayer),
                            hit.point.y,
                            hit.point.z + circleRadius * Mathf.Sin(radialOffset * unitsOnLayer)
                        );
                        unit.MoveTo(targetPosition);
                        unitsOnLayer++;
                        
                        if (unitsOnLayer >= maxUnitsOnLayer)
                        {
                            unitsOnLayer = 0;
                            circleRadius += unit.AgentRadius * 3.5f;
                            maxUnitsOnLayer = Mathf.FloorToInt(2 * Mathf.PI * circleRadius / (2 * unit.AgentRadius));
                            radialOffset = 2 * Mathf.PI / maxUnitsOnLayer;
                        }
                    }
                    /*foreach (ISelectable selectedUnit in _selectedUnits)
                    {
                        if (selectedUnit is IMoveable moveable)
                        {
                            moveable.MoveTo(hit.point);
                        }
                    }*/
                }
            }
        }
        
        private void HandleLeftClick()
        {
            if (camera == null)
            {
                return;
            }
            
            Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, selectableUnitLayers)
                && hit.collider.TryGetComponent(out ISelectable selectable))
            {
                // select the worker
                selectable.Select();
            }
        }

        private void HandleRotation()
        {
            if (ShouldSetRotationStartTime())
            {
                _rotationStartTime = Time.time;
            }

            float rotationTime = Mathf.Clamp01((Time.time - _rotationStartTime) * cameraConfig.RotationSpeed);

            Vector3 targetFollowOffset;

            if (Keyboard.current.pageDownKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                    _maxRotationAmount,
                    _cinemachineFollow.FollowOffset.y,
                    0
                );
            }
            else if (Keyboard.current.pageUpKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                    -_maxRotationAmount,
                    _cinemachineFollow.FollowOffset.y,
                    0
                );
            }
            else
            {
                targetFollowOffset = new Vector3(
                    _startingFollowOffset.x,
                    _cinemachineFollow.FollowOffset.y,
                    _startingFollowOffset.z
                );
            }

            _cinemachineFollow.FollowOffset = Vector3.Slerp(
                _cinemachineFollow.FollowOffset,
                targetFollowOffset,
                rotationTime
            );
        }

        private bool ShouldSetRotationStartTime()
        {
            return Keyboard.current.pageUpKey.wasPressedThisFrame
                || Keyboard.current.pageDownKey.wasPressedThisFrame
                || Keyboard.current.pageUpKey.wasReleasedThisFrame
                || Keyboard.current.pageDownKey.wasReleasedThisFrame;
        }

        private void HandleZooming()
        {
            if (ShouldSetZoomStartTime())
            {
                _zoomStartTime = Time.time;
            }

            float zoomTime = Mathf.Clamp01((Time.time - _zoomStartTime) * cameraConfig.ZoomSpeed);
            Vector3 targetFollowOffset;

            if (Keyboard.current.endKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                    _cinemachineFollow.FollowOffset.x,
                    cameraConfig.MinZoomDistance,
                    _cinemachineFollow.FollowOffset.z
                );
            }
            else
            {
                targetFollowOffset = new Vector3(
                    _cinemachineFollow.FollowOffset.x,
                    _startingFollowOffset.y,
                    _cinemachineFollow.FollowOffset.z
                );
            }

            _cinemachineFollow.FollowOffset = Vector3.Slerp(
                _cinemachineFollow.FollowOffset,
                targetFollowOffset,
                zoomTime
            );
        }

        private bool ShouldSetZoomStartTime()
        {
            return Keyboard.current.endKey.wasPressedThisFrame
                || Keyboard.current.endKey.wasReleasedThisFrame;
        }

        private void HandlePanning()
        {
            Vector2 moveAmount = GetKeyboardMoveAmount();
            moveAmount += GetMouseMoveAmount();

            cameraTarget.linearVelocity = new Vector3(moveAmount.x, 0, moveAmount.y);
        }

        private Vector2 GetMouseMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;

            if (!cameraConfig.EnableEdgeScrolling) { return moveAmount; }

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            int screenWidth = Screen.width;
            int screenHeight = Screen.height;

            if (mousePosition.x <= cameraConfig.EdgeScrollThickness)
            {
                moveAmount.x -= cameraConfig.MousePanSpeed;
            }
            else if (mousePosition.x >= screenWidth - cameraConfig.EdgeScrollThickness)
            {
                moveAmount.x += cameraConfig.MousePanSpeed;
            }

            if (mousePosition.y >= screenHeight - cameraConfig.EdgeScrollThickness)
            {
                moveAmount.y += cameraConfig.MousePanSpeed;
            }
            else if (mousePosition.y <= cameraConfig.EdgeScrollThickness)
            {
                moveAmount.y -= cameraConfig.MousePanSpeed;
            }

            return moveAmount;
        }

        private Vector2 GetKeyboardMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;

            if (Keyboard.current.upArrowKey.isPressed)
            {
                moveAmount.y += cameraConfig.KeyboardPanSpeed;
            }
            if (Keyboard.current.leftArrowKey.isPressed)
            {
                moveAmount.x -= cameraConfig.KeyboardPanSpeed;
            }
            if (Keyboard.current.downArrowKey.isPressed)
            {
                moveAmount.y -= cameraConfig.KeyboardPanSpeed;
            }
            if (Keyboard.current.rightArrowKey.isPressed)
            {
                moveAmount.x += cameraConfig.KeyboardPanSpeed;
            }

            return moveAmount;
        }
    }
}
