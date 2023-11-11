using System.Collections.Generic;
using UnityEngine;

namespace ObserverPattern
{
    public class Subject : MonoBehaviour
    {
        private readonly List<IObserver> _observers = new List<IObserver>();
        private readonly List<IObserverGrapple> _observerGrapple = new List<IObserverGrapple>();

        public void AddObserver(IObserver observer) {
            _observers.Add(observer);
        }

        public void AddObserverGrapple(IObserverGrapple observerGrapple) {
            _observerGrapple.Add(observerGrapple);
        }

        public void RemoveObserver(IObserver observer) {
            _observers.Remove(observer);
        }
        
        public void RemoveObserverGrapple(IObserverGrapple observerGrapple) {
            _observerGrapple.Remove(observerGrapple);
        }

        // This might be all very stupid but it works so improve it if you want
        protected void NewSceneNotification(string previousState, string currentState) {
            _observers.ForEach((observer) => { observer.OnSceneChangeNotify(previousState, currentState); });
        }

        protected void GrappleNotification(int type) {
            _observerGrapple.ForEach((observerGrapple) => { observerGrapple.OnGrappleNotify(type); });
        }
    }
}
