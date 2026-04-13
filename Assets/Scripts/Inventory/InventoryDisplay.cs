using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDisplay : MonoBehaviour, IInventoryHolder
{
    [SerializeField] private SlotCard _slotCardPrefab;

    public Inventory Inventory
    {
        get => _inventory;
        set
        {
            _inventory = value;

            UpdateSlotsCount();
            UpdateSlotsInfo();
        }
    }

    private Inventory _inventory;

    private List<SlotCard> _slotCards = new();

    public void UpdateSlotsCount()
    {
        if (_inventory is null)
        {
            DestroySlots();
            return;
        }

        if (_slotCards.Count > _inventory.SlotsCount)
        {
            int excess = _slotCards.Count - _inventory.SlotsCount;

            for (int i = 0; i < excess; ++i)
                DestroySlot(_slotCards[_slotCards.Count - 1]);
        }
        else if (_slotCards.Count < _inventory.SlotsCount)
        {
            int deficit = _inventory.SlotsCount - _slotCards.Count;

            for (int i = 0; i < deficit; ++i)
                SpawnSlotCard();
        }
    }

    public void UpdateSlotsInfo()
    {
        if (_inventory is null)
        {
            DestroySlots(); //заменить на disable
            return;
        }

        IReadOnlyList<Slot> inventorySlots = Inventory.Slots;

        for (int i = 0; i < Inventory.SlotsCount; ++i)
        {
            SlotCard cardToUpdate = _slotCards[i];
            cardToUpdate.Slot = inventorySlots[i];
            cardToUpdate.UpdateInfo();
        }
    }

    private SlotCard SpawnSlotCard()
    {
        SlotCard newSlotCard = Instantiate(_slotCardPrefab, transform);
        _slotCards.Add(newSlotCard);

        return newSlotCard;
    }

    private void DestroySlot(SlotCard slotToDestroy)
    {
        if (slotToDestroy is null) return;

        Destroy(slotToDestroy.gameObject);
        _slotCards.Remove(slotToDestroy);
    }

    private void DestroySlots()
    {
        while(_slotCards.Count > 0)
            DestroySlot(_slotCards[0]);
    }

    private void OnDestroy()
    {
        DestroySlots();
    }
}
