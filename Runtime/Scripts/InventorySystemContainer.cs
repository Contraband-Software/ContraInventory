using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor;

namespace Software.Contraband.Inventory
{
    public class InventorySystemContainer : MonoBehaviour
    {
        //events
        [HideInInspector] public UnityEvent event_Refresh = new UnityEvent();

        //settings
        [Serializable]
        public class OptionalIsolationSettings
        {
            public bool Enabled = false;
            public string Identifier = "Default";
        }

        [Tooltip("Disallow item transfer from this container to other containers and vice versa.")]
        public OptionalIsolationSettings IsolationSettings;

        //state
        private Dictionary<string, InventorySystemSlot> itemSlotIndex = new Dictionary<string, InventorySystemSlot>();

        private List<GameObject> itemCache = new List<GameObject>();

        [HideInInspector] public InventorySystemManager manager = null;

        public List<GameObject> GetRawItemsList()
        {
            return itemCache;
        }

        public Dictionary<string, InventorySystemSlot> GetContainerMap()
        {
            return itemSlotIndex;
        }

        /// <summary>
        /// Programatically adding item to slot
        /// </summary>
        /// <param name="SlotName"></param>
        /// <param name="Item"></param>
        /// <returns></returns>
        public bool _AddItemToSlot(string SlotName, GameObject Item)
        {
            InventorySystemSlot IS;
            if (itemSlotIndex.TryGetValue(SlotName, out IS))
            {
                //Debug.Log("_AddItemToSlot: SLOT FOUND FOR " + Item.name + "; " + SlotName);
                bool res = IS._InitSlotItem(Item);

                //Debug.Log("slot.init res: " + res.ToString());

                ContainerRefresh();

                return res;
            }

            return false;
        }

        private void ContainerRefresh()
        {
            itemCache.Clear();

            foreach (KeyValuePair<string, InventorySystemSlot> child in itemSlotIndex)
            {
                GameObject slotItem = child.Value.GetSlotItem();
                if (slotItem != null)
                {
                    itemCache.Add(slotItem);
                }
            }

            event_Refresh.Invoke();
        }

        void Awake()
        {
            //grab a reference to the item slot script for all the item slots in this container,
            //as well as initialising the item cache, and adding its own reference and event handlers
            foreach (Transform child in transform)
            {
                InventorySystemSlot t;
                if (child.gameObject.TryGetComponent<InventorySystemSlot>(out t))
                {
                    itemSlotIndex.Add(child.gameObject.name, t);

                    t.container = this;

                    t.event_Slotted.AddListener(ContainerRefresh);
                    t.event_Unslotted.AddListener(ContainerRefresh);

                    GameObject slotItem = t.GetSlotItem();
                    if (slotItem != null)
                    {
                        itemCache.Add(slotItem);
                    }
                }
            }
        }
    }
}