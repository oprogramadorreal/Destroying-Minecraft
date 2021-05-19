using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft
{
    public sealed class AudioManager : MonoBehaviour
    {
        [SerializeField]
        private List<Entry> allSoundsList;

        private readonly IDictionary<string, GameObject> allSoundsDictionary = new Dictionary<string, GameObject>();

        private void Awake()
        {
            foreach (var entry in allSoundsList)
            {
                allSoundsDictionary[entry.name] = entry.prefab;
            }
        }

        public AudioSource CreateAudioSource(string soundName)
        {
            return CreateAudioSourceWithin(soundName, transform);
        }

        public AudioSource CreateAudioSourceWithin(string soundName, Transform parent)
        {
            var newSound = Instantiate(allSoundsDictionary[soundName], parent);
            return newSound.GetComponent<AudioSource>();
        }

        public void CreateTemporaryAudioSource(string soundName)
        {
            CreateTemporaryAudioSourceWithin(soundName, transform);
        }

        public void CreateTemporaryAudioSourceWithin(string soundName, Transform parent)
        {
            var newSound = Instantiate(allSoundsDictionary[soundName], parent);
            var clip = newSound.GetComponent<AudioSource>().clip;

            if (clip != null)
            {
                DestroyAfter(newSound, clip.length);
            }
        }

        public void CreateTemporaryAudioSourceAt(string soundName, Vector3 position)
        {
            CreateAudioSourceAt(soundName, position, true);
        }

        public AudioSource CreateAudioSourceAt(string soundName, Vector3 position)
        {
            return CreateAudioSourceAt(soundName, position, false);
        }

        private AudioSource CreateAudioSourceAt(string soundName, Vector3 position, bool destroyWhenFinished)
        {
            var obj = Instantiate(allSoundsDictionary[soundName], position, Quaternion.identity, transform);
            var audioSource = obj.GetComponent<AudioSource>();

            if (destroyWhenFinished)
            {
                if (audioSource.clip != null)
                {
                    DestroyAfter(obj, audioSource.clip.length);
                }
            }

            return audioSource;
        }

        private void DestroyAfter(GameObject obj, float time)
        {
            StartCoroutine(DestroyAfterImpl(obj, time));
        }

        private static IEnumerator DestroyAfterImpl(GameObject obj, float time)
        {
            yield return new WaitForSecondsRealtime(time);
            Destroy(obj);
        }

        public void Fade(AudioSource audioSource, float duration, float targetVolume)
        {
            StartCoroutine(StartFade(audioSource, duration, targetVolume));
        }

        private static IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume)
        {
            var currentTime = 0.0f;
            var start = audioSource.volume;

            while (currentTime < duration)
            {
                currentTime += Time.unscaledDeltaTime;
                audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
                yield return null;
            }

            yield break;
        }

        [System.Serializable]
        private sealed class Entry
        {
            public string name;
            public GameObject prefab;
        }
    }
}
