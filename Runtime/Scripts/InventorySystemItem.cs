using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

namespace Contra
{
    namespace Inventory
    {
        [
            RequireComponent(typeof(RectTransform)), 
            RequireComponent(typeof(CanvasGroup))
        ]
        public class InventorySystemItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
        {
            //Events
            //the object has been removed from its slot
            [HideInInspector] public UnityEvent event_Unslotted = new UnityEvent();
            //The object was sent back to its original slot
            [HideInInspector] public UnityEvent event_Reslotted = new UnityEvent();
            //the object is in a new slot
            [HideInInspector] public UnityEvent event_Slotted = new UnityEvent();
            //the object is just clicked, not dragged
            //[HideInInspector] public UnityEvent event_mouseDown = new UnityEvent();

            //Prerequisites
            //[Header("Prerequisites")]
            //[Tooltip("The UI canvas this inventory system operates on")]
            private Canvas canvas;

            //Settings
            [Header("Settings")]
            [Min(0), Tooltip("The time in milliseconds to wait inbetween handling click events, should be at least 1000")]
            [SerializeField] private int ClickLockingTime = 1000;
            [Range(0, 1), Tooltip("The speed at which a 'floating/flying' item travels to its target slot. 1 is instant.")]
            [SerializeField] private float PingTravelSpeed = 0.1f;
            [Min(0), Tooltip("The distance between an item and its target slot at which it just snaps into the target slot")]
            [SerializeField] private float FlyingItemSnapThreshold = 6.0f;

            [Tooltip("Only allow the left click for dragging items")]
            [SerializeField] private bool LimitDragClick = true;

            //Options
            [Serializable]
            public class OptionalAttr
            {
                public bool Stackable = false;
                public float MaximumAmount = 1;
            }
            [Header("Game Options")]
            [Tooltip("The type of the item, such as 'wooden-block'")]
            //public string ItemTypeIdentifier = "Default";
            //[Tooltip("If the item is to be stackable, it must have a defined unique identifier.")]
            //public OptionalAttr StackOptions;

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
            private void resetFlag(object source, System.Timers.ElapsedEventArgs e)
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
                timer.Elapsed += resetFlag;
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
            public void _InitSlot(InventorySystemSlot newSlot)
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
            public void _AddToSlot(InventorySystemSlot newSlot)
            {
                _SetCurrentSlot(newSlot);
                event_Slotted.Invoke();
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
                        slot._UnsetSlotItem();
                    }

                    //remember the slot we are departing from
                    SetPreviousSlot(slot);

                    //we are currently not in a slot
                    _SetCurrentSlot(null);

                    //fire events
                    event_Unslotted.Invoke();
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
                    event_Reslotted.Invoke();
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

            public void MoveToPosition(Vector2 pos)
            {
                isFlying = true;
                desiredPosition = pos;
            }

            private void FixedUpdate()
            {
                if (!isBeingDragged)// && isFlying == false)
                {
                    Vector2 diff = desiredPosition - rectTransform.anchoredPosition;
                    if (diff.magnitude > FlyingItemSnapThreshold) {
                        rectTransform.anchoredPosition += (diff) * PingTravelSpeed;
                    } else
                    {
                        rectTransform.anchoredPosition = desiredPosition;
                        isFlying = false;
                    }
                }
            }

            private void Update()
            {
                //so the slot drop event can be fired even if it is occupied for swapping
                if (!isBeingDragged)
                {
                    if (Input.GetMouseButton(0))
                    {
                        cg.blocksRaycasts = false;
                    }
                    else
                    {
                        cg.blocksRaycasts = true;
                    }
                }
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
}
