using System.Collections;
using System.Collections.Generic;
using Game.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class PhaseIndicator : MonoBehaviour
    {
        [Header("Reference")]
        public Image[] indicators;
        public WDText textTitle;

        [Header("Settings")]
        public Color enabledColor;
        public Color disabledColor;

        public void SetState(GameState state)
        {
            indicators[0].color = state == GameState.EchoLocation ? enabledColor : disabledColor;
            indicators[1].color = state == GameState.Plan ? enabledColor : disabledColor;
            indicators[2].color = state == GameState.Perform ? enabledColor : disabledColor;

            if (state == GameState.EchoLocation) textTitle.text = ResourceManager.instance.dialogueResources.titleEcholocationPhase;
            else if (state == GameState.Plan) textTitle.text = ResourceManager.instance.dialogueResources.titlePlanPhase;
            else if (state == GameState.Perform) textTitle.text = ResourceManager.instance.dialogueResources.titlePerformPhase;
        }
    }
}