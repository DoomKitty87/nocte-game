using System.Collections.Generic;
using UnityEngine;

namespace ObserverPattern
{
    public class Subject : MonoBehaviour
    {
        private List<IObserver> _observers = new List<IObserver>();

        public void AddObserver(IObserver observer) {
            _observers.Add(observer);
        }

        public void RemoveObserver(IObserver observer) {
            _observers.Remove(observer);
        }

        // This might be all very stupid but it works so improve it if you want
        protected void NotifyObservers(string previousState, string currentState) {
            _observers.ForEach((observer) => { observer.OnNotify(previousState, currentState); });
        }
    }
}
