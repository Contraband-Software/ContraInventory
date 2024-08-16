using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;

namespace Software.Contraband.Inventory
{
    [RequireComponent(typeof(RectTransform))]
    public class InventorySystemSlot : MonoBehaviour, IDropHandler, IDragHandler
    {
        //Events
        [HideInInspector] public UnityEvent event_Slotted = new UnityEvent();
        [HideInInspector] public UnityEvent event_Unslotted = new UnityEvent();

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
            public InventorySystemSlotBehaviour script = null;
        }

        [Header("Settings")]
        [Tooltip("Custom script object that inherits from InventoryManager.SlotBehaviour to define checks on how this slot should accept items.")]
        [SerializeField] private OptionalScript CustomSlotBehaviour;
        //public StackSettings stackSettings;

        //State
        [HideInInspector] public InventorySystemContainer container = null;
        private GameObject slotItem = null;
        private RectTransform rectTransformComponent;

        private Action<InventorySystemItem> LostItemHandler;

        private void Awake()
        {
            gameObject.tag = "InventorySystemSlot";

            rectTransformComponent = GetComponent<RectTransform>();
        }
        private void Start()
        {
            LostItemHandler = container.manager.GetLostItemHandler();
        }

        public RectTransform GetRectTransform()
        {
            return rectTransformComponent;
        }

        //for spawning an item to a slot
        public bool _InitSlotItem(GameObject item)
        {
            if (slotItem == null && AddItemToSlot(item))
            {
                //Debug.Log("Slot allowed item: " + item.name);
                item.GetComponent<InventorySystemItem>()._InitSlot(this);
                return true;
            }
            else
            {
                return false;
            }
            //Debug.Log("init: " + item.name + " " + gameObject.name);
        }

        //for moving an item from another slot
        public bool SetSlotItem(GameObject item)
        {
            InventorySystemItem itemScript = item.GetComponent<InventorySystemItem>();

            //isolation
            if (itemScript.GetPreviousSlot() == null)
            {
                throw new System.Exception("itemScript.GetPreviousSlot() == null");
            }
            InventorySystemContainer previousSlotContainer = itemScript.GetPreviousSlot().container;
            if (container.IsolationSettings.Enabled || previousSlotContainer.IsolationSettings.Enabled)
            {//if either are isolated
                //compare identifiers
                if (previousSlotContainer.IsolationSettings.Identifier != container.IsolationSettings.Identifier)
                {
                    return false;
                }
            }

            if (slotItem == null)
            {
                if (AddItemToSlot(item))
                {
                    itemScript._AddToSlot(this);
                    return true;
                }
            } else {
                InventorySystemSlot itemsPreviousSlot = itemScript.GetPreviousSlot();
                if (itemsPreviousSlot.GetSlotItem() == null)
                {
                    if (itemsPreviousSlot.SetSlotItem(slotItem) && AddItemToSlot(item))
                    {
                        itemScript._AddToSlot(this);
                        return true;
                    }
                }
                else
                {
                    //Deal with item
                    if (LostItemHandler != null) { LostItemHandler(itemScript); };
                }
            }

            return false;
        }

        //core logic that applies to both initialising and moving
        private bool AddItemToSlot(GameObject item)
        {
            //custom logic
            if (CustomSlotBehaviour.Enabled)
            {
                if (!CustomSlotBehaviour.script.CanItemSlot(item))
                {
#if UNITY_EDITOR
                    Debug.Log(gameObject.name + ": SLOT BEHAVIOUR HAS BLOCKED THIS ATTEMPTED INSERT: " + item.name);
#endif
                    return false;
                }
            }
            
            slotItem = item;
            event_Slotted.Invoke();
            return true;
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

            event_Unslotted.Invoke();
        }

        public void OnDrop(PointerEventData eventData)
        {
            //fire a custom event saying that a slot has been updated within the inventory system
            //this needs to check if the pointer drag item is a compatible inventory system item, use tags instead of trygetcomponent
            if (eventData.pointerDrag != null && eventData.pointerDrag.tag == "InventorySystemItem")
            {
                //Debug.Log("Tried to drop in slot");
                SetSlotItem(eventData.pointerDrag);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            //Debug.Log("dungus");
        }
    }
}

