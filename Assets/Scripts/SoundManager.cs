using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SoundManager : MonoBehaviour {
    public static SoundManager I;
    [Serializable]
    public struct SSounds {
        public string soundName;
        public AudioClip clip;
    }public List<SSounds> sounds;
    
    
    private Dictionary<Transform, AudioSource> _audioSources = new();
    private Dictionary<string, AudioClip> _audioClips = new();
    private Transform _playingParent;
    private Transform _stoppedParent;
    private int _successfulCount;
    
    
    void Start() {
        I = this;
        _playingParent = transform.GetChild(0);
        _stoppedParent = transform.GetChild(1);
        for (int i = 0; i < _playingParent.childCount; i++) {
            _audioSources.Add(_playingParent.GetChild(i), _playingParent.GetChild(i).GetComponent<AudioSource>());
        }

        for (int i = 0; i < sounds.Count; i++) {
            _audioClips.Add(sounds[i].soundName, sounds[i].clip);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(_playingParent.childCount > 0) {
            for(int i = 0; i < _playingParent.childCount; i++) {
                if(!_audioSources[_playingParent.GetChild(i)].isPlaying) {
                    _playingParent.GetChild(i).SetParent(_stoppedParent);
                }
            }
        }
    }
    
    public void PlaySound(string name, float volume = 0.8f) {
        Transform toPlay = _stoppedParent.GetChild(0);
        toPlay.name = name;
        _audioSources[toPlay].clip = _audioClips[name];
        _audioSources[toPlay].volume = volume;
        _audioSources[toPlay].Play();
        toPlay.SetParent(_playingParent);
    }
    
    public void PlayNoteSound(bool bSuccess, float baseVolume = 0.2f, float basePitch = 0.8f, float increaseCount = 0.1f) {
        string clipName = "Note";
        Transform toPlay = _stoppedParent.GetChild(0);
        toPlay.name = clipName;
        _audioSources[toPlay].clip = _audioClips[clipName];
        
        if (bSuccess) _successfulCount++;
        else _successfulCount = 0;
        
        _audioSources[toPlay].pitch = basePitch + increaseCount * _successfulCount;
        _audioSources[toPlay].volume = Mathf.Clamp(baseVolume + increaseCount * _successfulCount, baseVolume, 1f);
        _audioSources[toPlay].Play();
        toPlay.SetParent(_playingParent);
    }
}
