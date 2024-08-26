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
    
    public sealed class Container : MonoBehaviour
    {
        //events
        [FormerlySerializedAs("event_Refresh")] [HideInInspector] 
        public UnityEvent<SlotAction, Item> eventRefresh = new();

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

        private List<Item> itemCache = new List<Item>();

        public InventoryContainersManager Manager { get; internal set; } = null;

        public List<Item> GetItemsList()
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
        /// <param name="slotName"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        internal bool _AddItemToSlot(string slotName, Item item)
        {
            Slot IS;
            if (itemSlotIndex.TryGetValue(slotName, out IS))
            {
                //Debug.Log("_AddItemToSlot: SLOT FOUND FOR " + Item.name + "; " + SlotName);
                bool res = IS.SpawnItem(item);

                //Debug.Log("slot.init res: " + res.ToString());

                ContainerRefreshEvent(SlotAction.Added, item);

                return res;
            }

            return false;
        }
        
        private void ContainerRefreshEvent(SlotAction action, Item item)
        {
            // print("Refresh: " + item.name);
            
            itemCache.Clear();

            foreach (KeyValuePair<string, Slot> child in itemSlotIndex)
            {
                Item slotItem = child.Value.SlotItem;
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

                    t.eventSlotted.AddListener(() => ContainerRefreshEvent(SlotAction.Added, t.SlotItem));
                    t.eventUnslotted.AddListener(() => ContainerRefreshEvent(SlotAction.Removed, t.SlotItem));

                    if (t.SlotItem != null)
                    {
                        itemCache.Add(t.SlotItem);
                    }
                }
            }
        }
    }
}