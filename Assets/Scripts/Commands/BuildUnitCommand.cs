using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    [CreateAssetMenu(fileName = "Build Unit", menuName = "Buildings/Commands/Build Unit", order = 120)]
    public class BuildUnitCommand : ActionBase
    {
        [field: SerializeField] private UnitSO Unit;

        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is BaseBuilding;
        }

        public override void Handle(CommandContext context)
        {
            BaseBuilding building = context.Commandable as BaseBuilding;
            building.BuildUnit(Unit);
        }
    }
}