using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MyItemBehaviour : MonoBehaviour
{
    public Enums.SlotType slotType = Enums.SlotType.A;

    public GameObject te;

    private CanvasGroup cg;

    private cyanseraph.InventorySystem.InventorySystemItem II;

    private void startDrag()
    {
        cg.alpha = 0.5f;
    }
    private void endDrag()
    {
        cg.alpha = 1.0f;
    }

    private void Start()
    {
        cg = GetComponent<CanvasGroup>();
        te.GetComponent<TextMeshProUGUI>().text = slotType.ToString();

        II = GetComponent<cyanseraph.InventorySystem.InventorySystemItem>();

        II.event_Unslotted.AddListener(startDrag);
        II.event_Slotted.AddListener(endDrag);

        II.ItemTypeIdentifier = slotType.ToString();
    }
}
