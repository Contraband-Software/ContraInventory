using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MySlotBehaviour : cyanseraph.InventorySystem.InventorySystemSlotBehaviour
{
    public Enums.SlotType slotType = Enums.SlotType.A;

    public GameObject te;
    private void Start()
    {
        te.GetComponent<TextMeshProUGUI>().text = slotType.ToString();
    }
    public override bool CanItemSlot(GameObject item)
    {
        //bool ans = false;
        //try {
        //    ans = slotType == item.GetComponent<MyItemBehaviour>().slotType;
        //} catch {
        //    Debug.Log("Err: " + item.name);
        //}

        return slotType == item.GetComponent<MyItemBehaviour>().slotType;//ans;
    }
}
