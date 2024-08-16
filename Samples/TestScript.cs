using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public Software.Contraband.Inventory.InventorySystemManager IM;

    public GameObject item;
    //public Canvas c;

    private Software.Contraband.Inventory.InventorySystemContainer IC;

    private GameObject newItem;

    // Start is called before the first frame update
    void Start()
    {
        IC = IM.GetContainer("Container1");
        IC.event_Refresh.AddListener(onRefresh);

        newItem = Instantiate(item);//, IM.GetCanvas().transform);
        newItem.name = "newItem3";
        newItem.GetComponent<MyItemBehaviour>().slotType = Enums.SlotType.C;

        Debug.Log("ADD ITEM (EXTERNAL): " + IM.AddItem("ContainerTest", "Slot_C_1", newItem));

        //works: Debug.Log(IM.GetContainer("ContainerTest").GetItems().Count);
    }

    void onRefresh()
    {
        string listofitems = "";
        List<GameObject> items = IC.GetRawItemsList();
        foreach (GameObject item in items)
        {
            listofitems += item.name + ", ";
        }
        //Debug.Log(listofitems);
    }

    // Update is called once per frame
    void Update()
    {

    }
}


//fix issue with spawned items not going to there position DONE
//add item swapping functionality
//add stacking
// need a way to select a portion of the stacked items (click and hold)
// an interface to provide the stack amount to secondary logic
// a way to move the entire stack (right click)
// slots must also be allowed to decide whether they accept - secondary logic
// must not bug out with the swapping functionality