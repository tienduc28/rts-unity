using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    public abstract class ActionBase: ScriptableObject, ICommand
    {
        [field:SerializeField] public Sprite Icon { get; protected set; }
        [field: Range(0, 8)] [field:SerializeField] public int Slot { get; protected set; }
        [field:SerializeField] public bool RequiresClickToActivate { get; protected set; }
        
        public abstract bool CanHandle(CommandContext context);
        public abstract void Handle(CommandContext context);
    }
}