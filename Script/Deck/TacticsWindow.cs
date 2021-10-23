using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TacticsWindow : MonoBehaviour
{
    public Sprite[] statusColor;
    public Text cost;
    public Text cutinName;
    public Text cutinTime;
    public Image[] statusImage;
    public Text[] statusName;
    public Text[] statusNum;
    public Text cutinText;
    public Text[] target;
    public GameObject unit;
    public Image field;
    public Image area;
    public Sprite[] shape;

    public void Display(Card card)
    {
        cutinName.text = card.tacticsName;
        cutinTime.text = card.tacticsTime.ToString();
        cutinText.text = card.tacticsText;
        cost.text = "Cost:" + card.tacticsCost.ToString();
        field.color = new Color(1, 1, 1);
        switch (card.color)
        {
            case "��":
                cutinName.color = new Color(1, 0, 0);
                break;
            case "��":
                cutinName.color = new Color(0, 1, 0);
                break;
            case "��":
                cutinName.color = new Color(0, 0, 1);
                break;
            case "��":
                cutinName.color = new Color(1, 1, 0);
                break;
            case "��":
                cutinName.color = new Color(1, 0, 1);
                break;
        }
        int statusSum = 0;
        for (int i = 0; i < statusImage.Length; i++)
        {
            statusImage[i].color = new Color(0, 0, 0, 1);
            statusName[i].text = "";
            statusNum[i].text = "";
        }

        if (card.p != 0)
        {
            statusImage[statusSum].sprite = statusColor[0];
            statusImage[statusSum].color = new Color(1, 1, 1, 1);
            statusName[statusSum].text = "Power";
            statusNum[statusSum].text = card.p.ToString();
            if (card.p > 0) statusNum[statusSum].text = "+" + card.p.ToString();
            statusSum++;
        }
        if (card.m != 0)
        {
            statusImage[statusSum].sprite = statusColor[1];
            statusImage[statusSum].color = new Color(1, 1, 1, 1);
            statusName[statusSum].text = "Magic";
            statusNum[statusSum].text = card.m.ToString();
            if (card.m > 0) statusNum[statusSum].text = "+" + card.m.ToString();
            statusSum++;
        }
        if (card.sk != 0)
        {
            statusImage[statusSum].sprite = statusColor[2];
            statusImage[statusSum].color = new Color(1, 1, 1, 1);
            statusName[statusSum].text = "Skill";
            statusNum[statusSum].text = card.sk.ToString();
            if (card.sk > 0) statusNum[statusSum].text = "+" + card.sk.ToString();
            statusSum++;
        }
        if (card.mh != 0)
        {
            statusImage[statusSum].sprite = statusColor[3];
            statusImage[statusSum].color = new Color(1, 1, 1, 1);
            statusName[statusSum].text = "MaxHP";
            statusNum[statusSum].text = card.mh.ToString();
            if (card.mh > 0) statusNum[statusSum].text = "+" + card.mh.ToString();
            statusSum++;
        }
        if (card.h != 0)
        {
            statusImage[statusSum].sprite = statusColor[3];
            statusImage[statusSum].color = new Color(1, 1, 1, 1);
            statusName[statusSum].text = "HP";
            statusNum[statusSum].text = card.h.ToString();
            if (card.h > 0) statusNum[statusSum].text = "+" + card.h.ToString();
            statusSum++;
        }
        if (card.sp != 0)
        {
            statusImage[statusSum].sprite = statusColor[2];
            statusImage[statusSum].color = new Color(1, 1, 1, 1);
            statusName[statusSum].text = "Speed";
            statusNum[statusSum].text = card.sp.ToString();
            if (card.sp > 0) statusNum[statusSum].text = "+" + card.sp.ToString();
            statusSum++;
        }
        if (card.a != 0)
        {
            statusImage[statusSum].sprite = statusColor[0];
            statusImage[statusSum].color = new Color(1, 1, 1, 1);
            statusName[statusSum].text = "Atk";
            statusNum[statusSum].text = card.a.ToString();
            if (card.a > 0) statusNum[statusSum].text = "+" + card.a.ToString();
            statusSum++;
        }
        if (card.d != 0)
        {
            statusImage[statusSum].sprite = statusColor[3];
            statusImage[statusSum].color = new Color(1, 1, 1, 1);
            statusName[statusSum].text = "P-Def";
            statusNum[statusSum].text = card.d.ToString();
            if (card.d > 0) statusNum[statusSum].text = "+" + card.d.ToString();
            statusSum++;
        }
        if (card.md != 0)
        {
            statusImage[statusSum].sprite = statusColor[1];
            statusImage[statusSum].color = new Color(1, 1, 1, 1);
            statusName[statusSum].text = "M-Def";
            statusNum[statusSum].text = card.md.ToString();
            if (card.md > 0) statusNum[statusSum].text = "+" + card.md.ToString();
            statusSum++;
        }
        if (card.bp != 0)
        {
            statusImage[statusSum].sprite = statusColor[4];
            statusImage[statusSum].color = new Color(1, 1, 1, 1);
            statusName[statusSum].text = "Break-P";
            statusNum[statusSum].text = card.bp.ToString();
            if (card.bp > 0) statusNum[statusSum].text = "+" + card.bp.ToString();
            statusSum++;
        }
        if (card.bs != 0)
        {
            statusImage[statusSum].sprite = statusColor[4];
            statusImage[statusSum].color = new Color(1, 1, 1, 1);
            statusName[statusSum].text = "Break-S";
            statusNum[statusSum].text = card.bs.ToString();
            if (card.bs > 0) statusNum[statusSum].text = "+" + card.bs.ToString();
            statusSum++;
        }

        foreach(Text text in target)
        {
            text.color = new Color(1, 1, 1, 0.3f);
        }
        target[8].text = "����";
        unit.transform.localPosition = new Vector3(0, -100, 0);
        if (card.tacticsAreaType == "�~�`") area.sprite = shape[0];
        if (card.tacticsAreaType == "�l�p") area.sprite = shape[1];
        if (card.tacticsAreaType == "���") field.color = new Color(1, 1, 1);
        switch (card.tacticsTarget)
        {
            case "����":
                target[0].color = new Color(1, 1, 1, 1);
                target[3].color = new Color(1, 1, 1, 1);
                break;
            case "�����P��":
                target[0].color = new Color(1, 1, 1, 1);
                target[4].color = new Color(1, 1, 1, 1);
                break;
            case "�G�P��":
                target[1].color = new Color(1, 1, 1, 1);
                target[4].color = new Color(1, 1, 1, 1);
                break;
            case "���P��":
                target[2].color = new Color(1, 1, 1, 1);
                target[4].color = new Color(1, 1, 1, 1);
                break;
            case "�����S��":
                target[0].color = new Color(1, 1, 1, 1);
                target[5].color = new Color(1, 1, 1, 1);
                break;
            case "�G�S��":
                target[1].color = new Color(1, 1, 1, 1);
                target[5].color = new Color(1, 1, 1, 1);
                break;
            case "�S��":
                target[2].color = new Color(1, 1, 1, 1);
                target[5].color = new Color(1, 1, 1, 1);
                break;
        }
        if (card.tacticsAreaSpin=="�\") target[6].color = new Color(1, 1, 1, 1);
        if (card.tacticsRegion == "�̈�") target[7].color = new Color(1, 1, 1, 1);
        if (card.tacticsField == "���_")
        {
            target[8].color = new Color(1, 1, 1, 1);
            target[8].text = "���_";
            unit.transform.localPosition = new Vector3(0, -225, 0);
        }
        area.transform.localPosition = new Vector3(card.tacticsAreaCenterX*4, card.tacticsAreaCenterY*4 - 100, 0);
        area.GetComponent<RectTransform>().sizeDelta = new Vector2(card.tacticsAreaSizeX*4, card.tacticsAreaSizeY*4);




        gameObject.transform.localPosition = new Vector3(0, 350, 0);
    }
    public void Back()
    {
        gameObject.transform.localPosition = new Vector3(0, 2000, 0);
    }
}
