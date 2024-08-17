using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.Serialization;
using System;

namespace Software.Contraband.Inventory
{
    public enum SlotAction
    {
        Added,
        Removed
    }
    
    public class InventorySystemContainer : MonoBehaviour
    {
        //events
        [FormerlySerializedAs("event_Refresh")] [HideInInspector] 
        public UnityEvent<SlotAction, GameObject> eventRefresh = new();

        //settings
        [Serializable]
        public class OptionalIsolationSettings
        {
            public bool Enabled = false;
            public string Identifier = "Default";
        }

        [Tooltip("Limit item transfer only to containers with this identifier")]
        [field: SerializeField] public OptionalIsolationSettings IsolationSettings { get; private set; }

        //state
        private Dictionary<string, InventorySystemSlot> itemSlotIndex = new Dictionary<string, InventorySystemSlot>();

        private List<GameObject> itemCache = new List<GameObject>();

        internal InventorySystemManager manager = null;

        public List<GameObject> GetItemsList()
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
        internal bool _AddItemToSlot(string SlotName, GameObject Item)
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
        }
        
        private void ContainerRefreshEvent(SlotAction action, GameObject item)
        {
            ContainerRefresh();

            eventRefresh.Invoke(action, item);
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

                    t.eventSlotted.AddListener(() => ContainerRefreshEvent(SlotAction.Added, t.GetSlotItem()));
                    t.eventUnslotted.AddListener(() => ContainerRefreshEvent(SlotAction.Removed, t.GetSlotItem()));

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