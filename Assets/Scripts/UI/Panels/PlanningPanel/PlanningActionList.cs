using System;
using System.Collections;
using System.Collections.Generic;
using Game.Gameplay;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Game.UI
{
    public class PlanningActionList : MonoBehaviour
    {
        [Header("References")]
        public GameObject prefabItem;

        private PlanningActionItem[] items;
        public void Setup(int count)
        {
            if (items != null && items.Length > 0)
            {
                foreach (var item in items)
                {
                    // bad for performance, will fix it if this project goes further
                    Destroy(item.gameObject);
                }
            }
            items = new PlanningActionItem[count];
            for (int i = 0; i < count; i++)
            {
                GameObject itemObj = Instantiate(prefabItem, transform);
                itemObj.name = $"Action {i}";
                items[i] = itemObj.GetComponent<PlanningActionItem>();
            }
        }

        public void Highlight(int idx)
        {
            if (idx >= items.Length) return;

            for (int i = 0; i < items.Length; i++)
            {
                items[i].SetEnabled(i == idx);
            }
        }
    }
}