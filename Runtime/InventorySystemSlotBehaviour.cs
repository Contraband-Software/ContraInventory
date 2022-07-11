using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cyanseraph
{
    namespace InventorySystem
    {
        public abstract class InventorySystemSlotBehaviour : MonoBehaviour
        {
            /// <summary>
            /// Custom logic for whether an item can be put in a certain slot, based on both of them having public attributes to compare
            /// </summary>
            /// <param name="item">The item being added to this slot</param>
            /// <returns>Whether the item can be put here or not</returns>
            public abstract bool CanItemSlot(GameObject item);//, GameObject slot
        }
    }
}