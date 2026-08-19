using System.Collections.Generic;
using System.Linq;
using RTS.Commands;
using RTS.EventBus;
using RTS.Events;
using RTS.Units;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace RTS.UI
{
    public class ActionsUI : MonoBehaviour
    {
        [SerializeField] private UIActionButton[] actionButtons;
        private HashSet<AbstractCommandable> _selectedUnits = new(12);

        private void Awake()
        {
            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
        }

        private void Start()
        {
            foreach (UIActionButton actionButton in actionButtons)
            {
                actionButton.Disable();
            }
        }

        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
        }

        private void HandleUnitSelected(UnitSelectedEvent e)
        {
            if (e.Unit is AbstractCommandable commandable)
            {
                _selectedUnits.Add(commandable);
                RefreshButtons();
            }
        }

        private void HandleUnitDeselected(UnitDeselectedEvent e)
        {
            if (e.Unit is AbstractCommandable commandable)
            {
                _selectedUnits.Remove(commandable);
                RefreshButtons();
            }
        }

        private void RefreshButtons()
        {
            HashSet<ActionBase> availableCommands = new(9);

            foreach (AbstractCommandable commandable in _selectedUnits)
            {
                availableCommands.AddRange(commandable.AvailableCommands);
            }

            for (int i = 0; i < actionButtons.Length; i++)
            {
                ActionBase actionForSlot = availableCommands.Where(action => action.Slot == i).FirstOrDefault();
                
                if (actionForSlot != null)
                {
                    actionButtons[i].EnableFor(actionForSlot, HandleClick(actionForSlot));
                }
                else
                {
                    actionButtons[i].Disable();
                }
            }
        }
        
        private UnityAction HandleClick(ActionBase action)
        {
            return () => Bus<ActionSelectedEvent>.Raise(new ActionSelectedEvent(action));
        }
    }
}