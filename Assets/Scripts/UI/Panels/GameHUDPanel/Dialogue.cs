using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class Dialogue : MonoBehaviour
    {
        [Header("Referneces")]
        public TextMeshProUGUI textMesh;
        private Coroutine _coroutine;

        [Header("Typing Delay")]
        public float typingDelay = 0.05f;

        public void Present(string text)
        {
            if (_coroutine != null) StopCoroutine(_coroutine);

            StartCoroutine(PresentCoroutine(text));
        }

        private IEnumerator PresentCoroutine(string text)
        {
            textMesh.text = "";

            foreach (char letter in text)
            {
                textMesh.text += letter;
                yield return new WaitForSeconds(typingDelay);
            }
        }
    }
}