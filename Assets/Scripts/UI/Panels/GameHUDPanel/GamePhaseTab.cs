using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using Game.Gameplay;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.UI
{
    public class GamePhaseTab : MonoBehaviour
    {
        [Header("References")]
        public WDTextButton btnEcho;
        public WDTextButton btnPlan;
        public Image backgroundBtnEcho;
        public Image backgroundBtnPlan;

        [Header("Settings")]
        public Color backgroundEnabled = Color.white;
        public Color textEnabled = Color.black;
        public Color backgroundDisabled = Color.black;
        public Color textDisabled = Color.white;

        private OnPhaseSelectEvent _onPhaseSelect = new OnPhaseSelectEvent();
        public IObservable<GamePhase> OnPhaseSelectObservable
        {
            get => _onPhaseSelect.AsObservable();
        }

        private GamePhase _phase;

        private void Start()
        {
            btnEcho
                .OnClickObservable
                .ObserveOnMainThread()
                .Subscribe(_ => SelectEchoLocationPhase())
                .AddTo(this);

            btnPlan
                .OnClickObservable
                .ObserveOnMainThread()
                .Subscribe(_ => SelectPlanningPhase())
                .AddTo(this);

            SelectEchoLocationPhase();
        }

        private void SelectEchoLocationPhase()
        {
            _onPhaseSelect.Invoke(GamePhase.EchoLocation);
            backgroundBtnEcho.color = backgroundEnabled;
            backgroundBtnPlan.color = backgroundDisabled;
            btnEcho.SetTextColor(textEnabled);
            btnPlan.SetTextColor(textDisabled);
            _phase = GamePhase.EchoLocation;
        }

        private void SelectPlanningPhase()
        {
            _onPhaseSelect.Invoke(GamePhase.Planning);
            backgroundBtnEcho.color = backgroundDisabled;
            backgroundBtnPlan.color = backgroundEnabled;
            btnEcho.SetTextColor(textDisabled);
            btnPlan.SetTextColor(textEnabled);
            _phase = GamePhase.Planning;
        }

        public class OnPhaseSelectEvent : UnityEvent<GamePhase>
        {

        }
    }
}