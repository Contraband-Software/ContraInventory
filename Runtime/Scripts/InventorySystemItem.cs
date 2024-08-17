using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Serialization;

namespace Software.Contraband.Inventory
{
    [
        RequireComponent(typeof(RectTransform)), 
        RequireComponent(typeof(CanvasGroup)),
        SelectionBase
    ]
    public class InventorySystemItem : 
        MonoBehaviour, 
        IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
    {
        //the object is just clicked, not dragged
        //[HideInInspector] public UnityEvent event_mouseDown = new UnityEvent();

        //Prerequisites
        //[Header("Prerequisites")]
        //[Tooltip("The UI canvas this inventory system operates on")]
        private Canvas canvas;

        //Settings
        [Header("Settings"), Space(10)]
        
        [Min(50), Tooltip(
             "(Immutable at runtime) The time in milliseconds to wait " +
             "between handling click events, lower means more instability")]
        [SerializeField] private int ClickLockingTime = 1000;
        
        [Range(0, 1), Tooltip(
             "The speed at which a floating (user has stopped dragging it) " +
             "item travels to its target slot. 1 is instant")]
        [SerializeField] private float PingTravelSpeed = 0.1f;
        
        [Min(1), Tooltip(
             "The distance between an item and its target slot at which it snaps " +
             "into the target slot and ceases floating")]
        [SerializeField] private float FlyingItemSnapThreshold = 6.0f;

        [Tooltip("Only allow the left click for dragging items, can sometimes get weird if disabled")]
        [SerializeField] private bool LimitDragClick = true;

        //Options
        [Serializable]
        public class OptionalAttr
        {
            public bool Stackable = false;
            public float MaximumAmount = 1;
        }
        // [Header("Game Options")]
        // [Tooltip("The type of the item, such as 'wooden-block'")]
        //public string ItemTypeIdentifier = "Default";
        //[Tooltip("If the item is to be stackable, it must have a defined unique identifier.")]
        //public OptionalAttr StackOptions;
        
        //Events
        [Header("Events"), Space(10)]
        
        [FormerlySerializedAs("event_Unslotted")]
        //the object has been removed from its slot
        public UnityEvent eventUnslotted = new UnityEvent();
        //The object was sent back to its original slot
        [FormerlySerializedAs("event_Reslotted")] public UnityEvent eventReslotted = new UnityEvent();
        //the object is in a new slot
        [FormerlySerializedAs("event_Slotted")] public UnityEvent eventSlotted = new UnityEvent();

        //State
        private RectTransform rectTransform;
        private CanvasGroup cg;

        private Vector2 desiredPosition;

        private InventorySystemSlot previousSlot = null;
        private InventorySystemSlot slot = null;

        private bool isBeingDragged = false;
        private bool isFlying = false;

        //Event locking
        private bool buttonLocked;
        private System.Timers.Timer timer;
        private void ResetFlag(object source, System.Timers.ElapsedEventArgs e)
        {
            buttonLocked = false;
            timer.Enabled = false;
        }

        //unity
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            cg = GetComponent<CanvasGroup>();

            timer = new System.Timers.Timer(ClickLockingTime);
            timer.Elapsed += ResetFlag;
            gameObject.tag = "InventorySystemItem";

            desiredPosition = rectTransform.anchoredPosition;
        }
        
        private void Start()
        {
            canvas = GetComponentInParent<InventorySystemManager>().GetCanvas();
        }

        /// <summary>
        /// Just spawns an item in a slot
        /// </summary>
        /// <param name="newSlot"></param>
        internal void _InitSlot(InventorySystemSlot newSlot)
        {
            previousSlot = newSlot;
            slot = newSlot;

            //MoveToPosition(newSlot.GetRectTransform().anchoredPosition);

            rectTransform.anchoredPosition = newSlot.GetRectTransform().anchoredPosition;
            desiredPosition = newSlot.GetRectTransform().anchoredPosition;
        }
        
        private void SetPreviousSlot(InventorySystemSlot newSlot)
        {
            previousSlot = newSlot;
        }
        
        private void _SetCurrentSlot(InventorySystemSlot newSlot)
        {
            slot = newSlot;
        }

        /// <summary>
        /// For the slot script to bind the item
        /// </summary>
        /// <param name="newSlot"></param>
        internal void _AddToSlot(InventorySystemSlot newSlot)
        {
            _SetCurrentSlot(newSlot);
            eventSlotted.Invoke();
            MoveToPosition(newSlot.GetRectTransform().anchoredPosition);

            //just in case, could be removed
            ToggleDrag(false);
        }

        public void SetCanvas(Canvas newcanvas)
        {
            canvas = newcanvas;
        }

        public InventorySystemSlot GetCurrentSlot()
        {
            return slot;
        }
        public InventorySystemSlot GetPreviousSlot()
        {
            return previousSlot;
        }

        //event handlers
        public void OnBeginDrag(PointerEventData eventData)
        {
            //blocked due to click limiting
            if (buttonLocked)
            {
                eventData.pointerDrag = null;
                return;
            }
            
            //blocked due to click limiting
            if (isFlying)
            {
                eventData.pointerDrag = null;
                return;
            }

            isFlying = true;

            if (!isBeingDragged && (LimitDragClick) ? (eventData.button == PointerEventData.InputButton.Left) : true)
            {
                //locking to prevent bugs
                buttonLocked = true;
                timer.Enabled = true;

                ToggleDrag(true);

                if (slot)
                {
                    slot.UnsetSlotItem();
                }

                //remember the slot we are departing from
                SetPreviousSlot(slot);

                //we are currently not in a slot
                _SetCurrentSlot(null);

                //fire events
                eventUnslotted.Invoke();
            }
            else
            {
                //Debug.Log("Drag Failed");
                //cancel the drag event
                eventData.pointerDrag = null;
            }
        }
        public void OnDrag(PointerEventData eventData)
        {
            //Move the item with respect to the canvas scaling (affects positioning)
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (slot == null)
            {
                //return the item back to its original slot, as it has not been put into a valid new one
                previousSlot.SetSlotItem(this.gameObject);

                _SetCurrentSlot(previousSlot);

                //fire events
                eventReslotted.Invoke();
            }

            ToggleDrag(false);
        }
        public void OnDrop(PointerEventData eventData)
        {
            //not captured by this script, captured by the ItemSlot script on the recieving end
            //the method still must exist even if it is empty as unity forces you to define it
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            // Debug.Log("Pointer Down: " + mouseDown);
            //event_mouseDown.Invoke();
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            //Debug.Log("Pointer Up");
        }

        private void MoveToPosition(Vector2 pos)
        {
            isFlying = true;
            desiredPosition = pos;
        }

        // private void FixedUpdate()
        // {
        //     if (!isBeingDragged)// && isFlying == false)
        //     {
        //
        //     }
        // }

        private void Update()
        {
            //so the slot drop event can be fired even if it is occupied for swapping
            if (isBeingDragged) return;
            
            cg.blocksRaycasts = !Input.GetMouseButton(0);
                
            #region FLYING
            Vector2 diff = desiredPosition - rectTransform.anchoredPosition;
            if (diff.magnitude > FlyingItemSnapThreshold) {
                rectTransform.anchoredPosition += (diff) * PingTravelSpeed;
            } else
            {
                rectTransform.anchoredPosition = desiredPosition;
                isFlying = false;
            }
            #endregion
        }

        private void ToggleDrag(bool status)
        {
            if (status)
            {
                //allow elements behind it to detect events, such as the item slot detecting a drop
                cg.blocksRaycasts = false;
                isBeingDragged = true;

                //Debug.Log("Drag Started");
            }
            else
            {
                cg.blocksRaycasts = true;
                isBeingDragged = false;

                //Debug.Log("Drag Ended");
            }
        }
    }
}