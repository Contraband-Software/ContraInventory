using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using System.IO;

public class GameObjectMenu : MonoBehaviour
{
    private static string GetPackagePath()
    {
        return "Packages/net.cyanseraph.inventorysystem";
    }

    private static void _SpawnPrefabInEditor(GameObject prefab, string name)
    {
        prefab.name = name;

        PrefabUtility.UnpackPrefabInstance(prefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        if (Selection.activeTransform != null)
        {
            prefab.transform.SetParent(Selection.activeTransform, false);
        }
        prefab.transform.localPosition = Vector3.zero;
        prefab.transform.localEulerAngles = Vector3.zero;
        prefab.transform.localScale = Vector3.one;
    }


    private static string ManagerEditorName = "InventoryManager";
    [MenuItem("GameObject/UI/Cyan Inventory/Manager/Empty Manager", false, 1)]
    private static void CreateNewEmptyManager()
    {
        GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetPackagePath() + "\\Runtime\\Prefabs\\InventorySystemBase01.prefab"));
        _SpawnPrefabInEditor(prefab, ManagerEditorName);
    }
    [MenuItem("GameObject/UI/Cyan Inventory/Manager/Basic Manager", false, 2)]
    private static void CreateNewBasicManager()
    {
        GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetPackagePath() + "\\Runtime\\Prefabs\\InventorySystemBase02.prefab"));
        _SpawnPrefabInEditor(prefab, ManagerEditorName);
    }


    private static string ContainerEditorName = "Container";
    [MenuItem("GameObject/UI/Cyan Inventory/Container", false, 3)]
    private static void CreateNewEmptyContainer()
    {
        GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetPackagePath() + "\\Runtime\\Prefabs\\InventoryContainerBase01.prefab"));
        _SpawnPrefabInEditor(prefab, ContainerEditorName);
    }

    private static string SlotEditorName = "Slot";
    [MenuItem("GameObject/UI/Cyan Inventory/Slot/Empty Slot", false, 4)]
    private static void CreateNewEmptySlot()
    {
        GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetPackagePath() + "\\Runtime\\Prefabs\\InventorySlotBase01.prefab"));
        _SpawnPrefabInEditor(prefab, SlotEditorName);
    }
    [MenuItem("GameObject/UI/Cyan Inventory/Slot/Basic Slot", false, 5)]
    private static void CreateNewBasicSlot()
    {
        GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetPackagePath() + "\\Runtime\\Prefabs\\InventorySlotBase02.prefab"));
        _SpawnPrefabInEditor(prefab, SlotEditorName);
    }

    private static string ItemEditorName = "Item";
    [MenuItem("GameObject/UI/Cyan Inventory/Item/Empty Item", false, 6)]
    private static void CreateNewEmptyItemt()
    {
        GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetPackagePath() + "\\Runtime\\Prefabs\\InventoryItemBase01.prefab"));
        _SpawnPrefabInEditor(prefab, ItemEditorName);
    }
    [MenuItem("GameObject/UI/Cyan Inventory/Item/Basic Item", false, 7)]
    private static void CreateNewBasicItem()
    {
        GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetPackagePath() + "\\Runtime\\Prefabs\\InventoryItemBase02.prefab"));
        _SpawnPrefabInEditor(prefab, ItemEditorName);
    }
}
