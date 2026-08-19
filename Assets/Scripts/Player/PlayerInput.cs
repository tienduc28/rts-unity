using System.Collections.Generic;
using System.Linq;
using RTS.Commands;
using RTS.EventBus;
using RTS.Units;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using RTS.Events;
using UnityEngine.EventSystems;

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
        
        private ActionBase activeAction;
        private bool wasMouseDownOnUI;
        private CinemachineFollow cinemachineFollow;
        private float zoomStartTime;
        private float rotationStartTime;
        private Vector3 startingFollowOffset;
        private float maxRotationAmount;
        private HashSet<AbstractUnit> aliveUnits = new(100);
        private HashSet<AbstractUnit> addedUnits = new(24);
        private List<ISelectable> selectedUnits = new(12);

        private void Awake()
        {
            if (!cinemachineCamera.TryGetComponent(out cinemachineFollow))
            {
                Debug.LogError("Cinemachine Camera did not have CinemachineFollow. Zoom functionality will not work!");
            }

            startingFollowOffset = cinemachineFollow.FollowOffset;
            maxRotationAmount = Mathf.Abs(cinemachineFollow.FollowOffset.z);
            
            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
            Bus<UnitSpawnEvent>.OnEvent += HandleUnitSpawn;
            Bus<ActionSelectedEvent>.OnEvent += HandleActionSelected;
        }

        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
            Bus<UnitSpawnEvent>.OnEvent -= HandleUnitSpawn;
            Bus<ActionSelectedEvent>.OnEvent -= HandleActionSelected;
        }
        
        private void HandleUnitSpawn(UnitSpawnEvent e)
        {
            aliveUnits.Add(e.Unit);
        }
        
        private void HandleUnitSelected(UnitSelectedEvent e)
        {
            selectedUnits.Add(e.Unit);
        }
        
        private void HandleUnitDeselected(UnitDeselectedEvent e)
        {
            selectedUnits.Remove(e.Unit);
        }
        
        private void HandleActionSelected(ActionSelectedEvent e)
        {
            activeAction = e.Action;
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
            addedUnits.Clear();
            wasMouseDownOnUI = EventSystem.current.IsPointerOverGameObject();
        }

        private void HandleMouseDrag()
        {
            if (activeAction != null || wasMouseDownOnUI) return;
            
            Bounds selectionBoxBounds = ResizeSelectionBox();

            foreach (AbstractUnit unit in aliveUnits)
            {
                Vector2 unitPosition = camera.WorldToScreenPoint(unit.transform.position);
                if (selectionBoxBounds.Contains(unitPosition))
                {
                    addedUnits.Add(unit);
                }
            }
        }

        private void HandleMouseUp()
        {
            // select unit
            // deselect non-included units
            if (activeAction == null && !shiftAction.action.IsPressed())
            {
                DeselectedAllUnits();
            }
            HandleLeftClick();
            foreach (AbstractUnit unit in addedUnits)  
            {
                unit.Select();
            }
            // disable the ui
            selectionBox.gameObject.SetActive(false);
        }

        private void DeselectedAllUnits()
        {
            ISelectable[] selectedUnits = this.selectedUnits.ToArray();
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
            if (selectedUnits.Count == 0)
            {
                return;
            }
            
            if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, floorLayers))
                { 
                    List<AbstractUnit> abstractUnits = new (selectedUnits.Count);
                    foreach (ISelectable selectable in selectedUnits)
                    {
                        if (selectable is AbstractUnit unit)
                        {
                            abstractUnits.Add(unit);
                        }
                    }
                    
                    for (int i = 0; i < abstractUnits.Count; i++)
                    {
                        CommandContext context = new(abstractUnits[i], hit, i);
                        foreach (ICommand command in abstractUnits[i].AvailableCommands)
                        {
                            if (command.CanHandle(context))
                            {
                                command.Handle(context);
                                break;
                            }
                        }
                    }
                }
            }
        }
        
        private void HandleLeftClick()
        {
            if (activeAction != null && activeAction.RequiresClickToActivate && !EventSystem.current.IsPointerOverGameObject())
            {
                Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, floorLayers))
                {   
                    
                    List<AbstractUnit> abstractUnits = selectedUnits
                        .Where((unit) => unit is AbstractUnit)
                        .Cast<AbstractUnit>()
                        .ToList();
                        
                    for (int i = 0; i < abstractUnits.Count; i++)
                    {
                        CommandContext context = new(abstractUnits[i], hit, i);
                        if (activeAction.CanHandle(context))
                        {
                            activeAction.Handle(context);
                        }
                    }

                    activeAction = null;
                }
            }
            else
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
        }

        private void HandleRotation()
        {
            if (ShouldSetRotationStartTime())
            {
                rotationStartTime = Time.time;
            }

            float rotationTime = Mathf.Clamp01((Time.time - rotationStartTime) * cameraConfig.RotationSpeed);

            Vector3 targetFollowOffset;

            if (Keyboard.current.pageDownKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                    maxRotationAmount,
                    cinemachineFollow.FollowOffset.y,
                    0
                );
            }
            else if (Keyboard.current.pageUpKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                    -maxRotationAmount,
                    cinemachineFollow.FollowOffset.y,
                    0
                );
            }
            else
            {
                targetFollowOffset = new Vector3(
                    startingFollowOffset.x,
                    cinemachineFollow.FollowOffset.y,
                    startingFollowOffset.z
                );
            }

            cinemachineFollow.FollowOffset = Vector3.Slerp(
                cinemachineFollow.FollowOffset,
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
                zoomStartTime = Time.time;
            }

            float zoomTime = Mathf.Clamp01((Time.time - zoomStartTime) * cameraConfig.ZoomSpeed);
            Vector3 targetFollowOffset;

            if (Keyboard.current.endKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                    cinemachineFollow.FollowOffset.x,
                    cameraConfig.MinZoomDistance,
                    cinemachineFollow.FollowOffset.z
                );
            }
            else
            {
                targetFollowOffset = new Vector3(
                    cinemachineFollow.FollowOffset.x,
                    startingFollowOffset.y,
                    cinemachineFollow.FollowOffset.z
                );
            }

            cinemachineFollow.FollowOffset = Vector3.Slerp(
                cinemachineFollow.FollowOffset,
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
