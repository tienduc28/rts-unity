using RTS.EventBus;
using RTS.Events;
using RTS.Units;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RTS.Units
{
    public class SupplyHut : MonoBehaviour, ISelectable
    {
        [SerializeField] private DecalProjector decalProjector;
        [field: SerializeField] public int Health { get; private set; }
        public void Select()
        {
            if (decalProjector != null)
            {
                decalProjector.gameObject.SetActive(true);   
            }
            
            Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
        }

        public void Deselect()
        {
            if (decalProjector != null)
            {
                decalProjector.gameObject.SetActive(false);   
            }
            
            Bus<UnitDeselectedEvent>.Raise(new UnitDeselectedEvent(this));
        }
    }
}

