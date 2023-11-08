namespace ObserverPattern
{
    public interface IObserver
    {
        public void OnNotify(string firstString, string secondString);
    }
}
