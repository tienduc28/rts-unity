/*using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RTS.Player
{
    public class SelectionManager : MonoBehaviour
    {
        private InputSystem_Actions _inputActions;
        private Vector2 _dragStartPosition;
        private bool _isDragging = false;
        private Rect _selectionRect;
        private List<Worker> _selectedUnits = new List<Worker>();
        private const float DRAG_THRESHOLD = 5f;

        private void Awake()
        {
            _inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _inputActions.Enable();
            _inputActions.Player.Attack.started += OnAttackStarted;
            _inputActions.Player.Attack.canceled += OnAttackCanceled;
        }

        private void OnDisable()
        {
            _inputActions.Player.Attack.canceled -= OnAttackCanceled;
            _inputActions.Player.Attack.started -= OnAttackStarted;
            _inputActions.Disable();
        }

        private void OnAttackStarted(InputAction.CallbackContext context)
        {
            _dragStartPosition = Mouse.current.position.ReadValue();
            _isDragging = true;
        }

        private void OnAttackCanceled(InputAction.CallbackContext context)
        {
            Vector2 currentMousePosition = Mouse.current.position.ReadValue();
            _isDragging = false;

            if (Vector2.Distance(_dragStartPosition, currentMousePosition) < DRAG_THRESHOLD)
            {
                // It's a click, not a drag
                HandleSingleClickSelection(currentMousePosition);
            }
            else
            {
                // It's a drag
                SelectUnitsInRectangle();
            }
        }

        private void Update()
        {
            if (_isDragging)
            {
                Vector2 currentMousePosition = Mouse.current.position.ReadValue();
                float x = Mathf.Min(_dragStartPosition.x, currentMousePosition.x);
                float y = Mathf.Min(Screen.height - _dragStartPosition.y, Screen.height - currentMousePosition.y);
                float width = Mathf.Abs(_dragStartPosition.x - currentMousePosition.x);
                float height = Mathf.Abs(_dragStartPosition.y - currentMousePosition.y);
                _selectionRect = new Rect(x, y, width, height);
            }
        }

        private void OnGUI()
        {
            if (_isDragging)
            {
                // Draw the selection rectangle
                GUI.Box(_selectionRect, "");
            }
        }

        private void DeselectAllUnits()
        {
            foreach (Worker worker in _selectedUnits)
            {
                worker.Deselect();
            }

            _selectedUnits.Clear();
        }

        private void HandleSingleClickSelection(Vector2 clickPosition)
        {
            DeselectAllUnits();

            Ray ray = Camera.main.ScreenPointToRay(clickPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Worker clickedWorker = hit.collider.GetComponent<Worker>();
                if (clickedWorker != null)
                {
                    _selectedUnits.Add(clickedWorker);
                    clickedWorker.Select();
                }
            }
        }

        private void SelectUnitsInRectangle()
        {
            DeselectAllUnits();
            // Find all selectable units (for now, just Workers)
            Worker[] allWorkers = FindObjectsOfType<Worker>();
            foreach (Worker worker in allWorkers)
            {
                // Convert world position to screen position
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(worker.transform.position);
                // Invert Y for GUI coordinates
                screenPosition.y = Screen.height - screenPosition.y;

                if (_selectionRect.Contains(screenPosition))
                {
                    _selectedUnits.Add(worker);
                    worker.Select(); // Assuming Worker has a Select method
                }
                else
                {
                    worker.Deselect(); // Deselect if not in selection
                }
            }
        }
    }
}*/