using System;
using System.Collections.Generic;
using UnityEngine;

public class UpdateManager : MonoBehaviour
{

  private struct Subscriber {

    public Action call;
    public int interval;
    public bool enabled;

  }

  private List<Subscriber> _subscribers = new List<Subscriber>();
  private Subscriber[] _activeSubscribers;

  private void Update() {
    int frames = Time.frameCount;
    for (int i = 0; i < _activeSubscribers.Length; i++) {
      if (frames % _activeSubscribers[i].interval == 0) _activeSubscribers[i].call();
    }
  }

  private void UpdateActive() {
    int active = 0;
    for (int i = 0; i < _subscribers.Count; i++) {
      if (_subscribers[i].enabled) active++;
    }
    _activeSubscribers = new Subscriber[active];
    for (int i = 0, j = 0; i < _subscribers.Count; i++) {
      if (!_subscribers[i].enabled) continue;
      _activeSubscribers[j] = _subscribers[i];
      j++;
    }
  }

  public int Subscribe(Action call, int interval) {
    Subscriber adding = new Subscriber();
    adding.call = call;
    adding.interval = interval;
    adding.enabled = true;
    _subscribers.Add(adding);
    UpdateActive();
    return _subscribers.Count - 1;
  }

  public void Deactivate(int index) {
    // _subscribers[index].enabled = false;
    UpdateActive();
  }

  public void UpdateInterval(int index, int interval) {
    // _subscribers[index].interval = interval;
  }
}