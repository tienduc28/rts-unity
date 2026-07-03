using RTS.EventBus;
using RTS.Units;

namespace RTS.Events
{
    public class UnitSelectedEvent : IEvent
    {
           public ISelectable Unit { get; private set; }
           
           public UnitSelectedEvent(AbstractCommandable unit)
           {
               Unit = unit;
           }
    }
}