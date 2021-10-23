using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckCard : MonoBehaviour
{
    public Deck deck;
    public TacticsWindow tacticsWindow;
    public Card card;
    public Text unitName;
    public Text nickName;
    public Text cost;
    public Text power;
    public Text magic;
    public Text skill;
    public Text type;
    public Image illust;
    public Image job;
    public Text tCost;
    public Text tName;

    public void LeftClick()
    {
        if (Input.GetMouseButton(0)) deck.DeckReSet(this);
    }
    public void RightClick()
    {
        if (Input.GetMouseButton(1)) tacticsWindow.Display(card);
    }
}
