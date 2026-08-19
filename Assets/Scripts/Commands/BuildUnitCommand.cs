using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    [CreateAssetMenu(fileName = "Build Unit", menuName = "Buildings/Commands/Build Unit", order = 120)]
    public class BuildUnitCommand : ActionBase
    {
        [field: SerializeField] private UnitSO unitSO;

        public override bool CanHandle(CommandContext context)
        {
            throw new System.NotImplementedException();
        }

        public override void Handle(CommandContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}