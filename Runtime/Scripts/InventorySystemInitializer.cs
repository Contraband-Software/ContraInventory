using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Software.Contraband.Inventory
{
    public class InventorySystemInitializer : MonoBehaviour
    {
        [Serializable]
        public struct SlotItemPair
        {
            public InventorySystemSlot ItemSlot;
            public GameObject Item;
        }

        [SerializeField] List<SlotItemPair> Population = new();

        private void Start()
        {
            foreach (SlotItemPair pair in Population)
            {
#if UNITY_EDITOR
                if (!pair.Item.TryGetComponent<InventorySystemItem>(out _))
                    throw new InvalidOperationException("Tried to initialize slot with a non-item gameObject");
#endif
                if (!pair.ItemSlot._InitSlotItem(pair.Item))
                {
                    Debug.LogException(new Exception("Cannot init item to this slot"));
                }
            }
        }
    }
}