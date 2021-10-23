using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListCard : MonoBehaviour
{
    Deck deck;
    TacticsWindow tacticsWindow;
    public bool select;
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

    void Start()
    {
        deck = GameObject.Find("Deck").GetComponent<Deck>();
        tacticsWindow = GameObject.Find("TWindow").GetComponent<TacticsWindow>();
    }
    public void LeftClick()
    {
        if (Input.GetMouseButton(0)) deck.CardSelect(card);
    }
    public void RightClick()
    {
        if (Input.GetMouseButton(1)) tacticsWindow.Display(card);
    }
}
