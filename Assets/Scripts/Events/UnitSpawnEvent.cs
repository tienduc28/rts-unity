using RTS.EventBus;
using RTS.Units;

namespace RTS.Events
{
    public class UnitSpawnEvent : IEvent
    {
           public AbstractUnit Unit { get; private set; }
           
           public UnitSpawnEvent(AbstractUnit unit)
           {
               Unit = unit;
           }
    }
}