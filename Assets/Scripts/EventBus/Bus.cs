namespace RTS.EventBus
{
    public static class Bus<T> where T : IEvent
    {
        public delegate void Event(T args);

        public static Event OnEvent ;
        
        public static void Raise(T args)
        {
            OnEvent?.Invoke(args);
        }
    }
}