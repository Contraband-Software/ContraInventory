using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Contra
{
    namespace Inventory
    {
        public class InventorySystemManager : MonoBehaviour
        {
            [Header("Prerequisites")]
            [Tooltip("The UI canvas this inventory system operates on")]
            [SerializeField] private Canvas canvas;

            [Header("Inventory Object Containers")]
            [SerializeField] GameObject ContainerContainer;
            [SerializeField] GameObject ItemContainer;

            private Dictionary<string, InventorySystemContainer> containerIndex = new Dictionary<string, InventorySystemContainer>();

            private Action<InventorySystemItem> lostItemHandler = (InventorySystemItem item) =>
            {
                Debug.Log("LOST ITEM: " + item.name + ", Destroying it...");
                Destroy(item.gameObject);
            };

            void Awake()
            {
#if UNITY_EDITOR
                if (canvas == null)
                {
                    throw new Exception("INVENTORY SYSTEM CANVAS NOT ASSIGNED");
                }
#endif

                //get a reference cache to all the containers in the system
                foreach (Transform child in ContainerContainer.transform)
                {
                    InventorySystemContainer t;
                    if (child.gameObject.TryGetComponent<InventorySystemContainer>(out t))
                    {
                        //containers.Add(t);
                        t.manager = this;

                        containerIndex.Add(child.gameObject.name, t);
                    }
#if UNITY_EDITOR
                    else
                    {
                        throw new System.Exception("NON-InventorySystemContainer GameObject within container heirarchy");
                    }
#endif
                }
            }

            public void SetLostItemHandler(Action<InventorySystemItem> handler)
            {
                lostItemHandler = handler;
            }
            public Action<InventorySystemItem> GetLostItemHandler()
            {
                return lostItemHandler;
            }

            public InventorySystemContainer GetContainer(string ContainerName)
            {
                return containerIndex[ContainerName];
            }

            public Dictionary<string, InventorySystemSlot> GetContainerMap(string ContainerName)
            {
                return containerIndex[ContainerName].GetContainerMap();
            }

            public Canvas GetCanvas()
            {
                return canvas;
            }

            //DOES NOT WORK IF THE ITEM IS NOT ALREADY PARENTED TO THE CANVAS, AS THE RECT TRANSFORM STARTS BEHAVING WEIRDLY
            // Be warned that the item MUST ALREADY BE PARENTED TO THE CANVAS OF THE INVENTORY SYSTEM

            /// <summary>
            /// Takes in an item and adds it to the given slot of the given container, as well as parenting the object to the item container Object.
            /// </summary>
            /// <param name="ContainerName"></param>
            /// <param name="SlotName"></param>
            /// <param name="Item"></param>
            /// <returns>Whether the slotting was successful or not</returns>
            public bool AddItem(string ContainerName, string SlotName, GameObject Item)
            {
                Item.transform.SetParent(ItemContainer.transform);
                Item.GetComponent<cyanseraph.InventorySystem.InventorySystemItem>().SetCanvas(canvas);

                InventorySystemContainer IC;
                if (containerIndex.TryGetValue(ContainerName, out IC))
                {
                    //Debug.Log("IM: TryGetValue - CONTAINER FOUND; " + ContainerName);
                    if (IC._AddItemToSlot(SlotName, Item))
                    {
                        return true;
                    }

                    return false;
                }

                return false;
            }
        }
    }
}
