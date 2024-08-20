using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Software.Contraband.Inventory
{
    [
        RequireComponent(typeof(RectTransform)),
        SelectionBase
    ]
    public class Slot : MonoBehaviour, IDropHandler, IDragHandler
    {

        //Settings
        //[Serializable]
        //public class StackSettings
        //{
        //    public bool AllowStacking = false;
        //    public int MaximumItemAmount = 1;
        //}

        //custom behaviour hooking
        [Serializable]
        public class OptionalScript
        {
            public bool Enabled = false;
            public AbstractSlotBehaviour script = null;
        }

        [Header("Settings"), Space(10)]
        [Tooltip(
            "Custom script object that inherits from InventoryManager.SlotBehaviour " +
            "to define checks on how this slot should accept items.")]
        [field: SerializeField] private OptionalScript CustomSlotBehaviour;
        //public StackSettings stackSettings;
        
        //Events
        [Header("Events"), Space(10)]
        public UnityEvent eventSlotted = new();
        public UnityEvent eventUnslotted = new();

        //State
        public Container Container { get; internal set; } = null;
        private GameObject slotItem = null;
        private RectTransform rectTransformComponent;

        private Action<Item> LostItemHandler;

        private void Awake()
        {
            gameObject.tag = "InventorySystemSlot";

            rectTransformComponent = GetComponent<RectTransform>();
        }
        private void Start()
        {
#if UNITY_EDITOR
            // ReSharper disable once Unity.NoNullPropagation
            if (Container?.Manager is null)
            {
                throw new InvalidOperationException(
                    "A container hierarchy may only be one level deep, and only composed of slots.");
            }
#endif
            LostItemHandler = Container.Manager.GetLostItemHandler();
        }

        public RectTransform GetRectTransform()
        {
            return rectTransformComponent;
        }

        //for spawning an item to a slot
        public bool InitSlotItem(GameObject item)
        {
            if (slotItem != null || !CheckCustomBehaviour(item)) return false;
            AddItemToSlot(item);
            //Debug.Log("Slot allowed item: " + item.name);
            item.GetComponent<Item>()._InitSlot(this);
            return true;

            //Debug.Log("init: " + item.name + " " + gameObject.name);
        }

        //for moving an item from another slot
        internal bool SetSlotItem(GameObject item)
        {
            Item itemScript = item.GetComponent<Item>();
            Slot itemsPreviousSlot = itemScript.GetPreviousSlot();

            //isolation
            if (itemsPreviousSlot == null)
            {
                throw new InvalidOperationException("itemScript.GetPreviousSlot() == null");
            }
            Container previousSlotContainer = itemsPreviousSlot.Container;
            if (Container.IsolationSettings.Enabled || previousSlotContainer.IsolationSettings.Enabled)
            {//if either are isolated
                //compare identifiers
                if (previousSlotContainer.IsolationSettings.Identifier != Container.IsolationSettings.Identifier)
                {
                    return false;
                }
            }
            
            // print("isolation passed");
            
            if (!CheckCustomBehaviour(item)) return false;

            // empty slot
            if (slotItem == null)
            {
                // print("slot empty");
                itemScript._AddToSlot(this);
                AddItemToSlot(item);
                return true;
            }
            
            // print("slot blocked");

            // empty source slot means we can swap the items
            if (itemsPreviousSlot.GetSlotItem() == null)
            {
                // print("prev slot available");
                
                // try to swap the items
                if (!itemsPreviousSlot.SetSlotItem(slotItem)) return false;
                
                itemScript._AddToSlot(this);
                AddItemToSlot(item);
                return true;
            }

            //Deal with item
            LostItemHandler?.Invoke(itemScript);

            return false;
        }

        private bool CheckCustomBehaviour(GameObject item)
        {
            //custom logic
            if (!CustomSlotBehaviour.Enabled)
                return true;

            return CustomSlotBehaviour.script.CanItemSlot(this, item);
        }

        //core logic that applies to both initialising and moving
        private void AddItemToSlot(GameObject item)
        {
            slotItem = item;
            eventSlotted.Invoke();
        }

        public GameObject GetSlotItem()
        {
            return slotItem;
        }

        //for when an item is removed from a slot
        internal void UnsetSlotItem()
        {
            //slotItem.GetComponent<InventorySystemItem>().SetSlot(null);
            slotItem = null;
            
            eventUnslotted.Invoke();
        }

        public void DestroyItem()
        {
            if (slotItem != null)
            {
                Destroy(slotItem);
                slotItem = null;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            //fire a custom event saying that a slot has been updated within the inventory system
            //this needs to check if the pointer drag item is a compatible inventory system item,
            //use tags instead of trygetcomponent
            if (eventData.pointerDrag != null && eventData.pointerDrag.CompareTag("InventorySystemItem"))
            {
                //Debug.Log("Tried to drop in slot");
                SetSlotItem(eventData.pointerDrag);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            
        }
    }
}

