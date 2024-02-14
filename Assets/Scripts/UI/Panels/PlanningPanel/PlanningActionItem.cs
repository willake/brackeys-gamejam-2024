using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class PlanningActionItem : MonoBehaviour
    {
        [Header("References")]
        public Image background;

        [Header("Settings")]
        public Color enabledColor;
        public Color disabledColor;

        public void SetIcon(Sprite sprite)
        {
            background.sprite = sprite;
        }

        public void SetEnabled(bool enabled)
        {
            background.color = enabled ? enabledColor : disabledColor;
        }
    }
}