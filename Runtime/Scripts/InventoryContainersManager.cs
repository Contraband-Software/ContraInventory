using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Software.Contraband.Inventory
{
    public sealed class InventoryContainersManager : MonoBehaviour
    {
        [Header("Prerequisites")]
        
        [Tooltip("The UI canvas this inventory system operates on, should be a direct parent for a clean project")]
        [SerializeField] private Canvas canvas;

        [Header("Inventory Object Containers")]
        
        [SerializeField] GameObject ContainerContainer;

        [field: SerializeField] public GameObject ItemContainer { get; private set; }

        private Dictionary<string, Container> containerIndex = new ();

        private Action<Item> lostItemHandler = (Item item) =>
        {
            Debug.LogWarning("Lost Item: " + item.name + ", Destroying it...");
            Destroy(item.gameObject);
        };

        internal void RegisterContainer(Container t)
        {
            t.Manager = this;
            containerIndex.Add(t.gameObject.name, t);
        }

        void Awake()
        {
#if UNITY_EDITOR
            if (canvas == null)
            {
                throw new InvalidOperationException("Inventory system canvas not assigned");
            }
#endif

            //get a reference cache to all the containers in the system
            foreach (Transform child in ContainerContainer.transform)
            {
                Container t;
                if (child.gameObject.TryGetComponent<Container>(out t))
                {
                    //containers.Add(t);
                    RegisterContainer(t);
                }
#if UNITY_EDITOR
                else
                {
                    throw new InvalidOperationException(
                        "NON-InventorySystemContainer GameObject within container hierarchy");
                }
#endif
            }
        }

        public void SetLostItemHandler(Action<Item> handler)
        {
            lostItemHandler = handler;
        }
        
        public Action<Item> GetLostItemHandler()
        {
            return lostItemHandler;
        }

        public Container GetContainer(string ContainerName)
        {
            return containerIndex[ContainerName];
        }

        public Dictionary<string, Slot> GetContainerMap(string ContainerName)
        {
            return containerIndex[ContainerName].GetContainerMap();
        }

        public Canvas GetCanvas()
        {
            return canvas;
        }

        /// <summary>
        /// Takes in an item and adds it to the given slot of the given container,
        /// as well as parenting the object to the item container Object.
        /// The item gameObject must already be parented to the same canvas as the target inventory system.
        /// </summary>
        /// <param name="ContainerName"></param>
        /// <param name="SlotName"></param>
        /// <param name="Item"></param>
        /// <returns>Whether the slotting was successful or not</returns>
        public bool AddItem(string ContainerName, string SlotName, Item Item)
        {
            Item.transform.SetParent(ItemContainer.transform);
            Item.GetComponent<Item>().SetCanvas(canvas);

            Container IC;
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
