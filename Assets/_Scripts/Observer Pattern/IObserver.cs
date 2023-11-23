namespace ObserverPattern
{
    public interface IObserver
    {
        public void OnSceneChangeNotify(string firstString, string secondString);
        
    }
}
