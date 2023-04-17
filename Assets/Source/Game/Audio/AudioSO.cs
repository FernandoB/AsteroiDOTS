using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AudioDataElement
{
    public FXEnum id;
    public AudioClip clip;
    public bool loop = false;
}

[CreateAssetMenu(fileName = "AudioData", menuName = "ScriptableObjects/AudioData", order = 1)]
public class AudioSO : ScriptableObject
{
    public List<AudioDataElement> elements;
}