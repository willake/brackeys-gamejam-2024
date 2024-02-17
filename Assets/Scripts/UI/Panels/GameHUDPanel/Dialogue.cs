using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Audios;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.UI
{
    public class Dialogue : MonoBehaviour
    {
        [Header("Referneces")]
        public TextMeshProUGUI textMesh;
        private Coroutine _coroutine;
        public AudioMixerGroup dialogueMixerGroup;

        [Header("Typing Delay")]
        public float typingDelay = 0.05f;
        public float audioInterval = 0.2f;

        private bool _isTyping = false;
        private float _lastPlayAudioTime = 0;

        private WrappedAudioClip audioClip;
        private AudioSource audioSource;

        private void Start()
        {
            audioClip = ResourceManager.instance.audioResources.uiAudios.dialogue;
            audioSource = AudioManager.instance.BorrowSFXSource();
            audioSource.outputAudioMixerGroup = dialogueMixerGroup;
        }

        public void Present(string text)
        {
            if (_coroutine != null) StopCoroutine(_coroutine);

            StartCoroutine(PresentCoroutine(text));
        }

        private IEnumerator PresentCoroutine(string text)
        {
            _isTyping = true;
            textMesh.text = "";

            foreach (char letter in text)
            {
                textMesh.text += letter;
                yield return new WaitForSeconds(typingDelay);
            }

            _isTyping = false;
        }

        private void Update()
        {
            if (audioSource == null || audioClip == null) return;
            if (_isTyping && Time.time - _lastPlayAudioTime > audioInterval)
            {
                if (audioSource.isPlaying) audioSource.Stop();
                audioSource.volume = audioClip.volume;
                audioSource.pitch = Random.Range(0.8f, 1.2f);
                audioSource.PlayOneShot(audioClip.clip);
                _lastPlayAudioTime = Time.time;
            }

        }
    }
}