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
    
    public class Container : MonoBehaviour
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
        private Dictionary<string, Slot> itemSlotIndex = new Dictionary<string, Slot>();

        private List<GameObject> itemCache = new List<GameObject>();

        public InventoryContainersManager Manager { get; internal set; } = null;

        public List<GameObject> GetItemsList()
        {
            return itemCache;
        }

        public Dictionary<string, Slot> GetContainerMap()
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
            Slot IS;
            if (itemSlotIndex.TryGetValue(SlotName, out IS))
            {
                //Debug.Log("_AddItemToSlot: SLOT FOUND FOR " + Item.name + "; " + SlotName);
                bool res = IS.InitSlotItem(Item);

                //Debug.Log("slot.init res: " + res.ToString());

                ContainerRefreshEvent(SlotAction.Added, Item);

                return res;
            }

            return false;
        }
        
        private void ContainerRefreshEvent(SlotAction action, GameObject item)
        {
            // print("Refresh: " + item.name);
            
            itemCache.Clear();

            foreach (KeyValuePair<string, Slot> child in itemSlotIndex)
            {
                GameObject slotItem = child.Value.GetSlotItem();
                if (slotItem != null)
                {
                    itemCache.Add(slotItem);
                }
            }
            eventRefresh.Invoke(action, item);
        }

        void Awake()
        {
            //grab a reference to the item slot script for all the item slots in this container,
            //as well as initialising the item cache, and adding its own reference and event handlers
            foreach (Transform child in transform)
            {
                Slot t;
                if (child.gameObject.TryGetComponent<Slot>(out t))
                {
                    itemSlotIndex.Add(child.gameObject.name, t);

                    t.Container = this;

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