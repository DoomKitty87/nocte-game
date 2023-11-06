namespace ObserverPattern
{
    public interface IObserver
    {
        public void OnNotify(string previousState, string currentState);
    }
}
