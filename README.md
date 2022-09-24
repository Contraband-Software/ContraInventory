# Cyan Inventory System

A bare-bones, EXTREMELY customizable inventory system framework.

## Usage

### Important Prerequisites

 - You MUST load the tags included in the `InventorySystemTagPreset.preset` preset file in the packages runtime directory, this adds the necessary Unity tags for the package to function
 - ALL CONTAINERS MUST HAVE THE EXACT SAME ANCHORED POSITION WITHIN THE CONTAINERS-PARENT, OTHERWISE THE ITEM SNAPPING WILL BREAK.
 - References to the parent canvas must be added to the inventory manager.
 - ALL ITEMS AND SLOTS MUST HAVE A UNIQUE NAME, otherwise you will get red errors

### Starting

## Best practices

 - The inventory manager should only contain a "containers" container GameObject and an "items" container GameObject, each of these GameObjects should also only contain their respective GameObjects - You run the risk of causing breakages if you do not follow this.

## Limitations

Currently does not support stackable items