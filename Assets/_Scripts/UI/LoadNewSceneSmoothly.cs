using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadNewSceneSmoothly : MonoBehaviour
{
    // Will darken the screen, fade out the audio, and load the new scene 
    [SerializeField] private string _sceneToLoadName;
    [SerializeField] private AnimationCurve _fadeCurve;
    [SerializeField] private float _fadeDuration = 1;
    
    private GameObject _darkenObject;
    private CanvasGroup _canvasGroup;
    private List<AudioSource> _audioSources;
    
    private bool _parentAlreadyDDOL = false;
    
    private bool _loading = false;
    
    private void InitalizeDarken() {
        _darkenObject = new GameObject("DarkenCanvas");
        Canvas canvas = _darkenObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        canvas.gameObject.AddComponent<CanvasRenderer>();
        _canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
        Image image = new GameObject("DarkenImage").AddComponent<Image>();
        image.transform.SetParent(_darkenObject.transform);
        image.rectTransform.anchorMin = Vector2.zero;
        image.rectTransform.anchorMax = Vector2.one * 2;
        image.rectTransform.sizeDelta = Vector2.one;
        image.color = Color.black;
        DontDestroyOnLoad(_darkenObject);
    }

    private List<AudioSource> FindAllAudioSources() {
        List<AudioSource> audioSources = new List<AudioSource>();
        foreach (AudioSource audioSource in FindObjectsByType<AudioSource>(FindObjectsSortMode.None)) {
            audioSources.Add(audioSource);
        }
        return audioSources;
    }
    
    private IEnumerator LoadNewSceneCoroutine() {
        _loading = true;
        InitalizeDarken();
        _audioSources = FindAllAudioSources();
        float t = 0;
        while (t < _fadeDuration) {
            _canvasGroup.alpha = _fadeCurve.Evaluate(t / _fadeDuration);
            foreach (AudioSource source in _audioSources) {
                source.volume = 1 - _fadeCurve.Evaluate(t / _fadeDuration);
            }
            t += Time.deltaTime;
            yield return null;
        }
        _canvasGroup.alpha = 1;
        _audioSources.Clear();
        SceneManager.LoadScene(_sceneToLoadName);
        while (!SceneManager.GetSceneByName(_sceneToLoadName).isLoaded) {
            yield return null;
        }
        t = 0;
        _audioSources = FindAllAudioSources();
        while (t < _fadeDuration) {
            _canvasGroup.alpha = 1 - _fadeCurve.Evaluate(t / _fadeDuration);
            foreach (AudioSource source in _audioSources) {
                source.volume = _fadeCurve.Evaluate(t / _fadeDuration);
            }
            t += Time.deltaTime;
            yield return null;
        }
        _loading = false;
        Destroy(_darkenObject);
        if (!_parentAlreadyDDOL) {
            Destroy(gameObject);
        }
        else {
            Destroy(this);
        }
    }
    public void LoadNewScene() {
        if (_loading) return;
        if (gameObject.scene.name == "DontDestroyOnLoad") {
            _parentAlreadyDDOL = true;
        }
        else {
            DontDestroyOnLoad(this);
        }
        StartCoroutine(LoadNewSceneCoroutine());
    }
}
