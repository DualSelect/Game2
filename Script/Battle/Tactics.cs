using KanKikuchi.AudioManager;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tactics : MonoBehaviour
{
    public GameObject onLock;
    public GameObject offLock;
    public GameObject cutinBack;
    public GameObject tacticsGroup;
    public Unit selectUnit;
    public Text tacticsName;
    public Text tacticsCost;
    public Text tacticsPoint;
    public Text tacticsSpeed;
    public Button tacticsButton;
    public CardMaster cardMaster;
    public Unit activateUnit = null;
    public Unit activateUnitEnemy = null;
    public Sprite[] statusColor;
    public Fade tacticsFade;
    public Stage stage;
    public PunManager punManager;
    public GameObject buttonStop;
    public Text[] result;

    public float tacticsSpeedNow = 1.1f;
    public float tacticsSpeedNowEnemy = 1.1f;
    public float tacticsPointNow = 2;
    public float tacticsPointNowEnemy = 2;
    int activateTimes = 0;
    int activateTimesEnemy = 0;

    public Text cutinName;
    public Text cutinTime;
    public Image[] statusImage;
    public Text[] statusName;
    public Text[] statusNum;
    public Text cutinText;
    public Image[] tacticsTimeUnit;
    public Text[] tacticsTimeName;
    public Text[] tacticsTimeCount;

    public List<TacticsNow> tacticsNows = new List<TacticsNow>();
    List<Unit> tacticsTarget = new List<Unit>();
    List<Unit> tacticsTargetEnemy = new List<Unit>();

    public Text playerRed;
    public Text playerBlue;

    public IEnumerator TacticsSecounds()
    {
        tacticsPointNow += tacticsSpeedNow/100;
        if (tacticsPointNow >= 11) tacticsPointNow = 10.99f;
        tacticsPoint.text = (Math.Floor(tacticsPointNow * 10) / 10).ToString();
        tacticsPointNowEnemy += tacticsSpeedNowEnemy/100;
        if (tacticsPointNowEnemy >= 11) tacticsPointNowEnemy = 10.99f;
        foreach (TacticsNow now in tacticsNows)
        {
            now.tacticsSecond -= 0.1f;
        }
        yield return TacticsTimeUpdate();
        TacticsPossibleButton();
    }
    IEnumerator TacticsTimeUpdate()
    {
        bool end = false;
        while (!end)
        {
            end = true;
            for (int i = 0; i < tacticsNows.Count; i++)
            {
                if (tacticsNows[i].tacticsSecond < 0)
                {
                    yield return TacticsEnd(i);
                    end = false;
                    break;
                }
            }
        }
        TacticsInfoPlus();
    }
    void TacticsInfoPlus()
    {
        for (int i = 0; i < 6; i++)
        {
            tacticsTimeUnit[i].gameObject.SetActive(false);
            tacticsTimeName[i].text = "";
            tacticsTimeCount[i].text = "";
        }
        int player= 0;
        int enemy = 3;
        for (int i = 0; i < tacticsNows.Count; i++)
        {
            if (!tacticsNows[i].activateUnit.enemy)
            {
                tacticsTimeUnit[player].sprite = tacticsNows[i].activateUnit.chara.sprite;
                tacticsTimeUnit[player].gameObject.SetActive(true);
                tacticsTimeName[player].text = tacticsNows[i].activateUnit.card.tacticsName;
                tacticsTimeCount[player].text = Math.Floor(tacticsNows[i].tacticsSecond).ToString() + "s";
                player++;
            }
            else
            {
                tacticsTimeUnit[enemy].sprite = tacticsNows[i].activateUnit.chara.sprite;
                tacticsTimeUnit[enemy].gameObject.SetActive(true);
                tacticsTimeName[enemy].text = tacticsNows[i].activateUnit.card.tacticsName;
                tacticsTimeCount[enemy].text = Math.Floor(tacticsNows[i].tacticsSecond).ToString() + "s";
                enemy++;
            }
        }
    }
    public void TacticsPossibleButton()
    {
        tacticsButton.interactable = TacticsPossible(selectUnit);
        TacticsTarget();
    }
    bool TacticsPossible(Unit unit)
    {
        bool possible = false;
        if (unit != null)
        {
            Card card = unit.card;
            tacticsName.text = card.tacticsName;
            tacticsCost.text = (card.tacticsCost + card.tacticsCost * unit.activateTimes * 0.1f).ToString();
            bool deathCheck = false;
            bool healCheck = false;
            bool activeCheck = false;
            GameObject tactics3;
            if (unit.enemy) tactics3 = tacticsTimeUnit[5].gameObject;
            else tactics3 = tacticsTimeUnit[2].gameObject;
            float point;
            if (unit.enemy) point = tacticsPointNowEnemy;
            else point = tacticsPointNow;
            if (card.tacticsCost + card.tacticsCost * unit.activateTimes * 0.1f <= point)
            {
                if (!tactics3.activeSelf)
                {
                    if (!unit.death)
                    {
                        deathCheck = true;
                    }
                    if (!unit.heal)
                    {
                        healCheck = true;
                    }
                    if (!unit.activateNow)
                    {
                        activeCheck = true;
                    }
                }
            }

            if (deathCheck && healCheck && activeCheck) possible = true;
            if (unit.card.tacticsField == "拠点")
            {
                if (deathCheck && !healCheck && activeCheck) possible = true;
                else possible = false;
            }
            if (unit.card.tacticsField == "撤退")
            {
                if (!deathCheck && activeCheck) possible = true;
                else possible = false;
            }
        }
        return possible;
    }
    public void TacticsTarget()
    {
        foreach (GameObject gameObject in stage.unitList)
        {
            Unit unit = gameObject.GetComponent<Unit>();
            unit.tacticsArea.gameObject.SetActive(false);
            unit.tacticsTarget.SetActive(false);
        }
        foreach (GameObject gameObject in stage.unitListEnemy)
        {
            Unit unit = gameObject.GetComponent<Unit>();
            unit.tacticsArea.gameObject.SetActive(false);
            unit.tacticsTarget.SetActive(false);
        }
        if (tacticsButton.interactable)
        {
            tacticsTarget = new List<Unit>();
            selectUnit.tacticsArea.gameObject.SetActive(true);

            if (selectUnit.card.tacticsTarget == "自分")
            {
                tacticsTarget.Add(selectUnit);
            }
            if (selectUnit.card.tacticsTarget == "味方全員" || selectUnit.card.tacticsTarget == "全員")
            {
                List<GameObject> unitLists;
                if (selectUnit.enemy) unitLists = stage.unitListEnemy;
                else unitLists = stage.unitList;
                TargetAll(unitLists);
            }
            if (selectUnit.card.tacticsTarget == "敵全員" || selectUnit.card.tacticsTarget == "全員")
            {
                List<GameObject> unitLists;
                if (selectUnit.enemy) unitLists = stage.unitList;
                else unitLists = stage.unitListEnemy;
                TargetAll(unitLists);
            }
            if (selectUnit.card.tacticsTarget == "味方単体")
            {
                List<GameObject> unitLists;
                if (selectUnit.enemy) unitLists = stage.unitListEnemy;
                else unitLists = stage.unitList;
                TargetAll(unitLists);
                if (tacticsTarget.Count > 0) {
                    tacticsTarget.Sort((a, b) => b.card.cost - a.card.cost);
                    Unit unit = tacticsTarget[0];
                    tacticsTarget = new List<Unit>();
                    tacticsTarget.Add(unit);
                }
            }
            if (selectUnit.card.tacticsTarget == "敵単体")
            {
                List<GameObject> unitLists;
                if (selectUnit.enemy) unitLists = stage.unitList;
                else unitLists = stage.unitListEnemy;
                TargetAll(unitLists);
                if (tacticsTarget.Count > 0)
                {
                    tacticsTarget.Sort((a, b) => b.card.cost - a.card.cost);
                    Unit unit = tacticsTarget[0];
                    tacticsTarget = new List<Unit>();
                    tacticsTarget.Add(unit);
                }
            }
            if (selectUnit.card.tacticsTarget == "両単体")
            {
                List<GameObject> unitLists;
                if (selectUnit.enemy) unitLists = stage.unitListEnemy;
                else unitLists = stage.unitList;
                TargetAll(unitLists);
                Unit unit1 = null;
                if (tacticsTarget.Count > 0)
                {
                    tacticsTarget.Sort((a, b) => b.card.cost - a.card.cost);
                    unit1 = tacticsTarget[0];
                    tacticsTarget = new List<Unit>();
                }
                if (selectUnit.enemy) unitLists = stage.unitList;
                else unitLists = stage.unitListEnemy;
                TargetAll(unitLists);
                Unit unit2 = null;
                if (tacticsTarget.Count > 0)
                {
                    tacticsTarget.Sort((a, b) => b.card.cost - a.card.cost);
                    unit2 = tacticsTarget[0];
                    tacticsTarget = new List<Unit>();
                }
                if (unit1 != null) tacticsTarget.Add(unit1);
                if (unit2 != null) tacticsTarget.Add(unit2);
            }
            foreach(Unit unit in tacticsTarget)
            {
                unit.tacticsTarget.SetActive(true);
            }
        }
    }
    public IEnumerator TacticsStart(Unit unit)
    {
        if (TacticsPossible(unit))
        {
            unit.activateNow = true;
            //戦術情報の作成
            TacticsNow now = CreateTacticsNow(unit);
            //カットイン開始
            yield return TacticsCutin(unit, true,false);
            //基本能力上昇
            TacticsStatus(now, true,now.target);
            //特殊能力上昇
            yield return TacticsSpecial(now, true);
            //カットイン終了
            yield return TacticsCutin(unit, false,true);
            //コスト更新
            if (!unit.enemy)
            {
                TacticsPossibleButton();
                UnLock();
            }
            else
            {
                if (stage.pun)
                {
                    punManager.TacticsCostUpdate();
                }
            }
        }
        if (!unit.enemy) activateUnit = null;
        else activateUnitEnemy = null;
    }

    IEnumerator TacticsEnd(int i)
    {
        //基本能力上昇解除
        TacticsStatus(tacticsNows[i],false,tacticsNows[i].target);
        //特殊能力上昇解除
        yield return TacticsSpecial(tacticsNows[i], false);

        tacticsNows[i].activateUnit.activateNow = false;
        tacticsNows.RemoveAt(i);
    }
    TacticsNow CreateTacticsNow(Unit unit)
    {
        Card card = unit.card;
        TacticsNow now = new TacticsNow();
        if(!unit.enemy)tacticsPointNow -= card.tacticsCost + card.tacticsCost * unit.activateTimes * 0.1f;
        else tacticsPointNowEnemy -= card.tacticsCost + card.tacticsCost * unit.activateTimes * 0.1f;
        tacticsPoint.text = (Math.Floor(tacticsPointNow * 10) / 10).ToString();
        if (!unit.enemy)
        {
            now.actvateTimes = activateTimes;
            activateTimes++;
            if(card.tacticsRegion=="")now.target = tacticsTarget;
        }
        else
        {
            now.actvateTimes = activateTimesEnemy;
            activateTimesEnemy++;
            if (card.tacticsRegion == "") now.target = tacticsTargetEnemy;
        }
        unit.activateTimes++;
        now.activateUnit = unit;
        now.tacticsSecond = card.tacticsTime;
        tacticsNows.Add(now);
        TacticsInfoPlus();
        return now;
    }
 
    void TacticsStatus(TacticsNow now,bool start,List<Unit> target)
    {
        Card card = now.activateUnit.card;
        if (start)
        {
            foreach (Unit unit in target)
            {
                if(now.activateUnit.enemy==unit.enemy)unit.statusUp.gameObject.transform.localScale = unit.statusUp.gameObject.transform.localScale + new Vector3(4, 0, 0);
                else unit.statusDown.gameObject.transform.localScale = unit.statusDown.gameObject.transform.localScale + new Vector3(4, 0, 0);
                unit.powerNow += card.p;
                unit.magicNow += card.m;
                unit.skillNow += card.sk;
                unit.speedNow += card.sp;
                unit.breakNow += card.bp;
                unit.breakSpeedNow += card.bs;
                unit.hpMaxNow += card.mh*100;
                unit.atkNow += card.a;
                unit.defNow += card.d;
                unit.mDefNow += card.md;
                if (card.h > 0 && unit.hpNow < unit.hpMaxNow)
                {
                    unit.hpNow += card.h * 100;
                    if (unit.hpNow > unit.hpMaxNow) unit.hpNow = unit.hpMaxNow;
                    stage.DisplayHP(unit);
                }
                if (card.h < 0)
                {
                    unit.hpNow += card.h * 100;
                    stage.DisplayHP(unit);
                }
                unit.power.text = unit.powerNow.ToString();
                unit.magic.text = unit.magicNow.ToString();
                if (unit.jobNow == 1)
                {
                    if (unit.skillAct > unit.skillNow) unit.skillAct = unit.skillNow;
                    unit.skill.text = Math.Floor(unit.skillAct).ToString();
                }
                else if (unit.jobNow == 3)
                {
                    unit.skillAct += card.sk;
                    unit.skill.text = Math.Floor(unit.skillAct).ToString();
                }
                else if(unit.jobNow == 5)
                {
                    unit.skillAct += card.sk/2f;
                    if (unit.skillAct > unit.skillNow) unit.skillAct = unit.skillNow;
                    unit.skill.text = Math.Floor(unit.skillAct).ToString();
                }
                else 
                {
                    unit.skill.text = unit.skillNow.ToString();
                }
            }
        }
        else
        {
            foreach (Unit unit in target)
            {
                if (now.activateUnit.enemy == unit.enemy) unit.statusUp.gameObject.transform.localScale = unit.statusUp.gameObject.transform.localScale - new Vector3(4, 0, 0);
                else unit.statusDown.gameObject.transform.localScale = unit.statusDown.gameObject.transform.localScale - new Vector3(4, 0, 0);

                unit.powerNow -= card.p;
                unit.magicNow -= card.m;
                unit.skillNow -= card.sk;
                unit.speedNow -= card.sp;
                unit.breakNow -= card.bp;
                unit.breakSpeedNow -= card.bs;
                unit.hpMaxNow -= card.mh*100;
                unit.atkNow -= card.a;
                unit.defNow -= card.d;
                unit.mDefNow -= card.md;
                unit.power.text = unit.powerNow.ToString();
                unit.magic.text = unit.magicNow.ToString();
                if (unit.jobNow == 1)
                {
                    if (unit.skillAct > unit.skillNow) unit.skillAct = unit.skillNow;
                    unit.skill.text = Math.Floor(unit.skillAct).ToString();
                }
                else if (unit.jobNow == 3)
                {
                    unit.skillAct -= card.sk;
                    unit.skill.text = Math.Floor(unit.skillAct).ToString();
                }
                else if (unit.jobNow == 5)
                {
                    unit.skillAct -= card.sk/2f;
                    if (unit.skillAct > unit.skillNow) unit.skillAct = unit.skillNow;
                    unit.skill.text = Math.Floor(unit.skillAct).ToString();
                }
                else
                {
                    unit.skill.text = unit.skillNow.ToString();
                }
            }
        }
    }
    public IEnumerator TacticsSpecial(TacticsNow now, bool start)
    {
        if(now.activateUnit.card.tacticsRegion == "領域")
        {
            if (start)
            {
                now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta = now.activateUnit.tacticsArea.GetComponent<RectTransform>().sizeDelta;
                now.activateUnit.tacticsRegion.transform.localPosition = now.activateUnit.tacticsArea.transform.localPosition;
                now.activateUnit.tacticsRegion.sprite = now.activateUnit.tacticsArea.sprite;
                now.activateUnit.tacticsRegion.gameObject.SetActive(true);
            }
            else
            {
                now.activateUnit.tacticsRegion.gameObject.SetActive(false);
            }
        }

        switch (now.activateUnit.card.tacticsID)
        {
            case "TG1":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.actSpeedNow += 150f;
                    }
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.actSpeedNow -= 150f;
                    }
                }
                break;
            case "TG2":
                if (start)
                {
                    bool flag = false;
                    foreach (TacticsNow t in tacticsNows)
                    {
                        if(t.activateUnit.card.tacticsID == "TG3")
                        {
                            flag = true;
                            foreach (Unit unit in now.target)
                            {
                                unit.powerNow += 6;
                                unit.power.text = unit.powerNow.ToString();
                            }
                            break;
                        }
                    }
                    now.flag.Add(flag);
                }
                else
                {
                    if (now.flag[0])
                    {
                        foreach (Unit unit in now.target)
                        {
                            unit.powerNow -= 6;
                            unit.power.text = unit.powerNow.ToString();
                        }
                    }
                }
                break;
            case "TG3":
                if (start)
                {
                    bool flag = false;
                    foreach (TacticsNow t in tacticsNows)
                    {
                        if (t.activateUnit.card.tacticsID == "TG2")
                        {
                            flag = true;
                            foreach (Unit unit in now.target)
                            {
                                unit.skillNow += 6;
                                unit.skill.text = unit.skillNow.ToString();
                            }
                            break;
                        }
                    }
                    now.flag.Add(flag);
                }
                else
                {
                    if (now.flag[0])
                    {
                        foreach (Unit unit in now.target)
                        {
                            unit.skillNow -= 6;
                            unit.skill.text = unit.skillNow.ToString();
                        }
                    }
                }
                break;
            case "TG4":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        if (unit.jobNow == 3)
                        {
                            unit.skillAct = unit.skillNow + 0.99f;
                            unit.skill.text = unit.skillNow.ToString();
                        }
                    }
                }
                break;
            case "TG6":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.arrowTargetNum += 1;
                    }
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.arrowTargetNum -= 1;
                    }
                }
                break;
            case "TG7":
                if (start)
                {
                    now.activateUnit.actCoolTime += 999;
                }
                else
                {
                    now.activateUnit.actCoolTime -= 999;
                }
                break;

            case "TG8":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.actTacticsID = "TG8";
                    }
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.actTacticsID = "";
                    }
                }
                break;
            case "TG10":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        foreach (TacticsNow tactics in tacticsNows)
                        {
                            if (tactics.activateUnit.enemy != unit.enemy)
                            {
                                bool b = false;
                                foreach (Unit same in tactics.target)
                                {
                                    if (unit.enemy == same.enemy && unit.unitNum == same.unitNum)
                                    {
                                        b = true;
                                        break;
                                    }
                                }
                                if (b) tactics.tacticsSecond -= 4;
                            }
                        }
                    }
                    TacticsInfoPlus();
                }
                break;
            case "TG12":
                if (start)
                {
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.actCoolTime += 5;
                    }
                }
                break;
            case "TR2":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.reviveNow += 20;
                    }
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        if(!unit.death)yield return stage.UnitDeath(unit);
                        unit.reviveNow -= 20;
                    }
                }
                break;
            case "TR3":
                if (start)
                {
                    bool flag = false;
                    now.flag.Add(flag);

                }
                else
                {
                    if (now.flag[0])
                    {
                        Unit unit = now.target[0];
                        now.flag[0] = false;
                        unit.powerNow -= 3;
                        unit.power.text = unit.powerNow.ToString();
                    }
                }
                break;
            case "TR4":
                if (start)
                {
                    bool flag = false;
                    now.flag.Add(flag);
                }
                else
                {
                    if (now.flag[0])
                    {
                        Unit unit = now.target[0];
                        now.flag[0] = false;
                        unit.powerNow -= 2;
                        unit.power.text = unit.powerNow.ToString();
                    }
                }
                break;
            case "TR6":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        if(unit.card.nickName == "海賊")
                        {
                            unit.skillNow += 3;
                            ChangeJob(unit, unit.jobDf, 4);
                        }
                    }
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        if (unit.card.nickName == "海賊")
                        {
                            unit.skillNow -= 3;
                            ChangeJob(unit, unit.jobNow, unit.jobDf);
                        }
                    }
                }
                break;
            case "TR7":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.actTypeNow = "砲撃";
                        unit.arrowTargetNum -= 10;
                    }
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.actTypeNow = unit.actTypeDf;
                        unit.arrowTargetNum += 10;
                    }
                }
                break;
            case "TR10":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.actTypeNow = "爆発";
                    }
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.actTypeNow = unit.actTypeDf;
                    }
                }
                break;
            case "TR11":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        now.flag.Add(false);
                        now.flag2.Add(false);
                        now.flag3.Add(false);
                    }
                }
                else
                {
                    for (int i = 0 ; i <now.target.Count; i++)
                    {
                        Unit unit = now.target[i];
                        if (now.flag[i])
                        {
                            unit.powerNow -= 1;
                        }
                        if (now.flag2[i])
                        {
                            unit.powerNow -= 1;
                        }
                        if (now.flag3[i])
                        {
                            unit.powerNow -= 1;
                        }
                        unit.power.text = unit.powerNow.ToString();
                    }
                }
                break;
            case "TB3":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        ChangeJob(unit, unit.jobDf, 1);
                    }
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        ChangeJob(unit, unit.jobNow, unit.jobDf);
                    }
                }
                break;
            case "TB4":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.actSpeedNow += 200;
                    }
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.actSpeedNow -= 200;
                    }
                }
                break;
            case "TB6":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        if (now.activateUnit.jobNow == 5)
                        {
                            float damage = (now.activateUnit.magicNow - unit.magicNow) * now.activateUnit.magicNow;
                            damage = damage * (now.activateUnit.atkNow / unit.mDefNow);
                            if (damage > 0) unit.hpNow = unit.hpNow - damage * 100;
                            stage.DisplayHP(unit);
                            now.activateUnit.skillAct = 0;
                            now.activateUnit.skill.text = "0";
                        }
                    }
                }
                break;
            case "TB7":
                if (start)
                {
                    now.flagInt.Add(0);
                    now.flagInt.Add(0);
                    foreach (Unit unit in now.target)
                    {
                        if (unit.jobNow == 5)
                        {
                            now.activateUnit.powerNow += unit.magicNow;
                            now.flagInt[0] += unit.magicNow;
                            now.activateUnit.unitSize += 0.5f;
                            now.flagInt[1] += 1;
                            now.tacticsSecond += (int)Math.Floor(unit.skillAct);
                            unit.skillAct = 0;
                            unit.skill.text = "0";
                            unit.statusUp.gameObject.transform.localScale = unit.statusUp.gameObject.transform.localScale - new Vector3(4, 0, 0);
                        }
                    }
                    now.activateUnit.unitInfo.transform.localScale = new Vector3(now.activateUnit.unitSize, now.activateUnit.unitSize, 0);
                    now.activateUnit.tacticsAngle.transform.localScale = new Vector3(1 / now.activateUnit.unitSize, 1 / now.activateUnit.unitSize, 1);
                    now.activateUnit.chara.transform.localScale = new Vector3(1 / now.activateUnit.unitSize, 1 / now.activateUnit.unitSize, 1);
                    now.target = new List<Unit>();
                    now.target.Add(now.activateUnit);
                    TacticsInfoPlus();
                    now.activateUnit.power.text = now.activateUnit.powerNow.ToString();
                }
                else
                {
                    now.activateUnit.powerNow -= now.flagInt[0];
                    now.activateUnit.power.text = now.activateUnit.powerNow.ToString();
                    now.activateUnit.unitSize -= now.flagInt[1] * 0.5f;
                    now.activateUnit.unitInfo.transform.localScale = new Vector3(now.activateUnit.unitSize, now.activateUnit.unitSize, 0) * now.activateUnit.unitSize;
                    now.activateUnit.tacticsAngle.transform.localScale = new Vector3(1 / now.activateUnit.unitSize, 1 / now.activateUnit.unitSize, 1);
                    now.activateUnit.chara.transform.localScale = new Vector3(1 / now.activateUnit.unitSize, 1 / now.activateUnit.unitSize, 1);
                }
                break;
            case "TB8":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.jobArea.transform.localPosition += new Vector3(0, 40, 0);
                        unit.jobArea2.transform.localPosition += new Vector3(0, 40, 0);
                        unit.arrowTargetNum += 3;
                    }
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.jobArea.transform.localPosition -= new Vector3(0, 40, 0);
                        unit.jobArea2.transform.localPosition -= new Vector3(0, 40, 0);
                        unit.arrowTargetNum -= 3;
                    }
                }
                break;
            case "TB11":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        now.tacticsSecond -= unit.magicNow;
                    }
                    TacticsInfoPlus();
                }
                break;
            case "TB12":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.actTypeNow = "クラブアーム";
                        unit.battleThrough += 1;
                    }
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.actTypeNow = "";
                        unit.battleThrough -= 1;
                    }
                }
                break;
            case "TW1":
                if (start)
                {
                }
                else
                {
                    List<GameObject> unitLists;
                    if (!now.activateUnit.enemy) unitLists = stage.unitListEnemy;
                    else unitLists = stage.unitList;
                    foreach (GameObject obj in unitLists)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        for (int i = 0; i < now.flagInt.Count; i++)
                        {
                            if (unit.unitNum == now.flagInt[i])
                            {
                                unit.speedNow += now.flagInt2[i];
                                break;
                            }
                        }
                    }
                }
                break;
            case "TW2":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.unitSize += 1;
                        unit.unitInfo.transform.localScale = new Vector3(now.activateUnit.unitSize, now.activateUnit.unitSize, 0);
                        unit.tacticsAngle.transform.localScale = new Vector3(1/unit.unitSize, 1/unit.unitSize, 1);
                        unit.chara.transform.localScale = new Vector3(1 / unit.unitSize, 1 / unit.unitSize, 1);
                    }
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.unitSize -= 1;
                        unit.unitInfo.transform.localScale = new Vector3(now.activateUnit.unitSize, now.activateUnit.unitSize, 0) * unit.unitSize;
                        unit.tacticsAngle.transform.localScale = new Vector3(1 / unit.unitSize, 1 / unit.unitSize, 1);
                        unit.chara.transform.localScale = new Vector3(1 / unit.unitSize, 1 / unit.unitSize, 1);
                    }
                }
                break;
            case "TW3":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.stealth += 1;
                    }
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.stealth -= 1;
                    }
                }
                break;
            case "TW7":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.battleThrough += 1;
                        unit.defenderThrough += 1;
                    }
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.battleThrough -= 1;
                        unit.defenderThrough -= 1;
                    }
                }
                break;
            case "TW8":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        if (now.activateUnit.enemy != unit.enemy)
                        {
                            unit.pinMoveDisable += 1;
                            now.tacticsSecond += now.activateUnit.magicNow - unit.magicNow;
                            TacticsInfoPlus();
                        }
                    }
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        if (now.activateUnit.enemy != unit.enemy) unit.pinMoveDisable -= 1;
                    }
                }
                break;
            case "TW9":
                if (start)
                {
                    now.target = new List<Unit>();
                    Unit unit = stage.CardLoad("W-9");
                    now.target.Add(unit);
                    unit.pinMoveDisable++;
                    if (now.activateUnit.enemy)
                    {
                        stage.UnitEnemy(unit,false, false);
                    }
                    if (!unit.enemy)
                    {
                        stage.unitList.Add(unit.gameObject);
                        unit.unitInfo.transform.position = now.activateUnit.unitInfo.transform.position;
                        unit.punPlayer = 0;
                        unit.unitNum = stage.unitNum;
                        stage.unitNum++;
                    }
                    else
                    {
                        stage.unitListEnemy.Add(unit.gameObject);
                        unit.unitInfo.transform.position = now.activateUnit.unitInfo.transform.position;
                        unit.punPlayer = -1;
                        unit.unitNum = stage.unitNumEnemy;
                        stage.unitNumEnemy++;
                    }
                }
                else
                {
                    if(!now.target[0].enemy) stage.unitList.Remove(now.target[0].gameObject);
                    else stage.unitListEnemy.Remove(now.target[0].gameObject);
                    if (stage.pun)
                    {
                        PhotonNetwork.Destroy(now.target[0].gameObject);
                    }
                    else
                    {
                        Destroy(now.target[0].gameObject);
                    }
                }
                break;
            case "TW10":
                if (start)
                {
                    now.flagInt.Add(0);
                    List<GameObject> playerList;
                    if (!now.activateUnit.enemy) playerList = stage.unitList;
                    else playerList = stage.unitListEnemy;

                    foreach (GameObject obj in playerList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        if (unit.death)
                        {

                            now.activateUnit.powerNow += unit.cost;
                            now.activateUnit.power.text = now.activateUnit.powerNow.ToString();
                            now.activateUnit.magicNow += unit.cost;
                            now.activateUnit.magic.text = now.activateUnit.magicNow.ToString();
                            now.flagInt[0] += unit.cost;
                        }
                    }
                }
                else
                {
                    now.activateUnit.powerNow -= now.flagInt[0];
                    now.activateUnit.power.text = now.activateUnit.powerNow.ToString();
                    now.activateUnit.magicNow -= now.flagInt[0];
                    now.activateUnit.magic.text = now.activateUnit.magicNow.ToString();
                }
                break;
            case "TW11":
                if (start)
                {
                    now.flag.Add(false);
                    now.flag.Add(false);
                    now.flag.Add(false);
                    now.flag.Add(false);
                    now.flag.Add(false);
                    foreach (Unit unit in now.target)
                    {
                        if (unit.enemy == now.activateUnit.enemy && unit.unitNum == now.activateUnit.unitNum) continue;
                        switch (unit.card.color)
                        {
                            case "赤":
                                now.flag[0] = true;
                                break;
                            case "緑":
                                now.flag[1] = true;
                                break;
                            case "青":
                                now.flag[2] = true;
                                break;
                            case "白":
                                now.flag[3] = true;
                                break;
                            case "黒":
                                now.flag[4] = true;
                                break;
                        }
                    }
                    foreach (Unit unit in now.target)
                    {
                        if (unit.enemy == now.activateUnit.enemy && unit.unitNum == now.activateUnit.unitNum) continue;
                        if (now.flag[0])
                        {
                            UnitStatus3(unit, 0, 0, 3);
                        }
                        if (now.flag[1])
                        {
                            UnitStatus3(unit, 0, 4, 0);
                        }
                        if (now.flag[2])
                        {
                            UnitStatus3(unit, 2, 0, 0);
                        }
                        if (now.flag[3])
                        {
                            unit.defNow += 40;
                            unit.mDefNow += 40;
                        }
                        if (now.flag[4])
                        {
                            unit.atkNow += 20;
                            unit.speedNow += 20;
                        }
                    }
                    now.activateUnit.hpNow = 0;
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        if (unit.enemy == now.activateUnit.enemy && unit.unitNum == now.activateUnit.unitNum) continue;
                        if (now.flag[0])
                        {
                            UnitStatus3(unit, 0, 0, -3);
                        }
                        if (now.flag[1])
                        {
                            UnitStatus3(unit, 0, -4, 0);
                        }
                        if (now.flag[2])
                        {
                            UnitStatus3(unit, -2, 0, 0);
                        }
                        if (now.flag[3])
                        {
                            unit.defNow -= 40;
                            unit.mDefNow -= 40;
                        }
                        if (now.flag[4])
                        {
                            unit.atkNow -= 20;
                            unit.speedNow -= 20;
                        }
                    }
                }
                break;
            case "TW12":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.battleAct += 1;
                        unit.jobArea.transform.localPosition += new Vector3(0, 15, 0);
                    }
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.battleAct -= 1;
                        unit.jobArea.transform.localPosition -= new Vector3(0, 15, 0);
                    }
                }
                break;
            case "TP1":
                if (start)
                {
                    List<GameObject> unitLists;
                    if (!now.activateUnit.enemy) unitLists = stage.unitList;
                    else unitLists = stage.unitListEnemy;
                    foreach(GameObject obj in unitLists)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        if(unit.jobNow == 4 && unit.death)
                        {
                            unit.unitInfo.transform.localScale = new Vector3(unit.unitSize, unit.unitSize, 1);
                            unit.hpDeath.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 3);
                            StartCoroutine(unit.RevivalAnime());
                            unit.death = false;
                            unit.heal = false;
                            unit.hpNow = unit.reviveHpNow;
                            stage.DisplayHP(unit);
                            unit.unitInfo.transform.position = now.activateUnit.unitInfo.transform.position;
                            unit.powerNow += 5;
                            unit.power.text = unit.powerNow.ToString();
                            now.flagInt.Add(unit.unitNum);
                            break;
                        }
                    }
                }
                else
                {
                    if (now.flagInt.Count > 0)
                    {
                        List<GameObject> unitLists;
                        if (!now.activateUnit.enemy) unitLists = stage.unitList;
                        else unitLists = stage.unitListEnemy;
                        foreach (GameObject obj in unitLists)
                        {
                            Unit unit = obj.GetComponent<Unit>();
                            if (unit.unitNum == now.flagInt[0])
                            {
                                unit.powerNow -= 5;
                                unit.power.text = unit.powerNow.ToString();
                                if (!unit.death) yield return stage.UnitDeath(unit);
                            }
                        }
                    }
                }
                break;
            case "TP2":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        if (unit.jobNow == 5)
                        {
                            unit.skillAct = unit.skillNow;
                            unit.skill.text = unit.skillNow.ToString();
                        }
                    }
                }
                break;
            case "TP5":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        now.tacticsSecond += now.activateUnit.magicNow - unit.magicNow;
                    }
                    TacticsInfoPlus();
                }
                break;
            case "TP6":
                if (start)
                {
                    now.flagInt.Add(0);
                    List<GameObject> playerList;
                    if (!now.activateUnit.enemy) playerList = stage.unitList;
                    else playerList = stage.unitListEnemy;
                    foreach (GameObject obj in playerList)
                    {
                        bool b = false;
                        Unit unit = obj.GetComponent<Unit>();
                        foreach (Unit target in now.target)
                        {
                            if(target.unitNum == unit.unitNum)
                            {
                                b = true;
                                break;
                            }
                        }
                        if (!b)
                        {
                            now.flagInt[0] += unit.cost;
                            unit.hpNow = 0;
                            stage.DisplayHP(unit);
                        }
                    }


                    foreach (Unit unit in now.target)
                    {
                        unit.powerNow += now.flagInt[0];
                        unit.power.text = unit.powerNow.ToString();
                    }
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.powerNow -= now.flagInt[0];
                        unit.power.text = unit.powerNow.ToString();
                    }
                }
                break;
            case "TP7":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.hpNow -= 10000;
                    }
                }
                break;
            case "TP8":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        now.tacticsSecond += now.activateUnit.magicNow - unit.magicNow;
                        now.flagInt.Add(unit.powerNow - unit.powerNow/2);
                        unit.powerNow /= 2;
                        unit.power.text = unit.powerNow.ToString();
                        now.activateUnit.powerNow += now.flagInt[0];
                        now.activateUnit.power.text = now.activateUnit.powerNow.ToString();
                    }
                    TacticsInfoPlus();
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.powerNow += now.flagInt[0];
                        unit.power.text = unit.powerNow.ToString();
                        now.activateUnit.powerNow -= now.flagInt[0];
                        now.activateUnit.power.text = now.activateUnit.powerNow.ToString();
                    }
                }
                break;
            case "TP9":
                if (start)
                {
                    now.flag.Add(false);
                    now.flagInt.Add(0);
                }
                else
                {
                    foreach (Unit unit in now.target)
                    {
                        if (now.flag[0])
                        {
                            now.flag[0] = false;
                            now.flagInt[0] = 0;
                            unit.powerNow += 4;
                            unit.power.text = unit.powerNow.ToString();
                            unit.speedNow -= 50;
                            unit.actSpeedNow -= 200;
                        }
                    }
                }
                break;
            case "TP11":
                if (start)
                {
                    foreach (Unit unit in now.target)
                    {
                        unit.hpDeath.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 3);
                    }
                }
                else
                {
                }
                break;
            case "TP12":
                if (start)
                {
                        ChangeJob(now.activateUnit, now.activateUnit.jobDf, 4);
                }
                else
                {
                        ChangeJob(now.activateUnit, now.activateUnit.jobNow, now.activateUnit.jobDf);
                }
                break;
        }
    }
    public void TacticsUpdate()
    {
        foreach (TacticsNow now in tacticsNows)
        {
            List<Unit> regionTarget = new List<Unit>();
            List<Unit> regionIn = new List<Unit>();
            List<Unit> regionOut = new List<Unit>();
            if (now.activateUnit.card.tacticsRegion == "領域")
            {
                if (!now.activateUnit.heal)
                {
                    now.activateUnit.tacticsRegion.gameObject.SetActive(true);
                    List<GameObject> unitLists;
                    if (now.activateUnit.card.tacticsTarget == "敵全員" || now.activateUnit.card.tacticsTarget == "全員")
                    {
                        if (now.activateUnit.enemy) unitLists = stage.unitList;
                        else unitLists = stage.unitListEnemy;
                        foreach (GameObject gameObject in unitLists)
                        {
                            Unit unit = gameObject.GetComponent<Unit>();
                            if (!unit.heal && unit.stealth < 1)
                            {
                                if (now.activateUnit.card.tacticsAreaType == "円形")
                                {
                                    float distanse = (unit.unitInfo.transform.position - now.activateUnit.tacticsRegion.transform.position).magnitude;
                                    if (distanse < (now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta.x + Stage.Constants.unitSizeFix) / 2 - 1)
                                    {
                                        regionTarget.Add(unit);
                                    }
                                }
                                else if (now.activateUnit.card.tacticsAreaType == "四角")
                                {
                                    if (now.activateUnit.card.tacticsAreaSpin == "可能")
                                    {
                                        Quaternion inverse = Quaternion.Inverse(now.activateUnit.tacticsAngle.transform.rotation);
                                        Vector3 rotateTactics = now.activateUnit.tacticsRegion.transform.position - now.activateUnit.unitInfo.transform.position;
                                        rotateTactics = inverse * rotateTactics;
                                        float distanse = (unit.unitInfo.transform.position - now.activateUnit.tacticsRegion.transform.position).magnitude;
                                        float diagonal = (now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta).magnitude / 2;
                                        if (distanse < diagonal + Stage.Constants.unitSizeFix / 2)
                                        {
                                            Rect rect = new Rect(rotateTactics - new Vector3((now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta.x + Stage.Constants.unitSizeFix) / 2, (now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta.y + Stage.Constants.unitSizeFix) / 2, 0), now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta + new Vector2(Stage.Constants.unitSizeFix, Stage.Constants.unitSizeFix));
                                            Vector3 rotateUnit = unit.unitInfo.transform.position - now.activateUnit.unitInfo.transform.position;
                                            rotateUnit = inverse * rotateUnit;
                                            if (rect.Contains(rotateUnit))
                                            {
                                                regionTarget.Add(unit);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        float distanse = (unit.unitInfo.transform.position - now.activateUnit.tacticsRegion.transform.position).magnitude;
                                        float diagonal = (now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta).magnitude / 2;
                                        if (distanse < diagonal + Stage.Constants.unitSizeFix / 2)
                                        {
                                            Rect rect = new Rect(now.activateUnit.tacticsRegion.transform.position - new Vector3((now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta.x + Stage.Constants.unitSizeFix) / 2, (now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta.y + Stage.Constants.unitSizeFix) / 2, 0), now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta + new Vector2(Stage.Constants.unitSizeFix, Stage.Constants.unitSizeFix));
                                            if (rect.Contains(unit.unitInfo.transform.position))
                                            {
                                                regionTarget.Add(unit);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (now.activateUnit.card.tacticsTarget == "味方全員" || now.activateUnit.card.tacticsTarget == "全員")
                    {
                        if (now.activateUnit.enemy) unitLists = stage.unitListEnemy;
                        else unitLists = stage.unitList;
                        foreach (GameObject gameObject in unitLists)
                        {
                            Unit unit = gameObject.GetComponent<Unit>();
                            if (!unit.heal && unit.stealth < 1)
                            {
                                if (now.activateUnit.card.tacticsAreaType == "円形")
                                {
                                    float distanse = (unit.unitInfo.transform.position - now.activateUnit.tacticsRegion.transform.position).magnitude;
                                    if (distanse < (now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta.x + Stage.Constants.unitSizeFix) / 2 - 1)
                                    {
                                        regionTarget.Add(unit);
                                    }
                                }
                                else if (now.activateUnit.card.tacticsAreaType == "四角")
                                {
                                    if (now.activateUnit.card.tacticsAreaSpin == "可能")
                                    {
                                        Quaternion inverse = Quaternion.Inverse(now.activateUnit.tacticsAngle.transform.rotation);
                                        Vector3 rotateTactics = now.activateUnit.tacticsRegion.transform.position - now.activateUnit.unitInfo.transform.position;
                                        rotateTactics = inverse * rotateTactics;
                                        float distanse = (unit.unitInfo.transform.position - now.activateUnit.tacticsRegion.transform.position).magnitude;
                                        float diagonal = (now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta).magnitude / 2;
                                        if (distanse < diagonal + Stage.Constants.unitSizeFix / 2)
                                        {
                                            Rect rect = new Rect(rotateTactics - new Vector3((now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta.x + Stage.Constants.unitSizeFix) / 2, (now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta.y + Stage.Constants.unitSizeFix) / 2, 0), now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta + new Vector2(Stage.Constants.unitSizeFix, Stage.Constants.unitSizeFix));
                                            Vector3 rotateUnit = unit.unitInfo.transform.position - now.activateUnit.unitInfo.transform.position;
                                            rotateUnit = inverse * rotateUnit;
                                            if (rect.Contains(rotateUnit))
                                            {
                                                regionTarget.Add(unit);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        float distanse = (unit.unitInfo.transform.position - now.activateUnit.tacticsRegion.transform.position).magnitude;
                                        float diagonal = (now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta).magnitude / 2;
                                        if (distanse < diagonal + Stage.Constants.unitSizeFix / 2)
                                        {
                                            Rect rect = new Rect(now.activateUnit.tacticsRegion.transform.position - new Vector3((now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta.x + Stage.Constants.unitSizeFix) / 2, (now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta.y + Stage.Constants.unitSizeFix) / 2, 0), now.activateUnit.tacticsRegion.GetComponent<RectTransform>().sizeDelta + new Vector2(Stage.Constants.unitSizeFix, Stage.Constants.unitSizeFix));
                                            if (rect.Contains(unit.unitInfo.transform.position))
                                            {
                                                regionTarget.Add(unit);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    now.activateUnit.tacticsRegion.gameObject.SetActive(false);
                }
                foreach(Unit next in regionTarget)
                {
                    bool inUnit = true;
                    foreach(Unit prev in now.target)
                    {
                        if(next.enemy == prev.enemy && next.unitNum == prev.unitNum)
                        {
                            inUnit = false;
                            break;
                        }
                    }
                    if (inUnit)
                    {
                        regionIn.Add(next);

                    }
                }
                foreach (Unit prev in now.target)
                {
                    bool outUnit = true;
                    foreach (Unit next in regionTarget)
                    {
                        if (next.enemy == prev.enemy && next.unitNum == prev.unitNum)
                        {
                            outUnit = false;
                            break;
                        }
                    }
                    if (outUnit)
                    {
                        regionOut.Add(prev);
                    }
                }
                TacticsStatus(now, true, regionIn);
                TacticsStatus(now, false, regionOut);
                now.target = regionTarget;
            }
            switch (now.activateUnit.card.tacticsID)
            {
                case "TR3":
                    foreach (Unit unit in now.target)
                    {
                        if (unit.battleTarget.Count > 0 && !now.flag[0])
                        {
                            now.flag[0] = true;
                            unit.powerNow += 3;
                            unit.power.text = unit.powerNow.ToString();
                        }
                        if (unit.battleTarget.Count == 0 && now.flag[0])
                        {
                            now.flag[0] = false;
                            unit.powerNow -= 3;
                            unit.power.text = unit.powerNow.ToString();
                        }
                    }
                    break;
                case "TR4":
                    foreach (Unit unit in now.target)
                    {
                        if (unit.battleTarget.Count > 0 && !now.flag[0])
                        {
                            now.flag[0] = true;
                            unit.powerNow += 2;
                            unit.power.text = unit.powerNow.ToString();
                        }
                        if (unit.battleTarget.Count == 0 && now.flag[0])
                        {
                            now.flag[0] = false;
                            unit.powerNow -= 2;
                            unit.power.text = unit.powerNow.ToString();
                        }
                    }
                    break;
                case "TR5":
                    foreach (Unit unit in now.target)
                    {
                        if (unit.jobNow == 3)
                        {
                            foreach (Unit unitEnemy in unit.actTarget)
                            {
                                float damage = unit.magicNow*2 - unitEnemy.magicNow;
                                damage = damage * unit.atkNow / unitEnemy.mDefNow;
                                if(damage > 0)unitEnemy.hpNow -= damage * Stage.Constants.arrowDamage * stage.frameRate;
                            }
                        }
                    }
                    break;
                case "TR8":
                    foreach (Unit unit in now.target)
                    {
                        if (unit.breakSuccess)
                        {
                            if (unit.hpNow < unit.hpMaxNow)
                            {
                                unit.hpNow += 10 * 100;
                                if (unit.hpNow > unit.hpMaxNow) unit.hpNow = unit.hpMaxNow;
                                stage.DisplayHP(unit);
                            }
                            if (!unit.enemy) stage.hp.GetComponent<RectTransform>().sizeDelta = new Vector2(stage.hp.GetComponent<RectTransform>().sizeDelta.x + 2, 50);
                            else stage.hp2.GetComponent<RectTransform>().sizeDelta = new Vector2(stage.hp2.GetComponent<RectTransform>().sizeDelta.x + 2, 50);
                        }
                    }
                    break;
                case "TR9":
                    foreach (Unit unit in now.target)
                    {
                        if (unit.breakSuccess && now.flag.Count<4)
                        {
                            now.flag.Add(true);
                            if (!unit.enemy)
                            {
                                tacticsPointNow += 1;
                                if (tacticsPointNow >= 11) tacticsPointNow = 10.99f;
                            }
                            else
                            {
                                tacticsPointNowEnemy += 1;
                                if (tacticsPointNowEnemy >= 11) tacticsPointNowEnemy = 10.99f;
                            }
                        }
                    }
                    break;
                case "TR11":
                    for ( int i=0;i< now.target.Count;i++)
                    {
                        Unit unit = now.target[i];
                        if (unit.jobNow == 4 && !now.flag[i])
                        {
                            now.flag[i] = true;
                            unit.powerNow += 1;
                            unit.power.text = unit.powerNow.ToString();
                        }
                        if (unit.jobNow != 4 && now.flag[i])
                        {
                            now.flag[i] = false;
                            unit.powerNow -= 1;
                            unit.power.text = unit.powerNow.ToString();
                        }
                        if (unit.breakCheck && !now.flag2[i])
                        {
                            now.flag2[i] = true;
                            unit.powerNow += 1;
                            unit.power.text = unit.powerNow.ToString();
                        }
                        if (!unit.breakCheck && now.flag2[i])
                        {
                            now.flag2[i] = false;
                            unit.powerNow -= 1;
                            unit.power.text = unit.powerNow.ToString();
                        }
                        if (unit.battleTarget.Count > 0 && !now.flag3[i])
                        {
                            now.flag3[i] = true;
                            unit.powerNow += 1;
                            unit.power.text = unit.powerNow.ToString();
                        }
                        if (unit.battleTarget.Count == 0 && now.flag3[i])
                        {
                            now.flag3[i] = false;
                            unit.powerNow -= 1;
                            unit.power.text = unit.powerNow.ToString();
                        }
                    }
                    break;
                case "TR12":
                    foreach (Unit unit in now.target)
                    {
                        if (unit.jobNow == 1)
                        {
                            foreach (Unit unitEnemy in unit.actTarget)
                            {
                                float damage = 10;
                                damage *= (unit.atkNow / unitEnemy.defNow);
                                if (damage > 0) unitEnemy.hpNow -= damage * Stage.Constants.dashDamage;
                            }
                        }
                        unit.hpNow -= 1 * stage.frameRate;
                        if (unit.heal) now.tacticsSecond = 0;
                    }
                    break;
                case "TG7":
                    if (!now.activateUnit.enemy) stage.hp.GetComponent<RectTransform>().sizeDelta = new Vector2(stage.hp.GetComponent<RectTransform>().sizeDelta.x + 1f / 100 * stage.frameRate, 50);
                    else stage.hp2.GetComponent<RectTransform>().sizeDelta = new Vector2(stage.hp2.GetComponent<RectTransform>().sizeDelta.x + 1f / 100 * stage.frameRate, 50);

                    List<GameObject> enemyList;
                    if (!now.activateUnit.enemy) enemyList = stage.unitListEnemy;
                    else enemyList = stage.unitList;
                    foreach (GameObject obj in enemyList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        if (unit.breakSuccess)
                        {
                            unit.hpNow -= 200;
                        }
                    }
                    break;
                case "TB2":
                    {
                        List<GameObject> unitLists;
                        if (!now.activateUnit.enemy) unitLists = stage.unitList;
                        else unitLists = stage.unitListEnemy;
                        foreach (GameObject obj in unitLists)
                        {
                            Unit unit = obj.GetComponent<Unit>();
                            bool prev = false;
                            bool next = false;
                            int flagNum = 0;
                            for (int i = 0; i < now.flagInt.Count; i++)
                            {
                                if (unit.unitNum == now.flagInt[i])
                                {
                                    prev = true;
                                    flagNum = i;
                                    break;
                                }
                            }
                            foreach (Unit target in regionTarget)
                            {
                                if (unit.unitNum == target.unitNum && unit.enemy == target.enemy)
                                {
                                    next = true;
                                    break;
                                }
                            }
                            if (!prev && !next)
                            {
                                //なし
                            }
                            else if (!prev && next)
                            {
                                //新規
                                now.flagInt.Add(unit.unitNum);
                                now.flagInt2.Add(1 * stage.frameRate);
                            }
                            else if (prev && !next)
                            {
                                //抜け
                            }
                            else if (prev && next)
                            {
                                //継続
                                now.flagInt2[flagNum] += 1 * stage.frameRate;
                                if (now.flagInt2[flagNum] >= 100 * 30)
                                {
                                    now.flagInt2[flagNum] = 0;
                                    unit.magicNow += 1;
                                    unit.skillNow += 1;
                                    unit.magic.text = unit.magicNow.ToString();
                                }
                            }
                        }
                    }
                    break;
                case "TB3":
                    foreach (Unit unit in now.target)
                    {
                        if (unit.jobNow == 1)
                        {
                            foreach (Unit unitEnemy in unit.actTarget)
                            {
                                float damage = unit.magicNow*2 - unitEnemy.magicNow;
                                damage = damage * (unit.atkNow / unitEnemy.mDefNow);
                                if (damage > 0) unitEnemy.hpNow -= damage * Stage.Constants.dashDamage;
                            }
                        }
                    }
                    break;
                case "TB5":
                    foreach (Unit unit in now.target)
                    {
                        if (unit.jobNow == 2)
                        {
                            foreach (Unit unitEnemy in unit.actTarget)
                            {
                                float damage = 20;
                                damage *= (unit.atkNow / unitEnemy.defNow);
                                if (unit.actTarget.Count == 1)unitEnemy.hpNow -= damage * Stage.Constants.ranceDamage * stage.frameRate;
                            }
                        }
                    }
                    break;
                case "TB8":
                    foreach (Unit unit in now.target)
                    {
                        if (unit.jobNow == 3)
                        {
                            unit.jobAngle.transform.rotation = unit.jobDirection.transform.rotation;
                            foreach (Unit unitEnemy in unit.actTarget)
                            {
                                float damage = unit.magicNow - unitEnemy.magicNow/2f;
                                damage = damage * unit.atkNow / unitEnemy.mDefNow;
                                if (damage > 0) unitEnemy.hpNow = unitEnemy.hpNow - damage * Stage.Constants.arrowDamage * stage.frameRate;
                            }
                        }
                    }
                    break;
                case "TB12":
                    foreach (Unit unit in now.target)
                    {
                        if (unit.jobNow == 4 && unit.actCoolTime == 0)
                        {
                            foreach (Unit act in stage.actUnit)
                            {
                                if (unit.enemy == act.enemy && unit.unitNum == act.unitNum)
                                {
                                    if(now.flagUnit.Count == 0)
                                    {
                                        if ( unit.battleTarget.Count > 0)
                                        {
                                            unit.battleTarget.Sort((a, b) => b.card.cost - a.card.cost);
                                            now.flagUnit.Add(unit.battleTarget[0]);
                                            unit.actCoolTime += 0.3f;
                                        }
                                    }
                                    else
                                    {
                                        now.flagUnit = new List<Unit>();
                                        unit.actCoolTime += 0.3f;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    if(now.flagUnit.Count == 1)
                    {
                        if (now.flagUnit[0].heal)
                        {
                            now.flagUnit = new List<Unit>();
                        }
                        else
                        {
                            now.flagUnit[0].actCoolTime = 0.1f;
                            now.flagUnit[0].unitInfo.transform.position = now.activateUnit.unitInfo.transform.position + now.activateUnit.jobDirection.transform.rotation * new Vector3(0, 1, 0) * 10;
                        }
                    }
                    break;
                case "TW1":
                    {
                        List<GameObject> unitLists;
                        if (!now.activateUnit.enemy) unitLists = stage.unitListEnemy;
                        else unitLists = stage.unitList;
                        foreach (GameObject obj in unitLists)
                        {
                            Unit unit = obj.GetComponent<Unit>();
                            bool prev = false;
                            bool next = false;
                            int flagNum = 0;
                            for (int i = 0; i < now.flagInt.Count; i++)
                            {
                                if (unit.unitNum == now.flagInt[i])
                                {
                                    prev = true;
                                    flagNum = i;
                                    break;
                                }
                            }
                            foreach (Unit target in regionTarget)
                            {
                                if (unit.unitNum == target.unitNum && unit.enemy == target.enemy)
                                {
                                    next = true;
                                    break;
                                }
                            }
                            if (!prev && !next)
                            {
                                //なし
                            }
                            else if (!prev && next)
                            {
                                //新規
                                now.flagInt.Add(unit.unitNum);
                                now.flagInt2.Add(now.activateUnit.magicNow * 2 - unit.magicNow);
                                unit.speedNow -= now.activateUnit.magicNow * 2 - unit.magicNow;
                            }
                            else if (prev && !next)
                            {
                                //抜け
                                unit.speedNow += now.flagInt2[flagNum];
                                now.flagInt2[flagNum] = 0;
                            }
                            else if (prev && next)
                            {
                                //継続
                                unit.speedNow += now.flagInt2[flagNum];
                                now.flagInt2[flagNum] = now.activateUnit.magicNow * 2 - unit.magicNow;
                                unit.speedNow -= now.flagInt2[flagNum];
                            }
                        }
                        foreach (Unit target in regionTarget)
                        {
                            if (now.activateUnit.enemy == target.enemy)
                            {
                                if (target.hpNow < target.hpMaxNow)
                                {
                                    target.hpNow += 1 * stage.frameRate;
                                    if (target.hpNow > target.hpMaxNow) target.hpNow = target.hpMaxNow;
                                    stage.DisplayHP(target);
                                }
                            }
                        }
                    }
                    break;
                case "TW5":
                    foreach (Unit unit in now.target)
                    {
                        if (unit.jobNow == 1)
                        {
                            foreach (Unit unitEnemy in unit.actTarget)
                            {
                                if (unitEnemy.battleTarget.Count > 1)
                                {
                                    float damage = int.Parse(unit.skill.text) * 6;
                                    damage = damage * (unit.atkNow / unitEnemy.defNow);
                                    unitEnemy.hpNow = unitEnemy.hpNow - damage * Stage.Constants.dashDamage;
                                }
                            }
                        }
                    }
                    break;
                case "TW8":
                    if (now.target.Count == 2)
                    {
                        Unit player =null;
                        Unit enemy = null;
                        foreach (Unit unit in now.target)
                        {
                            if (now.activateUnit.enemy == unit.enemy) player = unit;
                            if (now.activateUnit.enemy != unit.enemy) enemy = unit;
                        }
                        if (player.heal)
                        {
                            now.tacticsSecond = 0;
                        }
                        enemy.ForcePinMove(player.unitInfo.transform.position);
                    }
                    break;
                case "TW9":
                    if (now.target[0].death) now.target[0].pin.transform.position = now.target[0].unitInfo.transform.position;
                    if (!now.target[0].death) now.target[0].pin.transform.position = now.activateUnit.unitInfo.transform.position + now.activateUnit.jobDirection.transform.rotation * new Vector3(0, 1, 0) * 15;
                    if (!now.target[0].death && now.activateUnit.heal) now.target[0].pin.transform.position = now.activateUnit.unitInfo.transform.position;
                    if (!now.activateUnit.enemy && now.target[0].pin.transform.position.y > 55) now.target[0].pin.transform.position = new Vector3(now.target[0].pin.transform.position.x, 55, 0);
                    if (now.activateUnit.enemy && now.target[0].pin.transform.position.y < -55) now.target[0].pin.transform.position = new Vector3(now.target[0].pin.transform.position.x, -55, 0);
                    break;
                case "TW12":
                    foreach (Unit unit in now.target)
                    {
                        if (unit.jobNow == 2)
                        {
                            unit.jobAngle.transform.Rotate(new Vector3(0, 0, stage.frameRate*2));
                        }
                    }
                    break;
                case "TP4":
                    foreach (Unit unit in now.target)
                    {
                        if (unit.jobNow == 1)
                        {
                            foreach (Unit unitEnemy in unit.battleTarget)
                            {
                                float damage = unitEnemy.cost * 2.5f;
                                damage = damage * (unit.atkNow / unitEnemy.defNow);
                                unitEnemy.hpNow = unitEnemy.hpNow - damage * Stage.Constants.battleDamage * stage.frameRate;
                            }
                        }
                    }
                    break;
                case "TP9":
                    foreach (Unit unit in now.target)
                    {
                        if (unit.jobNow == 1)
                        {
                            if (now.flagInt[0] > 0) now.flagInt[0] -= stage.frameRate;
                            if (unit.actTarget.Count > 0)
                            {
                                now.flagInt[0] = 300;
                            }
                            if(!now.flag[0] && now.flagInt[0] > 0)
                            {
                                now.flag[0] = true;
                                unit.powerNow -= 4;
                                unit.power.text = unit.powerNow.ToString();
                                unit.speedNow += 50;
                                unit.actSpeedNow += 200;
                            }
                            if (now.flag[0] && now.flagInt[0] == 0)
                            {
                                now.flag[0] = false;
                                unit.powerNow += 4;
                                unit.power.text = unit.powerNow.ToString();
                                unit.speedNow -= 50;
                                unit.actSpeedNow -= 200;
                            }
                        }
                        else
                        {
                            if (now.flag[0])
                            {
                                now.flag[0] = false;
                                now.flagInt[0] = 0;
                                unit.powerNow += 4;
                                unit.power.text = unit.powerNow.ToString();
                                unit.speedNow -= 50;
                                unit.actSpeedNow -= 200;
                            }
                        }
                    }
                    break;
                case "TP10":
                    foreach (Unit unit in regionTarget)
                    {
                        float damage = (now.activateUnit.magicNow - unit.magicNow / 2f) * stage.frameRate;
                        if(damage>0)unit.hpNow -= damage;
                    }
                    break;
            }
        }
    }
    public void Lock()
    {
        SEManager.Instance.Play(SEPath.LOCK);
        onLock.SetActive(true);
        offLock.SetActive(false);
    }
    public void UnLock()
    {
        SEManager.Instance.Play(SEPath.UNLOCK);
        onLock.SetActive(false);
        offLock.SetActive(true);
    }
    public void Activate()
    {
        if (!stage.pun)
        {
            if (!selectUnit.enemy)
            {
                Lock();
                tacticsButton.interactable = false;
                activateUnit = selectUnit;
            }
            else if (stage.debug)
            {
                tacticsTargetEnemy = tacticsTarget;
                activateUnitEnemy = selectUnit;
            }
            
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (!selectUnit.enemy)
                {
                    Lock();
                    tacticsButton.interactable = false;
                    activateUnit = selectUnit;
                }
                else if (stage.debug)
                {
                    tacticsTargetEnemy = tacticsTarget;
                    activateUnitEnemy = selectUnit;
                }
            }
            else
            {
                punManager.PunTacticsActivate(selectUnit, tacticsTarget);
            }
        }
    }
    public void PunEnemyActivate(int unitNum, int[] targetNum, bool[] targetEnemy)
    {
        Unit unit = stage.unitListEnemy.Find(u => u.GetComponent<Unit>().unitNum == unitNum).GetComponent<Unit>();
        tacticsTargetEnemy = new List<Unit>();
        for (int i=0;i<targetNum.Length;i++)
        {
            if(!targetEnemy[i])tacticsTargetEnemy.Add(stage.unitList.Find(u => u.GetComponent<Unit>().unitNum == targetNum[i]).GetComponent<Unit>());
            else tacticsTargetEnemy.Add(stage.unitListEnemy.Find(u => u.GetComponent<Unit>().unitNum == targetNum[i]).GetComponent<Unit>());
        }
        activateUnitEnemy = unit;
    }
    public void TacticsChange(Unit unit)
    {
        if (offLock.activeSelf)
        {
            tacticsButton.interactable = false;
            selectUnit = unit;
            TacticsPossibleButton();
        }
    }
    public IEnumerator TacticsCutin(Unit unit,bool start,bool end)
    {
        Card card = unit.card;
        if (start)
        {
            if (stage.pun && PhotonNetwork.IsMasterClient) punManager.TacticsCutin(unit);
            cutinName.text = card.tacticsName;
            cutinTime.text = card.tacticsTime.ToString();
            cutinText.text = card.tacticsText;
            switch (card.color)
            {
                case "赤":
                    cutinName.color = new Color(1, 0, 0);
                    break;
                case "緑":
                    cutinName.color = new Color(0, 1, 0);
                    break;
                case "青":
                    cutinName.color = new Color(0, 0, 1);
                    break;
                case "白":
                    cutinName.color = new Color(1, 1, 0);
                    break;
                case "黒":
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
            cutinBack.GetComponent<Image>().enabled=true;
            unit.chara.gameObject.SetActive(true);
            float distanse = (unit.unitInfo.transform.position).magnitude;
            for (int i = 0; i < 10; i++)
            {
                unit.chara.transform.position = Vector3.MoveTowards(unit.chara.transform.position, new Vector3(0, 0, 0), distanse / 10);
                unit.chara.GetComponent<RectTransform>().sizeDelta = unit.chara.GetComponent<RectTransform>().sizeDelta + new Vector2(10, 10);
                unit.chara.color = unit.chara.color + new Color(0, 0, 0, 0.05f);
                yield return new WaitForSecondsRealtime(0.02f);
            }
            tacticsGroup.SetActive(true);
            BGMManager.Instance.ChangeBaseVolume(0.2f);
            SEManager.Instance.Play(SEPath.TACTICSSTART);
            yield return tacticsFade.FadeoutCoroutine(5f, null);
        }
        if(end)
        {
            float distanse = (unit.unitInfo.transform.position- unit.chara.transform.position).magnitude;
            yield return new WaitForSecondsRealtime(1f);
            SEManager.Instance.Play(SEPath.TACTICSEND, delay: 3);
            yield return tacticsFade.FadeinCoroutine(5f, null);
            BGMManager.Instance.ChangeBaseVolume(0.4f);
            tacticsGroup.SetActive(false);
            for (int i = 0; i < 10; i++)
            {
                unit.chara.transform.position = Vector3.MoveTowards(unit.chara.transform.position, unit.unitInfo.transform.position, distanse / 10);
                unit.chara.GetComponent<RectTransform>().sizeDelta = unit.chara.GetComponent<RectTransform>().sizeDelta + new Vector2(-10, -10);
                unit.chara.color = unit.chara.color + new Color(0, 0, 0, -0.05f);
                yield return new WaitForSecondsRealtime(0.02f);
            }
            unit.chara.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
            unit.chara.color = new Color(1, 1, 1, 0.4f);
            unit.chara.transform.position = unit.unitInfo.transform.position;
            if (!unit.select) unit.chara.gameObject.SetActive(false);
            cutinBack.GetComponent<Image>().enabled = false;

            if (stage.pun)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    while (punManager.cutinSync == -1)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                else
                {
                    punManager.CutinSync();
                }
            }
        }
    }
    public IEnumerator PunPossible()
    {
        while (true)
        {
            TacticsPossibleButton();
            yield return new WaitForSecondsRealtime(0.05f);
        }
    }
    void ChangeJob(Unit unit,int prev,int next)
    {
        if (unit.jobDf != unit.jobNow && unit.jobDf != next) return;
        float prevSpeed = 0;
        float nextSpeed = 0;
        float prevBreak = 0;
        float nextBreak = 0;
        switch (prev) 
        {
            case 1:
                prevSpeed = 100;
                prevBreak = 80;
                break;
            case 2:
                prevSpeed = 75;
                prevBreak = 180;
                break;
            case 3:
                prevSpeed = 90;
                prevBreak = 130;
                break;
            case 4:
                prevSpeed = 90;
                prevBreak = 180;
                break;
            case 5:
                prevSpeed = 80;
                prevBreak = 150;
                break;
        }
        switch (next)
        {
            case 1:
                nextSpeed = 100;
                nextBreak = 80;
                unit.skillAct = 0;
                unit.skill.text = "0";
                break;
            case 2:
                nextSpeed = 75;
                nextBreak = 180;
                unit.skillAct = 0;
                unit.skill.text = unit.skillNow.ToString();
                break;
            case 3:
                nextSpeed = 90;
                nextBreak = 130;
                unit.skillAct = unit.skillNow + 0.99f;
                unit.skill.text = unit.skillNow.ToString();
                break;
            case 4:
                nextSpeed = 90;
                nextBreak = 180;
                unit.skillAct = 0;
                unit.skill.text = unit.skillNow.ToString();
                break;
            case 5:
                nextSpeed = 80;
                nextBreak = 150;
                unit.skillAct = unit.skillNow / 2f;
                unit.skill.text = Math.Floor(unit.skillAct).ToString();
                break;
        }
        unit.speedNow += nextSpeed - prevSpeed;
        unit.breakNow += nextBreak - prevBreak;
        unit.jobNow = next;
        unit.jobDirection.GetComponent<Image>().sprite = stage.jobImage[next];
        for(int i=0;i< unit.jobDirection.transform.childCount; i++)
        {
            unit.jobDirection.transform.GetChild(i).gameObject.GetComponent<Image>().sprite = stage.jobImage[next];
        }
    }
    void TargetAll(List<GameObject> unitLists)
    {
        foreach (GameObject gameObject in unitLists)
        {
            Unit unit = gameObject.GetComponent<Unit>();
            if (!unit.heal)
            {
                if (selectUnit.card.tacticsAreaType == "戦場")
                {
                    if (!unit.heal)
                    {
                        tacticsTarget.Add(unit);
                    }
                }
                if (selectUnit.card.tacticsAreaType == "円形")
                {
                    float distanse = (unit.unitInfo.transform.position - selectUnit.tacticsArea.transform.position).magnitude;
                    if (distanse < (selectUnit.tacticsArea.GetComponent<RectTransform>().sizeDelta.x + Stage.Constants.unitSizeFix) / 2 - 1)
                    {
                        tacticsTarget.Add(unit);
                    }
                }
                else if (selectUnit.card.tacticsAreaType == "四角")
                {
                    if (selectUnit.card.tacticsAreaSpin == "可能")
                    {
                        Quaternion inverse = Quaternion.Inverse(selectUnit.tacticsAngle.transform.rotation);
                        Vector3 rotateTactics = selectUnit.tacticsArea.transform.position - selectUnit.unitInfo.transform.position;
                        rotateTactics = inverse * rotateTactics;
                        float distanse = (unit.unitInfo.transform.position - selectUnit.tacticsArea.transform.position).magnitude;
                        float diagonal = (selectUnit.tacticsArea.GetComponent<RectTransform>().sizeDelta).magnitude / 2;
                        if (distanse < diagonal + Stage.Constants.unitSizeFix / 2)
                        {
                            Rect rect = new Rect(rotateTactics - new Vector3((selectUnit.tacticsArea.GetComponent<RectTransform>().sizeDelta.x + Stage.Constants.unitSizeFix) / 2, (selectUnit.tacticsArea.GetComponent<RectTransform>().sizeDelta.y + Stage.Constants.unitSizeFix) / 2, 0), selectUnit.tacticsArea.GetComponent<RectTransform>().sizeDelta + new Vector2(Stage.Constants.unitSizeFix, Stage.Constants.unitSizeFix));
                            Vector3 rotateUnit = unit.unitInfo.transform.position - selectUnit.unitInfo.transform.position;
                            rotateUnit = inverse * rotateUnit;
                            if (rect.Contains(rotateUnit))
                            {
                                tacticsTarget.Add(unit);
                            }
                        }
                    }
                    else
                    {
                        float distanse = (unit.unitInfo.transform.position - selectUnit.tacticsArea.transform.position).magnitude;
                        float diagonal = (selectUnit.tacticsArea.GetComponent<RectTransform>().sizeDelta).magnitude / 2;
                        if (distanse < diagonal + Stage.Constants.unitSizeFix / 2)
                        {
                            Rect rect = new Rect(selectUnit.tacticsArea.transform.position - new Vector3((selectUnit.tacticsArea.GetComponent<RectTransform>().sizeDelta.x + Stage.Constants.unitSizeFix) / 2, (selectUnit.tacticsArea.GetComponent<RectTransform>().sizeDelta.y + Stage.Constants.unitSizeFix) / 2, 0), selectUnit.tacticsArea.GetComponent<RectTransform>().sizeDelta + new Vector2(Stage.Constants.unitSizeFix, Stage.Constants.unitSizeFix));
                            if (rect.Contains(unit.unitInfo.transform.position))
                            {
                                tacticsTarget.Add(unit);
                            }
                        }
                    }
                }
            }
        }
    }
    void UnitStatus3(Unit unit,int power,int magic,int skill)
    {
        unit.powerNow += power;
        unit.magicNow += magic;
        unit.skillNow += skill;
        unit.power.text = unit.powerNow.ToString();
        unit.magic.text = unit.magicNow.ToString();
        if (unit.jobNow == 1)
        {
            if (unit.skillAct > unit.skillNow) unit.skillAct = unit.skillNow;
            unit.skill.text = Math.Floor(unit.skillAct).ToString();
        }
        else if (unit.jobNow == 3)
        {
            unit.skillAct += skill;
            unit.skill.text = Math.Floor(unit.skillAct).ToString();
        }
        else if (unit.jobNow == 5)
        {
            unit.skillAct += skill / 2f;
            if (unit.skillAct > unit.skillNow) unit.skillAct = unit.skillNow;
            unit.skill.text = Math.Floor(unit.skillAct).ToString();
        }
        else
        {
            unit.skill.text = unit.skillNow.ToString();
        }
    }
}




public class TacticsNow
{
    public List<bool> flag = new List<bool>();
    public List<bool> flag2 = new List<bool>();
    public List<bool> flag3 = new List<bool>();
    public List<int> flagInt = new List<int>();
    public List<int> flagInt2 = new List<int>();
    public List<Unit> flagUnit = new List<Unit>();
    public Sprite sprite;
    public int actvateTimes;
    public Unit activateUnit;
    public float tacticsSecond;
    public List<Unit> target = new List<Unit>();
}