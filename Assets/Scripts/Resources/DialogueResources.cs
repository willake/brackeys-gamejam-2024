using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    [CreateAssetMenu(menuName = "MyGame/Resources/DialogueResources")]
    public class DialogueResources : ScriptableObject
    {
        [Header("Dialogues")]
        public string enterEchoLocator;
        public string enterPlan;
        public string enterPerform;
        public string onKillEnemy;
    }
}