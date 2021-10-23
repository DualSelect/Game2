using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Deck : MonoBehaviour
{
    public InputField playerName;
    public Button next;
    public CardMaster cardMaster;
    public CommandMaster commandMaster;
    public GameObject listCardPrefab;
    public GameObject listContent;
    public Sprite[] jobMark;
    public Dropdown min;
    public Dropdown max;
    public Dropdown sort;
    public Sprite dummy;

    public Text[] total; 


    List<ListCard> listCards = new List<ListCard>();
    public List<DeckCard> deckCards;
    public List<DeckCommand> deckCommands;

    public Image[] colorButton;
    public bool[] colorSelect = new bool[5] {true, true, true, true, true};
    public Image[] jobButton;
    public bool[] jobSelect = new bool[6] { false,true, true, true, true, true };

    void Start()
    {
        BGMManager.Instance.Play(BGMPath.DECK0, 0.8f);
        playerName.text = PlayerPrefs.GetString("playerName", "ÉvÉåÉCÉÑÅ[ñº");
        List<string> option = new List<string>();
        foreach (Command command in commandMaster.CommandList) {
            option.Add("Cost"+command.cost.ToString()+" "+command.name);
        }
        foreach (DeckCommand deckCommand in deckCommands)
        {
            deckCommand.commandList.ClearOptions();
            deckCommand.commandList.AddOptions(option);
        }
        SetCommandLeft(PlayerPrefs.GetInt("command0", 1)-1);
        SetCommandRight(PlayerPrefs.GetInt("command1", 2)-1);
        Total();
        StartCoroutine(DeckSet(cardMaster.CardList.Find(c => c.colorNo == PlayerPrefs.GetString("unit0", "G1")), deckCards[0]));
        StartCoroutine(DeckSet(cardMaster.CardList.Find(c => c.colorNo == PlayerPrefs.GetString("unit1", "G2")), deckCards[1]));
        StartCoroutine(DeckSet(cardMaster.CardList.Find(c => c.colorNo == PlayerPrefs.GetString("unit2", "G3")), deckCards[2]));
        StartCoroutine(DeckSet(cardMaster.CardList.Find(c => c.colorNo == PlayerPrefs.GetString("unit3", "G5")), deckCards[3]));
        StartCoroutine(DeckSet(cardMaster.CardList.Find(c => c.colorNo == PlayerPrefs.GetString("unit4", "G7")), deckCards[4]));
        StartCoroutine(DeckSet(cardMaster.CardList.Find(c => c.colorNo == PlayerPrefs.GetString("unit5", "")), deckCards[5]));

        foreach (Card card in cardMaster.CardList)
        {
            if (card.no >0 && card.ok!="")
            {
                ListCard listCard = (Instantiate(listCardPrefab, listContent.transform) as GameObject).GetComponent<ListCard>();
                StartCoroutine(CardLoad(card, listCard));
                listCards.Add(listCard);
            }
        }
        listCards.Sort((a, b) => a.card.colorNoSort - b.card.colorNoSort);
        LineUp();
    }
    public void SortList()
    {
        if(sort.captionText.text == "Color")
        {
            listCards.Sort((a, b) => a.card.colorSort - b.card.colorSort);
        }
        if (sort.captionText.text == "Job")
        {
            listCards.Sort((a, b) => a.card.jobNo - b.card.jobNo);
        }
        if (sort.captionText.text == "Cost")
        {
            listCards.Sort((a, b) => b.card.cost - a.card.cost);
        }
        if (sort.captionText.text == "Power")
        {
            listCards.Sort((a, b) => b.card.power - a.card.power);
        }
        if (sort.captionText.text == "Magic")
        {
            listCards.Sort((a, b) => b.card.magic - a.card.magic);
        }
        if (sort.captionText.text == "Skill")
        {
            listCards.Sort((a, b) => b.card.skill - a.card.skill);
        }
        LineUp();
    }
    public void LineUp()
    {
        foreach (ListCard listCard in listCards)
        {
            listCard.transform.localPosition = new Vector3(-150, 0, 0);
        }
        int i = 0;
        foreach (ListCard listCard in listCards)
        {
            if (colorSelect[listCard.card.colorSort] && jobSelect[listCard.card.jobNo] && int.Parse(min.captionText.text) <= listCard.card.cost && listCard.card.cost <= int.Parse(max.captionText.text))
            {
                listCard.transform.localPosition = new Vector3(150+300*i, -155, 0);
                i++;
            }
        }
        listContent.GetComponent<RectTransform>().sizeDelta = new Vector2(300*i,300);
    }
    IEnumerator CardLoad(Card card,ListCard listCard)
    {
        var illust = Addressables.LoadAssetAsync<Sprite>(card.colorNo);
        listCard.card = card;
        listCard.unitName.text = card.name;
        listCard.nickName.text = card.nickName;
        listCard.cost.text = card.cost.ToString();
        listCard.power.text = card.power.ToString();
        listCard.magic.text = card.magic.ToString();
        listCard.skill.text = card.skill.ToString();
        listCard.unitName.text = card.name;
        listCard.job.sprite = jobMark[card.jobNo];
        if(card.magicType!="")listCard.type.text = card.magicType[0].ToString();
        switch (card.color)
        {
            case "ê‘":
                listCard.job.color = new Color(1, 0, 0);
                break;
            case "óŒ":
                listCard.job.color = new Color(0, 1, 0);
                break;
            case "ê¬":
                listCard.job.color = new Color(0, 0, 1);
                break;
            case "îí":
                listCard.job.color = new Color(1, 1, 0);
                break;
            case "çï":
                listCard.job.color = new Color(1, 0, 1);
                break;
        }

        yield return illust;
        listCard.illust.sprite = illust.Result;
    }
    IEnumerator DeckSet(Card card, DeckCard deckCard)
    {
        if (card == null)
        {
            deckCard.card = null;
            yield break;
        }
        var illust = Addressables.LoadAssetAsync<Sprite>(card.colorNo);
        deckCard.card = card;
        deckCard.unitName.text = card.name;
        deckCard.nickName.text = card.nickName;
        deckCard.cost.text = card.cost.ToString();
        deckCard.power.text = card.power.ToString();
        deckCard.magic.text = card.magic.ToString();
        deckCard.skill.text = card.skill.ToString();
        deckCard.unitName.text = card.name;
        deckCard.job.sprite = jobMark[card.jobNo];
        deckCard.tCost.text = card.tacticsCost.ToString();
        deckCard.tName.text = card.tacticsName;
        if (card.magicType != "") deckCard.type.text = card.magicType[0].ToString();
        switch (card.color)
        {
            case "ê‘":
                deckCard.job.color = new Color(1, 0, 0);
                break;
            case "óŒ":
                deckCard.job.color = new Color(0, 1, 0);
                break;
            case "ê¬":
                deckCard.job.color = new Color(0, 0, 1);
                break;
            case "îí":
                deckCard.job.color = new Color(1, 1, 0);
                break;
            case "çï":
                deckCard.job.color = new Color(1, 0, 1);
                break;
        }

        yield return illust;
        deckCard.illust.sprite = illust.Result;
        Total();
    }
    public void DeckReSet(DeckCard deckCard)
    {
        deckCard.card = null;
        deckCard.unitName.text = "";
        deckCard.nickName.text = "";
        deckCard.cost.text = "";
        deckCard.power.text = "";
        deckCard.magic.text = "";
        deckCard.skill.text = "";
        deckCard.unitName.text = "";
        deckCard.job.sprite = dummy;
        deckCard.tCost.text = "";
        deckCard.tName.text = "";
        deckCard.type.text = "";
        deckCard.illust.sprite = dummy;
        Total();
    }
    public void CardSelect(Card card)
    {
        foreach(DeckCard deck in deckCards)
        {
            if(deck.card!=null)if (deck.card == card) return; 
        }
        foreach (DeckCard deck in deckCards)
        {
            if (deck.card == null)
            {
                StartCoroutine(DeckSet(card, deck));
                return;
            }
        }
    }
    public void ColorSwitch(int i)
    {
        if (colorSelect[i]) colorButton[i].color = new Color(1, 1, 1, 0.5f);
        else colorButton[i].color = new Color(1, 1, 1, 1);
        colorSelect[i] = !colorSelect[i];
        LineUp();
    }
    public void JobSwitch(int i)
    {
        if (jobSelect[i]) jobButton[i].color = new Color(1, 1, 1, 0.5f);
        else jobButton[i].color = new Color(1, 1, 1, 1);
        jobSelect[i] = !jobSelect[i];
        SortList();
    }
    public void SetCommandLeft(int i)
    {
        Command command = commandMaster.CommandList.Find(c => c.no == i + 1);
        deckCommands[0].commandList.value = i;
        deckCommands[0].command = command;
        deckCommands[0].passive.text = command.passive;
        deckCommands[0].force.text = command.force;
        deckCommands[0].time.text = command.count.ToString();
        deckCommands[0].condition.text = command.condition;
        deckCommands[0].active.text = command.active;
        Total();
    }
    public void SetCommandRight(int i)
    {
        Command command = commandMaster.CommandList.Find(c => c.no == i + 1);
        deckCommands[1].commandList.value = i;
        deckCommands[1].command = command;
        deckCommands[1].passive.text = command.passive;
        deckCommands[1].force.text = command.force;
        deckCommands[1].time.text = command.count.ToString();
        deckCommands[1].condition.text = command.condition;
        deckCommands[1].active.text = command.active;
        Total();
    }
    void Total()
    {
        int[] num = new int[5] { 0, 0, 0, 0, 0 };
        bool[] colorBool = new bool[] { false, false, false, false, false };
        foreach (DeckCommand deckCommand in deckCommands)
        {
            num[0] += deckCommand.command.cost;
        }
        foreach (DeckCard deck in deckCards)
        {
            if (deck.card == null) continue;
            num[0] += deck.card.cost;
            num[1] += deck.card.power;
            num[2] += deck.card.magic;
            num[3] += deck.card.skill;
            switch (deck.card.color)
            {
                case "ê‘":
                    colorBool[0] = true;
                    break;
                case "óŒ":
                    colorBool[1] = true;
                    break;
                case "ê¬":
                    colorBool[2] = true;
                    break;
                case "îí":
                    colorBool[3] = true;
                    break;
                case "çï":
                    colorBool[4] = true;
                    break;
            }
        }
        foreach (bool color in colorBool)
        {
            if (color) num[4]++;
        }
        for(int i = 0; i < total.Length; i++)
        {
            total[i].text = num[i].ToString();
        }
        if (num[0] == 21)
        {
            next.interactable = true;
        }
        else
        {
            next.interactable = false;
        }
    }
    public void Next()
    {
        PlayerPrefs.SetString("playerName", playerName.text);
        for (int i = 0; i < deckCommands.Count; i++)
        {
            PlayerPrefs.SetInt("command" + i.ToString(), deckCommands[i].command.no);
        }
        for (int i = 0; i < deckCards.Count; i++)
        {
            if (deckCards[i].card!=null) PlayerPrefs.SetString("unit" + i.ToString(), deckCards[i].card.colorNo);
            else PlayerPrefs.SetString("unit" + i.ToString(), "");
        }
        SceneManager.LoadScene("Battle");
    }
}
