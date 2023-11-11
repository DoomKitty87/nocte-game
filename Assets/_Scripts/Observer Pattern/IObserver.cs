namespace ObserverPattern
{
    public interface IObserver
    {
        public void OnSceneChangeNotify(string firstString, string secondString);
        
    }

    public interface IObserverGrapple
    {
        public void OnGrappleNotify(int type);
    }
}
