using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Contra
{
    namespace Inventory
    {
        public class InventorySystemInitializer : MonoBehaviour
        {
            [Serializable]
            public struct SlotItemPair
            {
                public cyanseraph.InventorySystem.InventorySystemSlot ItemSlot;
                public GameObject Item;
            }

            public List<SlotItemPair> Population = new List<SlotItemPair>();

            void Start()
            {
                foreach (SlotItemPair pair in Population)
                {
                    if (!pair.ItemSlot._InitSlotItem(pair.Item))
                    {
                        Debug.LogException(new Exception("Cannot init item to this slot"));
                    }
                }
            }
        }
    }
}
