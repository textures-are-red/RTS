using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExistsUnitsShower : MonoBehaviour
{
    [SerializeField] private UnitCard _cardPrefab;

    [Space(15)]

    [SerializeField] private UnitController _unitController;
    [SerializeField] private Transform _content;

    private List<UnitCard> _unitCards = new();

    public void UpdateUI()
    {
        while(_unitCards.Count is not 0)
            Delete(_unitCards[0]);

        for (byte i = 0; i < _unitController.ExistsUnits.Count; ++i)
            UpdateUIOnAdd(i);
    }

    private void OnUnitsChange(object sender, NotifyCollectionChangedEventArgs context)
    {
        switch (context.Action)
        {
            case NotifyCollectionChangedAction.Add: UpdateUIOnAdd(context.NewStartingIndex); break;
            case NotifyCollectionChangedAction.Remove: UpdateUIOnRemove(context.OldStartingIndex); break;
            default: UpdateUI(); break;
        }
    }

    private void UpdateUIOnAdd(int index)
    {
        Unit unitToShow = _unitController.ExistsUnits[index];
        UnitCard card = CreateUnitCard();

        Color colorToShow = unitToShow.UnitColor;
        colorToShow.a = 1f;

        card.Image.color = colorToShow;
        card.Text.text = "lvl " + unitToShow.Level;
    }

    private void UpdateUIOnRemove(int index)
    {
        Delete(_unitCards[index]);
    }

    private void UpdateCardAt(int index, sbyte newLevel)
    {
        print($"UpdateCardAt {index}");
        _unitCards[index].Text.text = "lvl " + newLevel;
    }

    private void Delete(UnitCard card)
    {
        Destroy(card.gameObject);
        _unitCards.Remove(card);
    }

    private UnitCard CreateUnitCard()
    {
        var card = Instantiate(_cardPrefab, _content); //pool required
        _unitCards.Add(card);
        return card;
    }

    private void OnEnable()
    {
        StartCoroutine(SubscribeWhenNotNull());

        _unitController.LevelChanged += UpdateCardAt;
    }

    private IEnumerator SubscribeWhenNotNull()
    {
        yield return new WaitUntil(() => _unitController.ExistsUnits is not null);
        ((INotifyCollectionChanged)_unitController.ExistsUnits).CollectionChanged += OnUnitsChange;

        UpdateUI();
    }

    private void OnDisable()
    {
        UpdateUI();
        ((INotifyCollectionChanged)_unitController.ExistsUnits).CollectionChanged -= OnUnitsChange;

        _unitController.LevelChanged -= UpdateCardAt;
    }
}
