using UnityEngine;

namespace RTS.Units
{
    public class SupplyHut : AbstractCommandable
    {
        [field: SerializeField] public int Health { get; private set; }
    }
}

