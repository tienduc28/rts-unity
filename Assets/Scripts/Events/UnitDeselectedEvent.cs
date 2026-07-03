using RTS.EventBus;
using RTS.Units;

namespace RTS.Events
{
    public class UnitDeselectedEvent : IEvent
    {
           public ISelectable Unit { get; private set; }
           
           public UnitDeselectedEvent(AbstractCommandable unit)
           {
               Unit = unit;
           }
    }
}