using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cyanseraph
{
    namespace InventorySystem
    {
        public class InventorySystemManager : MonoBehaviour
        {
            private Dictionary<string, InventorySystemContainer> containerIndex = new Dictionary<string, InventorySystemContainer>();

            [Header("Prerequisites")]
            [Tooltip("The UI canvas this inventory system operates on")]
            [SerializeField] private Canvas canvas;

            [Header("Inventory Object Containers")]
            public GameObject ContainerContainer;
            public GameObject ItemContainer;

            void Awake()
            {
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
                }

                //Debug.Log("Containers: " + containerIndex.Count);
            }

            public InventorySystemContainer GetContainer(string ContainerName)
            {
                return containerIndex[ContainerName];
            }

            public Dictionary<string, InventorySystemSlot> GetContainerMap(string ContainerName)
            {
                return containerIndex[ContainerName].GetContainerMap();
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
                Item.transform.parent = canvas.transform;
                Item.GetComponent<cyanseraph.InventorySystem.InventorySystemItem>().SetCanvas(canvas);

                InventorySystemContainer IC;
                if (containerIndex.TryGetValue(ContainerName, out IC))
                {
                    if (IC._AddItemToSlot(SlotName, Item))
                    {
                        //Debug.Log("S: IM ICAI");
                        Item.transform.SetParent(ItemContainer.transform);
                        return true;
                    }

                    return false;
                }
                //Debug.Log("F: IM TG");

                return false;
            }
        }
    }
}