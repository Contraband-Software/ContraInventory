using System.Collections;
using System.Collections.Generic;
using Software.Contraband.Inventory;
using UnityEngine;
using TMPro;

public class MySlotBehaviour : Software.Contraband.Inventory.IInventorySystemSlotBehaviour
{
    public Enums.SlotType slotType = Enums.SlotType.A;

    public GameObject te;
    private void Start()
    {
        te.GetComponent<TextMeshProUGUI>().text = slotType.ToString();
    }
    public override bool CanItemSlot(InventorySystemSlot _, GameObject item)
    {
        return slotType == item.GetComponent<MyItemBehaviour>().slotType;
    }
}
