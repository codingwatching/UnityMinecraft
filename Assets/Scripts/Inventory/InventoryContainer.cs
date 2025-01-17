﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryContainer : MonoBehaviour
{
	/// <summary>
	/// The UUID associated with the inventory container.
	/// </summary>
	private string id;

	/// <summary>
	/// Reference to the item objects in the grid.
	/// </summary>
	public GameObject[] itemObjects;

	/// <summary>
	/// Reference to the inventory items.
	/// </summary>
	public InventoryItem[] items;

	/// <summary>
	/// The number of items the inventory will contain.
	/// </summary>
	[SerializeField]
	private int itemsCount;

	/// <summary>
	/// Determines whether the inventory container is used for a 2x2 crafting grid (or 3x3 if `false`).
	/// </summary>
	public bool is2x2CraftingGrid;

	/// <summary>
	/// The name of the item container.
	/// </summary>
	public string itemContainerName;

	/// <summary>
	/// Signature for the ItemsChanged event.
	/// Called when the items within the inventory container change.
	/// </summary>
	public delegate void ItemsChangedDelegate();

	/// <summary>
	/// Contains listeners to the ItemsChanged event.
	/// </summary>
	public ItemsChangedDelegate itemsChangedEvent;

    void Awake()
    {
		// Register inventory in InventoryContainers
		this.id = System.Guid.NewGuid().ToString();
		InventoryContainers.containers[this.id] = this;

		if (this.itemContainerName == "hotbar")
			InventoryContainers.hotbar = this;

		if (this.itemContainerName == "items")
			InventoryContainers.inventory = this;

		GridLayoutGroup layoutGroup = this.GetComponent<GridLayoutGroup>();
		layoutGroup.cellSize 		= new Vector2(32, 32);
		layoutGroup.spacing 		= new Vector2(4, 3);

        // Initialize the inventory item objects.
		this.itemObjects = new GameObject[this.itemsCount];

		// Initialize the inventory items.
		this.items = new InventoryItem[this.itemsCount];

		for (int i = 0; i < this.itemsCount; i++)
			this.itemObjects[i] = this.CreateItemSlotObject(i);

		this.UpdateGUI();
    }

	/// <summary>
	/// Updates the inventory container GUI.
	/// </summary>
	public void UpdateGUI()
	{
		for(int i = 0; i < this.itemsCount; i++)
		{
			InventoryItem item 		= this.items[i];
			GameObject itemObj 		= this.itemObjects[i];

			if (item == null || item?.quantity == 0)
				this.items[i] = null;
			
			this.UpdateSingleItem(item, itemObj);
		}
	}

	/// <summary>
	/// Returns a 3x3 string matrix representing the inv. items as crafting requirements.
	/// </summary>
	public string[,] ItemsToCraftingRequirements()
	{
		string[,] requirements 	= new string[3,3];
		int matrixLength 		= this.is2x2CraftingGrid ? 2 : 3;

		for (int row = 0; row < matrixLength; row++)
				for (int column = 0; column < matrixLength; column++)
					requirements[row,column] = this.items[row + column*matrixLength]?.itemName;

		return requirements;
	}

	/// <summary>
	/// Allows to trigger the ItemsChanged event.
	/// </summary>
	public void TriggerItemsChangedEvent()
	{
		for (int i = 0; i < this.items.Length; i++)
			if (this.items[i]?.quantity == 0)
				this.items[i] = null;

		if (GUI.isAGUIShown)
			GUI.activeGUI.UpdateGUI();

		if (this.itemsChangedEvent != null)
			this.itemsChangedEvent();
	}

	/// <summary>
	/// Given an `InventoryItem` and its inventory GameObject (object that contains the the image & quantity text),
	/// updates the single item's texture and quantity text.
	/// </summary>
	private void UpdateSingleItem(InventoryItem item, GameObject inventoryItemGameObject)
	{
		Image image				= inventoryItemGameObject.GetComponent<Image>();
		GameObject quantityText	= inventoryItemGameObject.transform.GetChild(0).gameObject;

		if (item == null || item?.quantity == 0) 
		{
			inventoryItemGameObject.GetComponent<InventoryItemSlot>().itemName = null;
			image.color = Color.clear;
			quantityText.SetActive(false);
			return;
		}

		string itemName = item.itemName;

		inventoryItemGameObject.GetComponent<InventoryItemSlot>().itemName = itemName;
		image.sprite = TextureStitcher.instance.GetBlockItemSprite(itemName);
		image.color = Color.white;

		if (item.quantity != 1)
		{
			quantityText.SetActive(true);
			quantityText.GetComponent<Text>().text = item.quantity.ToString();

			int fontSize = item.quantity > 99 ? 18 : 22;
			quantityText.GetComponent<Text>().fontSize = fontSize;
		}
		else
			quantityText.SetActive(false);
	}

	/// <summary>
	/// Allows to create an item slot object to append to the grid.
	/// </summary>
	private GameObject CreateItemSlotObject(int index)
	{
		GameObject itemSlotObject = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/UIItemSlot"));
		itemSlotObject.name = String.Format("item{0}", index);
		itemSlotObject.transform.SetParent(this.transform, false);

		switch(this.name)
		{
			case "Items":
				itemSlotObject.GetComponent<InventoryItemSlot>().slotType = InventorySlotType.INVENTORY;
				break;
			case "Hotbar":
				itemSlotObject.GetComponent<InventoryItemSlot>().slotType = InventorySlotType.HOTBAR;
				break;
			case "Crafting":
				itemSlotObject.GetComponent<InventoryItemSlot>().slotType = InventorySlotType.CRAFTING;
				break;
			default:
				itemSlotObject.GetComponent<InventoryItemSlot>().slotType = InventorySlotType.INVENTORY;
				break;
		}

		itemSlotObject.GetComponent<InventoryItemSlot>().slotIndex = index;

		return itemSlotObject;
	}
}
