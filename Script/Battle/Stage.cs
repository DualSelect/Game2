using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using KanKikuchi.AudioManager;
using Photon.Pun;
using Photon.Realtime;

public class Stage : MonoBehaviourPunCallbacks
{
    public string[] deck;
    public string[] deckEnemy;
    public List<GameObject> unitList;
    public List<GameObject> unitListEnemy;
    public List<GameObject> playerList;
    List<GameObject> crystalList = new List<GameObject>();
    public GameObject hp;
    public GameObject hp2;
    public GameObject crystalPrefab;
    public GameObject[] jobPrefab;
    public GameObject unitListObject;
    public GameObject effectListObject;
    public CardMaster cardMaster;
    public Animator deathAnime;
    public GameObject cutinBack;
    public Text time;
    public Tactics tactics;
    public Commander commander;
    public GameObject mainCamera;
    public PunAudio punAudio;
    public GameObject[] playerStage;
    public GameObject[] enemyStage;
    public List<Unit> actUnit = new List<Unit>();
    List<ActObject> actList = new List<ActObject>();
    public GameObject[] effectAnime;
    public Sprite[] shape;
    public Sprite[] jobImage;
    public string key;
    PunManager punManager;
    public string room;



    public int unitNum = 0;
    public int unitNumEnemy = 0;
    int frame = 0;
    public int frameRate = 2;
    public int cpuLevel;
    public int cpuRandom;
    public bool pun;
    public bool cpu;
    public bool debug;

    bool match = false;
    public bool start = false;

    public static class Constants
    {
        public const float moveSpeed = 0.75f;
        public const float unitSizeFix = 14;
        public const float battleDamage = 0.8f;
        public const float dashDamage = 100;
        public const float ranceDamage = 0.45f;
        public const float arrowDamage = 0.6f;
        public const float magicDamage = 100;
    }

    public void GameStart()
    {
        playerList = unitList;
        if (pun)
        {
            PhotonNetwork.SendRate = 10;
            PhotonNetwork.SerializationRate = 10;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            StartCoroutine(Prepare());
        }
    }
    public override void OnConnectedToMaster()
    {
        // "Room"という名前のルームに参加する（ルームが存在しなければ作成して参加する）
        PhotonNetwork.JoinOrCreateRoom(room, new RoomOptions(), TypedLobby.Default);
    }
    public override void OnJoinedRoom()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            foreach (GameObject gameObject in playerStage)
            {
                gameObject.SetActive(false);
            }
            foreach (GameObject gameObject in enemyStage)
            {
                gameObject.SetActive(true);
            }
            playerList = unitListEnemy;
            mainCamera.transform.position = new Vector3(-20, 12, -55);
            mainCamera.transform.rotation = Quaternion.Euler(6, 0, 180);
            StartCoroutine(tactics.PunPossible());
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && match == false)
        {
            match = true;
            punAudio = PhotonNetwork.Instantiate("PunAudio", new Vector3(0, 0, 0), Quaternion.identity).GetComponent<PunAudio>();
            punManager = PhotonNetwork.Instantiate("PunManager", new Vector3(0, 0, 0), Quaternion.identity).GetComponent<PunManager>();
        }
    }
    public IEnumerator Prepare()
    {
        if (debug) playerStage[3].SetActive(false);
        
        //if (pun) BGM(BGMPath.PERITUNE_MATERIAL_EPIC_BATTLE_LOOP, 0.8f);
        //BGMManager.Instance.Play(BGMPath.PERITUNE_MATERIAL_EPIC_BATTLE_LOOP, 0.8f);

        bool[] colorBool = new bool[] { false, false, false, false, false };
        for (int i = 0; i < deck.Length; i++)
        {
            if (deck[i] == "") continue;
            Unit unit = CardLoad(deck[i]);
            unitList.Add(unit.gameObject);
            unit.unitInfo.transform.position = new Vector3(-50 + i * 15, -61.5f, 0);
            unit.pin.transform.position = unit.unitInfo.transform.position;
            unit.unitInfo.GetComponent<Image>().raycastTarget = true;
            unit.unitInfo.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
            unit.actAngle.SetActive(true);
            unit.punPlayer = 0;
            unit.unitNum = unitNum;
            unitNum++;
            switch (unit.card.color)
            {
                case "赤":
                    colorBool[0] = true;
                    break;
                case "緑":
                    colorBool[1] = true;
                    break;
                case "青":
                    colorBool[2] = true;
                    break;
                case "白":
                    colorBool[3] = true;
                    break;
                case "黒":
                    colorBool[4] = true;
                    break;
            }
        }
        foreach(bool color in colorBool)
        {
            if (color) tactics.tacticsSpeedNow -= 0.08f;
        }
        if (!cpu)
        {
            colorBool = new bool[] { false, false, false, false, false };
            for (int i = 0; i < deckEnemy.Length; i++)
            {
                if (deckEnemy[i] == "") continue;
                Unit unit = CardLoad(deckEnemy[i]);
                UnitEnemy(unit, false, false);
                unitListEnemy.Add(unit.gameObject);
                unit.unitInfo.transform.position = new Vector3(50 + i * -15, 61.5f, 0);

                unit.pin.transform.position = unit.unitInfo.transform.position;
                if (debug) unit.unitInfo.GetComponent<Image>().raycastTarget = true;
                if (debug) unit.actAngle.SetActive(true);
                if (!debug) unit.unitInfo.SetActive(false);
                unit.punPlayer = 1;
                unit.unitNum = unitNumEnemy;
                unitNumEnemy++;
                switch (unit.card.color)
                {
                    case "赤":
                        colorBool[0] = true;
                        break;
                    case "緑":
                        colorBool[1] = true;
                        break;
                    case "青":
                        colorBool[2] = true;
                        break;
                    case "白":
                        colorBool[3] = true;
                        break;
                    case "黒":
                        colorBool[4] = true;
                        break;
                }
            }
            foreach (bool color in colorBool)
            {
                if (color) tactics.tacticsSpeedNowEnemy -= 0.08f;
            }
        }
        commander.CommandInitial();
        time.text = "20";
        while (int.Parse(time.text) > 0)
        {
            frame++;
            for (int a = 0; a < 2; a++)
            {
                List<GameObject> unitLists = null;
                if (a == 0) unitLists = unitList;
                if (a == 1) unitLists = unitListEnemy;
                for (int i = 0; unitLists.Count > i; i++)
                {
                    Unit unit = unitLists[i].GetComponent<Unit>();
                    if (!unit.enemy && unit.pin.transform.position.y > -10) unit.pin.transform.position = new Vector3(unit.pin.transform.position.x, -10, 0);
                    if (unit.enemy && unit.pin.transform.position.y < 10) unit.pin.transform.position = new Vector3(unit.pin.transform.position.x, 10, 0);
                    unit.unitInfo.transform.position = unit.pin.transform.position;
                    unit.root.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                }
            }
            if (frame % (100 / frameRate) == 0) time.text = (int.Parse(time.text) - 1).ToString();
            yield return new WaitForSecondsRealtime(0.01f * frameRate);
        }
        playerStage[3].SetActive(false);
        if (pun) punManager.BattleStart();
        tactics.buttonStop.SetActive(false);
        foreach(GameObject obj in unitListEnemy)
        {
            Unit unit = obj.GetComponent<Unit>();
            if(unit.unitInfo.transform.position.y < 61.5) unit.unitInfo.SetActive(true);
        }
        StartCoroutine(RealTime001());
    }
    IEnumerator RealTime001()
    {
        start = true;
        frame = 0;
        time.text = "250";
        while (true)
        {
            UnitActCheack();
            if (tactics.activateUnit != null) yield return tactics.TacticsStart(tactics.activateUnit);
            if (tactics.activateUnitEnemy != null) yield return tactics.TacticsStart(tactics.activateUnitEnemy);
            if (commander.activate > 0) yield return commander.CommandEffect(true, false);
            if (commander.activateEnemy > 0) yield return commander.CommandEffect(true, true);
            tactics.TacticsUpdate();

            //HP増減にかかわる処理
            for (int a = 0; a < 2; a++)
            {
                List<GameObject> unitLists = null;
                if (a == 0) unitLists = unitList;
                if (a == 1) unitLists = unitListEnemy;
                for (int i = 0; unitLists.Count > i; i++)
                {
                    Unit unit = unitLists[i].GetComponent<Unit>();
                    //城内判定
                    if (unit.heal)
                    {
                        if (unit.death)
                        {
                            unit.hpNow += 1 * frameRate;
                            unit.hpDeath.GetComponent<RectTransform>().sizeDelta = new Vector2(-unit.hpNow / 100, 3);
                            if (unit.hpNow >= 0)
                            {
                                StartCoroutine(unit.RevivalAnime());
                                unit.death = false;
                                unit.hpNow = unit.reviveHpNow;
                                DisplayHP(unit);
                            }
                        }
                        else
                        {
                            if (unit.hpNow < unit.hpMaxNow)
                            {
                                unit.hpNow += unit.healSpeedNow * frameRate;
                                if (unit.hpNow > unit.hpMaxNow)
                                {
                                    StartCoroutine(unit.HealAnime());
                                    unit.hpNow = unit.hpMaxNow;
                                }
                                DisplayHP(unit);
                            }
                            if (unit.jobNow == 3)
                            {
                                unit.skillAct = unit.skillNow + 0.9f;
                                unit.skill.text = Math.Floor(unit.skillAct).ToString();
                            }
                        }
                    }
                    //戦闘判定
                    else
                    {
                        foreach (Unit unitEnemy in unit.battleTarget)
                        {
                            //近接
                            {
                                unitEnemy.breakSpeedPer = 0;
                                if (unit.jobNow == 5)
                                {
                                    float damage = (7 + unit.magicNow - unitEnemy.magicNow) * 1;
                                    if (damage < 2) damage = 2;
                                    damage *= (unit.atkNow / unitEnemy.mDefNow);
                                    unitEnemy.hpNow -= damage * Constants.battleDamage * frameRate;
                                }
                                else
                                {
                                    float damage = (7 + unit.powerNow - unitEnemy.powerNow) * 1.8f;
                                    if (damage < 2) damage = 2;
                                    damage *= (unit.atkNow / unitEnemy.defNow);
                                    unitEnemy.hpNow -= damage * Constants.battleDamage * frameRate;
                                }

                                if (unit.jobNow == 4)
                                {
                                    unitEnemy.breakGauge.GetComponent<RectTransform>().sizeDelta = new Vector2(unitEnemy.breakGauge.GetComponent<RectTransform>().sizeDelta.x - 0.1f * unit.skillAct * 0.02f, 3);
                                    if (unitEnemy.breakGauge.GetComponent<RectTransform>().sizeDelta.x < 0) unitEnemy.breakGauge.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 3);
                                }
                            }
                            //速度
                            {
                                if (unit.battleThrough == 0)
                                {
                                    float distanse = (unit.unitInfo.transform.position - unitEnemy.unitInfo.transform.position).magnitude;
                                    float moveV = Mathf.Atan2(unit.unitInfo.transform.position.x - unit.pin.transform.position.x, unit.unitInfo.transform.position.y - unit.pin.transform.position.y) * Mathf.Rad2Deg;
                                    float enemyV = Mathf.Atan2(unit.unitInfo.transform.position.x - unitEnemy.unitInfo.transform.position.x, unit.unitInfo.transform.position.y - unitEnemy.unitInfo.transform.position.y) * Mathf.Rad2Deg;
                                    float angle = moveV - enemyV;
                                    if (angle < 0) angle = angle + 360;
                                    if (angle >= 180) angle = 360 - angle;
                                    unit.speedUpFix -= 5 + (180 - angle) / 180 * 25 + (Constants.unitSizeFix * (unit.unitSize + unitEnemy.unitSize) / 2 - distanse) / Constants.unitSizeFix * (unit.unitSize + unitEnemy.unitSize) / 2 * 25;
                                }
                            }
                        }
                        foreach (Unit unitEnemy in unit.actTarget)
                        {
                            //アタッカー
                            if (unit.jobNow == 1)
                            {
                                float damage = 7 + int.Parse(unit.skill.text) * 3 + unit.powerNow * (1.8f + int.Parse(unit.skill.text) * 0.1f) - unitEnemy.powerNow * 2;
                                if (damage < 1.5) damage = 1.5f;
                                damage *= (unit.atkNow / unitEnemy.defNow);
                                unitEnemy.hpNow -= damage * Constants.dashDamage;
                                StartCoroutine(unit.AttackAnime((unit.unitInfo.transform.position + unitEnemy.unitInfo.transform.position) / 2));
                            }
                            //ディフェンダー
                            if (unit.jobNow == 2)
                            {
                                unitEnemy.breakSpeedPer *= 0.7f;
                                float damage = 13 + unit.powerNow * 1.1f + unit.skillNow * 0.7f - unitEnemy.powerNow * 1.8f;
                                if (damage < 1) damage = 1;
                                damage *= (unit.atkNow / unitEnemy.defNow);
                                unitEnemy.hpNow -= damage * Constants.ranceDamage * frameRate;
                                if (unitEnemy.defenderThrough == 0)
                                {
                                    if (unitEnemy.jobNow == 1)
                                    {
                                        unitEnemy.speedUpFix -= 17 + unit.skillNow * 3;
                                    }
                                    else
                                    {
                                        unitEnemy.speedUpFix -= unit.skillNow * 3;
                                    }
                                }
                            }
                            //シューター
                            if (unit.jobNow == 3)
                            {
                                unitEnemy.breakSpeedPer *= 0.4f;
                                float damage = 13 + int.Parse(unit.skill.text) * 0.7f + unit.powerNow * 1.1f - unitEnemy.powerNow * 1.8f;
                                if (unitEnemy.jobNow == 1) damage *= (1 - int.Parse(unit.skill.text) * 0.05f - unit.skillNow * 0.03f);
                                if (damage < 2) damage = 2;
                                damage *= (unit.atkNow / unitEnemy.defNow);
                                unitEnemy.hpNow -= damage * Constants.arrowDamage * frameRate;
                                unit.skillAct -= 0.0015f * frameRate;
                                unit.skill.text = Math.Floor(unit.skillAct).ToString();

                            }
                        }
                    }
                }
            }
            for (int i = actList.Count - 1; i >= 0; i--)
            {
                List<GameObject> unitListsEnemy;
                ActObject act = actList[i];
                act.stopTime -= 0.01f * frameRate;
                if (act.stopTime < 0) act.stopTime = 0;
                if (act.stopTime == 0)
                {
                    if (act.type < 11)
                    {
                        act.gameObject.transform.position = Vector3.MoveTowards(act.gameObject.transform.position, act.targetPoint, act.speed / 100 * Constants.moveSpeed * 0.1f * frameRate);
                        if (act.gameObject.transform.position == act.targetPoint)
                        {
                            actList.RemoveAt(i);
                            if (!pun) Destroy(act.gameObject);
                            else PhotonNetwork.Destroy(act.gameObject);
                            if (act.type == 3)
                            {
                                GameObject effect;
                                if (!pun)
                                {
                                    effect = Instantiate(effectAnime[6], effectListObject.transform) as GameObject;
                                    effect.transform.position = act.gameObject.transform.position;
                                    effect.transform.localScale = new Vector3(15 + 3 * act.lebel, 15 + 3 * act.lebel, 1);
                                    effect.GetComponent<EffectAnime>().EffectStart(false);
                                }
                                else
                                {
                                    effect = PhotonNetwork.Instantiate("爆発", new Vector3(999, 999, 0), Quaternion.identity);
                                    effect.transform.position = act.gameObject.transform.position;
                                    effect.transform.localScale = new Vector3(15 + 3 * act.lebel, 15 + 3 * act.lebel, 1);
                                    effect.GetComponent<EffectAnime>().EffectStart(true);
                                }
                                SE(SEPath.BURN, 0.3f);
                                if (!act.enemy) unitListsEnemy = unitListEnemy;
                                else unitListsEnemy = unitList;
                                for (int j = 0; unitListsEnemy.Count > j; j++)
                                {
                                    Unit unitEnemy = unitListsEnemy[j].GetComponent<Unit>();
                                    if (unitEnemy.heal) continue;
                                    float distanse = (act.gameObject.transform.position - unitEnemy.unitInfo.transform.position).magnitude;
                                    if (distanse <= Constants.unitSizeFix * unitEnemy.unitSize / 2 + act.damageArea)
                                    {
                                        float damage = (12f + act.magic * 2f - unitEnemy.magicNow * 2f)/2f + act.actDamage;
                                        if (damage < 1) damage = 1f;
                                        damage = damage * (act.atk / unitEnemy.mDefNow) * act.rate;
                                        unitEnemy.hpNow -= damage * Constants.magicDamage;
                                    }
                                }
                            }
                            continue;
                        }
                        if (!act.enemy) unitListsEnemy = unitListEnemy;
                        else unitListsEnemy = unitList;
                        for (int j = 0; unitListsEnemy.Count > j; j++)
                        {
                            Unit unitEnemy = unitListsEnemy[j].GetComponent<Unit>();
                            if (unitEnemy.heal) continue;
                            float distanse = (act.gameObject.transform.position - unitEnemy.unitInfo.transform.position).magnitude;
                            if (distanse <= Constants.unitSizeFix * unitEnemy.unitSize / 2 + 2)
                            {
                                if (act.type == 2)
                                {
                                    bool already = false;
                                    foreach (Unit hit in act.hitUnits)
                                    {
                                        if (unitEnemy.unitNum == hit.unitNum)
                                        {
                                            already = true;
                                            break;
                                        }
                                    }
                                    if (already) continue;
                                    act.hitUnits.Add(unitEnemy);
                                }
                                float damage = (12f + act.magic * 2f - unitEnemy.magicNow * 2f) / 2f + act.actDamage;
                                if (damage < 1) damage = 1f;
                                damage = damage * (act.atk / unitEnemy.mDefNow) * act.rate;
                                unitEnemy.hpNow -= damage * Constants.magicDamage;
                                GameObject effect;
                                if (act.type == 1 || act.type == 2)
                                {
                                    switch (act.color)
                                    {
                                        case "赤":
                                            unitEnemy.hpNow -= 25 + act.lebel * 25;
                                            if (!pun)
                                            {
                                                effect = Instantiate(effectAnime[0], effectListObject.transform) as GameObject;
                                                effect.transform.position = (act.gameObject.transform.position + unitEnemy.unitInfo.transform.position) / 2;
                                                effect.GetComponent<EffectAnime>().EffectStart(false);
                                            }
                                            else
                                            {
                                                effect = PhotonNetwork.Instantiate("火", new Vector3(999, 999, 0), Quaternion.identity);
                                                effect.transform.position = (act.gameObject.transform.position + unitEnemy.unitInfo.transform.position) / 2;
                                                effect.GetComponent<EffectAnime>().EffectStart(true);
                                            }
                                            SE(SEPath.FIRE, 1);
                                            break;
                                        case "緑":
                                            Vector3 direction = unitEnemy.unitInfo.transform.position - act.gameObject.transform.position;
                                            //unitEnemy.unitInfo.transform.position = Vector3.MoveTowards(unitEnemy.unitInfo.transform.position, act.targetPoint, 1 + act.lebel);
                                            if (act.tacticsID == "TG8") act.lebel += 10;
                                            unitEnemy.unitInfo.transform.position = Vector3.MoveTowards(unitEnemy.unitInfo.transform.position, unitEnemy.unitInfo.transform.position + direction * (1 + act.lebel) / distanse, 1 + act.lebel);
                                            if (!pun)
                                            {
                                                effect = Instantiate(effectAnime[1], effectListObject.transform) as GameObject;
                                                effect.transform.position = (act.gameObject.transform.position + unitEnemy.unitInfo.transform.position) / 2;
                                                effect.GetComponent<EffectAnime>().EffectStart(false);
                                            }
                                            else
                                            {
                                                effect = PhotonNetwork.Instantiate("風", new Vector3(999, 999, 0), Quaternion.identity);
                                                effect.transform.position = (act.gameObject.transform.position + unitEnemy.unitInfo.transform.position) / 2;
                                                effect.GetComponent<EffectAnime>().EffectStart(true);
                                            }
                                            SE(SEPath.WIND, 1);
                                            break;
                                        case "青":
                                            if (unitEnemy.ice < act.lebel * 2) unitEnemy.ice = 2f + act.lebel * 2f;
                                            if (!pun)
                                            {
                                                effect = Instantiate(effectAnime[2], effectListObject.transform) as GameObject;
                                                effect.transform.position = (act.gameObject.transform.position + unitEnemy.unitInfo.transform.position) / 2;
                                                effect.GetComponent<EffectAnime>().EffectStart(false);
                                            }
                                            else
                                            {
                                                effect = PhotonNetwork.Instantiate("氷", new Vector3(999, 999, 0), Quaternion.identity);
                                                effect.transform.position = (act.gameObject.transform.position + unitEnemy.unitInfo.transform.position) / 2;
                                                effect.GetComponent<EffectAnime>().EffectStart(true);
                                            }
                                            SE(SEPath.ICE, 1);
                                            break;
                                        case "白":
                                            unitEnemy.actCoolTime += 0.1f;
                                            if (!pun)
                                            {
                                                effect = Instantiate(effectAnime[3], effectListObject.transform) as GameObject;
                                                effect.transform.position = (act.gameObject.transform.position + unitEnemy.unitInfo.transform.position) / 2;
                                                effect.GetComponent<EffectAnime>().EffectStart(false);
                                            }
                                            else
                                            {
                                                effect = PhotonNetwork.Instantiate("雷", new Vector3(999, 999, 0), Quaternion.identity);
                                                effect.transform.position = (act.gameObject.transform.position + unitEnemy.unitInfo.transform.position) / 2;
                                                effect.GetComponent<EffectAnime>().EffectStart(true);
                                            }
                                            SE(SEPath.THUNDER, 1);
                                            break;
                                        case "黒":
                                            Unit unit;
                                            if (!act.enemy) unit = unitList.Find(u => u.GetComponent<Unit>().unitNum == act.unitNum).GetComponent<Unit>();
                                            else unit = unitListEnemy.Find(u => u.GetComponent<Unit>().unitNum == act.unitNum).GetComponent<Unit>();
                                            if (unit.hpNow < unit.hpMaxNow)
                                            {
                                                unit.hpNow += act.lebel * 50;
                                                if (unit.hpNow > unit.hpMaxNow) unit.hpNow = unit.hpMaxNow;
                                            }
                                            if (!pun)
                                            {
                                                effect = Instantiate(effectAnime[4], effectListObject.transform) as GameObject;
                                                effect.transform.position = (act.gameObject.transform.position + unitEnemy.unitInfo.transform.position) / 2;
                                                effect.GetComponent<EffectAnime>().EffectStart(false);
                                            }
                                            else
                                            {
                                                effect = PhotonNetwork.Instantiate("コウモリ", new Vector3(999, 999, 0), Quaternion.identity);
                                                effect.transform.position = (act.gameObject.transform.position + unitEnemy.unitInfo.transform.position) / 2;
                                                effect.GetComponent<EffectAnime>().EffectStart(true);
                                            }
                                            SE(SEPath.BLOOD, 1);
                                            break;
                                    }
                                }
                                if (act.tacticsID != "")
                                {
                                    switch (act.tacticsID)
                                    {
                                        case "TG8":
                                            damage = act.power * 2 - unitEnemy.powerNow;
                                            if (damage < 1) damage = 1f;
                                            damage *= (act.atk / unitEnemy.defNow);
                                            unitEnemy.hpNow -= damage * Constants.magicDamage;
                                            break;
                                    }
                                }
                                if (act.type == 3)
                                {
                                    if (!pun)
                                    {
                                        effect = Instantiate(effectAnime[6], effectListObject.transform) as GameObject;
                                        effect.transform.position = act.gameObject.transform.position;
                                        effect.transform.localScale = new Vector3(15 + 3 * act.lebel, 15 + 3 * act.lebel, 1);
                                        effect.GetComponent<EffectAnime>().EffectStart(false);
                                    }
                                    else
                                    {
                                        effect = PhotonNetwork.Instantiate("爆発", new Vector3(999, 999, 0), Quaternion.identity);
                                        effect.transform.position = act.gameObject.transform.position;
                                        effect.transform.localScale = new Vector3(15 + 3 * act.lebel, 15 + 3 * act.lebel, 1);
                                        effect.GetComponent<EffectAnime>().EffectStart(true);
                                    }
                                    SE(SEPath.BURN, 0.3f);
                                    if (!act.enemy) unitListsEnemy = unitListEnemy;
                                    else unitListsEnemy = unitList;
                                    for (int k = 0; unitListsEnemy.Count > k; k++)
                                    {
                                        Unit unitEnemy2 = unitListsEnemy[k].GetComponent<Unit>();
                                        if (unitEnemy2.heal) continue;
                                        distanse = (act.gameObject.transform.position - unitEnemy2.unitInfo.transform.position).magnitude;
                                        if (distanse <= Constants.unitSizeFix * unitEnemy2.unitSize / 2 + act.damageArea)
                                        {
                                            damage = (12f + act.magic * 2f - unitEnemy.magicNow * 2f) / 2f + act.actDamage;
                                            if (damage < 1) damage = 1f;
                                            damage = damage * (act.atk / unitEnemy2.mDefNow) * act.rate;
                                            unitEnemy2.hpNow -= damage * Constants.magicDamage;
                                        }
                                    }
                                }
                                if (act.type != 2)
                                {
                                    actList.RemoveAt(i);
                                    if (!pun) Destroy(act.gameObject);
                                    else PhotonNetwork.Destroy(act.gameObject);
                                    break;
                                }
                            }
                        }
                    }
                    else if (act.type == 11)
                    {
                        act.speed += 1 * frameRate;
                        act.gameObject.transform.position = CalcLerpPoint(act.startPoint, act.halfPoint, act.targetPoint, act.speed / 100f);
                        if (act.speed >= 100)
                        {
                            actList.RemoveAt(i);
                            if (!pun) Destroy(act.gameObject);
                            else PhotonNetwork.Destroy(act.gameObject);
                            GameObject effect;
                            if (!pun)
                            {
                                effect = Instantiate(effectAnime[5], effectListObject.transform) as GameObject;
                                effect.transform.position = act.gameObject.transform.position;
                                effect.transform.localScale = new Vector3(10 + act.rate, 10 + act.rate, 1);
                                effect.GetComponent<EffectAnime>().EffectStart(false);
                            }
                            else
                            {
                                effect = PhotonNetwork.Instantiate("強爆発", new Vector3(999, 999, 0), Quaternion.identity);
                                effect.transform.position = act.gameObject.transform.position;
                                effect.transform.localScale = new Vector3(10 + 1 * act.rate, 10 + 1 * act.rate, 1);
                                effect.GetComponent<EffectAnime>().EffectStart(true);
                            }
                            SE(SEPath.BURN, 0.5f);
                            if (!act.enemy) unitListsEnemy = unitListEnemy;
                            else unitListsEnemy = unitList;
                            for (int j = 0; unitListsEnemy.Count > j; j++)
                            {
                                Unit unitEnemy = unitListsEnemy[j].GetComponent<Unit>();
                                float distanse = (act.gameObject.transform.position - unitEnemy.unitInfo.transform.position).magnitude;
                                if (distanse <= Constants.unitSizeFix * unitEnemy.unitSize / 2 + act.damageArea)
                                {
                                    float damage = (4 + act.power * 1.5f - unitEnemy.powerNow * 1.5f + act.rate) * (act.damageArea * 1.5f - distanse) / act.damageArea;
                                    if (damage < 1) damage = 1f;
                                    damage *= (act.atk / unitEnemy.defNow);
                                    unitEnemy.hpNow -= damage * Constants.magicDamage;
                                }
                            }
                        }
                    }
                }

            }

            for (int a = 0; a < 2; a++)
            {
                List<GameObject> unitLists = null;
                if (a == 0) unitLists = unitList;
                if (a == 1) unitLists = unitListEnemy;
                for (int i = unitLists.Count-1; 0 <= i; i--)
                {
                    Unit unit = unitLists[i].GetComponent<Unit>();
                    //ブレイク判定(戦闘判定のブレイク速度低下後に処理する必要がある)
                    unit.breakSuccess = false;
                    if (unit.breakCheck)
                    {
                        unit.breakFrame.SetActive(true);
                        if (unit.stopCheak)
                        {
                            if (unit.breakGauge.GetComponent<RectTransform>().sizeDelta.x < 10)
                            {
                                if (unit.jobNow == 4 && unit.skillNow > 0)
                                {
                                    unit.breakGauge.GetComponent<RectTransform>().sizeDelta = new Vector2(unit.breakGauge.GetComponent<RectTransform>().sizeDelta.x + 0.07f * unit.breakSpeedNow / 100 * (unit.breakSpeedPer + unit.skillNow * 0.02f) * frameRate, 3);
                                }
                                else
                                {
                                    unit.breakGauge.GetComponent<RectTransform>().sizeDelta = new Vector2(unit.breakGauge.GetComponent<RectTransform>().sizeDelta.x + 0.07f * unit.breakSpeedNow / 100 * unit.breakSpeedPer * frameRate, 3);
                                }
                            }

                            if (unit.breakGauge.GetComponent<RectTransform>().sizeDelta.x >= 10)
                            {
                                StartCoroutine(unit.BreakAnime());
                                unit.breakGauge.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 3);
                                if (a == 0) hp2.GetComponent<RectTransform>().sizeDelta = new Vector2(hp2.GetComponent<RectTransform>().sizeDelta.x - unit.breakNow / 100 * unit.cost * (120 - Math.Abs(unit.unitInfo.transform.position.x)) / 120, 50);
                                if (a == 1) hp.GetComponent<RectTransform>().sizeDelta = new Vector2(hp.GetComponent<RectTransform>().sizeDelta.x - unit.breakNow / 100 * unit.cost * (120 - Math.Abs(unit.unitInfo.transform.position.x)) / 120, 50);
                                float damage = 200 - (unit.powerNow + unit.magicNow + unit.skillNow) * 5;
                                if (damage < 0) damage = 0;
                                if (unit.jobNow != 4) damage += 100;
                                unit.hpNow -= damage;
                                unit.breakSuccess = true;
                            }
                        }
                    }
                    else
                    {
                        unit.breakFrame.SetActive(false);
                        unit.breakGauge.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 3);
                    }
                    DisplayHP(unit);
                    //撤退判定(ここまでにすべてのダメージ判定を終わらせる※ブレイク反射ダメージ)
                    if (unit.hpNow <= 0 && !unit.death)
                    {

                        yield return UnitDeath(unit);
                        if (unit.noRevival)
                        {
                            unit.StopAllCoroutines();
                            unitLists.RemoveAt(i);
                            if (pun)
                            {
                                //PhotonNetwork.Destroy(unit.gameObject);
                            }
                            else
                            {
                                //Destroy(unit.gameObject);
                            }
                            continue;
                        }
                    }
                    for (int j = crystalList.Count - 1; j >= 0; j--)
                    {
                        Crystal crystal = crystalList[j].GetComponent<Crystal>();
                        float distanse = (unit.unitInfo.transform.position - crystal.transform.position).magnitude;
                        if (unit.jobNow == 4) distanse -= unit.skillNow * 0.2f;
                        if (distanse < Constants.unitSizeFix * unit.unitSize / 2 + 1)
                        {
                            if (!unit.enemy)
                            {
                                if(!crystal.enemy) hp.GetComponent<RectTransform>().sizeDelta = new Vector2(hp.GetComponent<RectTransform>().sizeDelta.x + unit.breakNow / 100 * unit.cost * 2, 50);
                                if (crystal.enemy) hp2.GetComponent<RectTransform>().sizeDelta = new Vector2(hp2.GetComponent<RectTransform>().sizeDelta.x - unit.breakNow / 100 * unit.cost  * 2, 50);
                            }
                            if (unit.enemy)
                            {
                                if (!crystal.enemy) hp.GetComponent<RectTransform>().sizeDelta = new Vector2(hp.GetComponent<RectTransform>().sizeDelta.x - unit.breakNow / 100 * unit.cost  * 2, 50);
                                if (crystal.enemy) hp2.GetComponent<RectTransform>().sizeDelta = new Vector2(hp2.GetComponent<RectTransform>().sizeDelta.x + unit.breakNow / 100 * unit.cost * 2, 50);
                            }
                            GameObject effect;
                            if (!pun)
                            {
                                effect = Instantiate(effectAnime[7], effectListObject.transform) as GameObject;
                                effect.transform.position = unit.unitInfo.gameObject.transform.position;
                                effect.GetComponent<EffectAnime>().EffectStart(false);
                            }
                            else
                            {
                                effect = PhotonNetwork.Instantiate("キラキラ", new Vector3(999, 999, 0), Quaternion.identity);
                                effect.transform.position = unit.unitInfo.gameObject.transform.position;
                                effect.GetComponent<EffectAnime>().EffectStart(true);
                            }
                            SE(SEPath.CRYSTAL, 1f);
                            crystalList.RemoveAt(j);
                            if (!pun) Destroy(crystal.gameObject);
                            else PhotonNetwork.Destroy(crystal.gameObject);
                        }
                    }
                }
            }
            for (int a = 0; a < 2; a++)
            {
                List<GameObject> unitLists = null;
                if (a == 0) unitLists = unitList;
                if (a == 1) unitLists = unitListEnemy;
                for (int i = 0; unitLists.Count > i; i++)
                {
                    Unit unit = unitLists[i].GetComponent<Unit>();
                    //移動判定
                    {
                        if (!unit.heal)
                        {
                            if (!unit.enemy && unit.unitInfo.transform.position.y > 55) unit.unitInfo.transform.position = new Vector3(unit.unitInfo.transform.position.x, 55, 0);
                            if (unit.enemy && unit.unitInfo.transform.position.y < -55) unit.unitInfo.transform.position = new Vector3(unit.unitInfo.transform.position.x, -55, 0);
                            if (unit.jobNow == 1) unit.speedUpFix += int.Parse(unit.skill.text) * 3;
                            float speed = unit.speedNow - unit.ice + unit.speedUpFix;
                            if (speed < 5) speed = 5;
                            if (unit.actCoolTime == 0)
                            {
                                if(Math.Abs(unit.unitInfo.transform.position.x)>60) unit.unitInfo.transform.position = Vector3.MoveTowards(unit.unitInfo.transform.position, new Vector3(0, unit.unitInfo.transform.position.y,0), speed * Constants.moveSpeed * frameRate /1000);
                                else unit.unitInfo.transform.position = Vector3.MoveTowards(unit.unitInfo.transform.position, unit.pin.transform.position, speed * Constants.moveSpeed * frameRate /1000);
                            }
                            //騎兵加速
                            if (unit.jobNow == 1)
                            {
                                if (speed <= 90 || unit.battleTarget.Count > 0 || (unit.breakCheck && unit.stopCheak))
                                {
                                    unit.skillAct = 0;
                                }
                                else if (unit.pin.transform.position != unit.unitInfo.transform.position)
                                {
                                    if (unit.skillNow > unit.skillAct) unit.skillAct += 0.01f * (unit.actSpeedNow + int.Parse(unit.skill.text) * 20) / 100 * frameRate;
                                }
                                unit.skill.text = Math.Floor(unit.skillAct).ToString();
                                if (Math.Floor(unit.skillAct) > 0)
                                {
                                    unit.trail.enabled = true;
                                    unit.trail.startWidth = 0.4f + int.Parse(unit.skill.text) * 0.4f;
                                }
                                else
                                {
                                    unit.trail.enabled = false;
                                }
                            }
                            else
                            {
                                unit.trail.enabled = false;
                            }
                            if (Math.Abs(unit.unitInfo.transform.position.y) > 60)
                            {
                                unit.heal = true;
                                unit.unitInfo.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                                if (unit.jobNow == 1)
                                {
                                    unit.trail.enabled = false;
                                    unit.skill.text = "0";
                                    unit.skillAct = 0;
                                }
                                if (!debug && a == 1) unit.unitInfo.gameObject.SetActive(false);
                            }

                        }
                        else
                        {
                            if (a == 0) unit.unitInfo.transform.position = new Vector3(unit.pin.transform.position.x, -61.5f);
                            if (a == 1) unit.unitInfo.transform.position = new Vector3(unit.pin.transform.position.x, 61.5f);
                            if (unit.jobNow == 1) unit.skillAct = 0;
                            if (!unit.death)
                            {
                                if (Math.Abs(unit.pin.transform.position.y) <= 55 && unit.actCoolTime == 0)
                                {
                                    if (unit.start < 250)
                                    {
                                        unit.start += 1 * frameRate;
                                    }
                                    if (unit.start >= 250)
                                    {
                                        if (a == 0) unit.unitInfo.transform.position = new Vector3(unit.pin.transform.position.x, -55);
                                        if (a == 1) unit.unitInfo.transform.position = new Vector3(unit.pin.transform.position.x, 55);
                                        unit.heal = false;
                                        unit.unitInfo.transform.localScale = new Vector3(unit.unitSize,unit.unitSize, 1);
                                        unit.start = 0;
                                        if (a == 1) unit.unitInfo.gameObject.SetActive(true);
                                    }
                                }
                                else
                                {
                                    unit.start = 0;
                                }
                            }
                        }
                        unit.root.transform.position = new Vector2((unit.pin.transform.position.x + unit.unitInfo.transform.position.x) / 2, (unit.pin.transform.position.y + unit.unitInfo.transform.position.y) / 2);
                        unit.root.GetComponent<RectTransform>().sizeDelta = new Vector2(80 / ((unit.pin.transform.position - unit.unitInfo.transform.position).magnitude + 20), (unit.pin.transform.position - unit.unitInfo.transform.position).magnitude);
                        unit.root.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(unit.unitInfo.transform.position.x - unit.pin.transform.position.x, unit.unitInfo.transform.position.y - unit.pin.transform.position.y) * Mathf.Rad2Deg);
                    }
                }
            }
            //時間表示
            if (frame % (100 / frameRate) == 0) time.text = (int.Parse(time.text) - 1).ToString();
            if (frame % (10 / frameRate) == 0)
            {
                yield return tactics.TacticsSecounds();
                yield return commander.CommandUpdate();
            }
            if (int.Parse(time.text) < 0 || hp.GetComponent<RectTransform>().sizeDelta.x < 0 || hp2.GetComponent<RectTransform>().sizeDelta.x < 0) Result();
            tactics.TacticsTarget();
            actUnit = new List<Unit>();//戦術処理に回すため
            if (cpu) CpuCreate(frame * frameRate);
            frame++;
            CrystalCreate();
            yield return new WaitForSecondsRealtime(0.01f * frameRate);
        }
    }
    public Unit CardLoad(string colorNo)
    {
        Card card = cardMaster.CardList.Find(c => c.colorNo == colorNo);
        Unit unit = null;
        if (pun)
        {
            unit = PhotonNetwork.Instantiate(card.job, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<Unit>();
            unit.transform.SetParent(unitListObject.transform);
        }
        else
        {
            unit = (Instantiate(jobPrefab[card.jobNo], unitListObject.transform) as GameObject).GetComponent<Unit>();
        }
        unit.unitInfo.SetActive(true);
        unit.pin.SetActive(true);
        unit.root.SetActive(true);
        unit.card = card;
        unit.cost = card.cost;
        unit.actTypeDf = card.magicType;
        unit.tacticsId = card.tacticsID;
        unit.powerDf = card.power;
        unit.magicDf = card.magic;
        unit.skillDf = card.skill;
        StartCoroutine(LoadIllust(unit, card.colorNo));
        unit.charaName.text = card.name;
        unit.power.text = unit.powerDf.ToString();
        unit.magic.text = unit.magicDf.ToString();
        if (unit.jobDf == 2 || unit.jobDf == 3 || unit.jobDf == 4) unit.skill.text = unit.skillDf.ToString();
        if (unit.jobDf == 2 || unit.jobDf == 4) unit.skillAct = unit.skillDf;
        if (unit.jobDf == 3) unit.skillAct = unit.skillDf + 0.9f;
        if (unit.jobDf == 5)
        {
            unit.skillAct = unit.skillDf / 2f;
            unit.skill.text = Math.Floor(unit.skillAct).ToString();
        }
        DfToNow(unit);
        unit.tacticsArea.GetComponent<RectTransform>().sizeDelta = new Vector2(card.tacticsAreaSizeX, card.tacticsAreaSizeY);
        unit.tacticsArea.transform.localPosition = new Vector3(card.tacticsAreaCenterX, card.tacticsAreaCenterY, 0);

        if (card.tacticsAreaType == "円形") unit.tacticsArea.sprite = shape[0];
        if (card.tacticsAreaType == "四角") unit.tacticsArea.sprite = shape[1];
        if (unit.jobDf == 1) unit.trail.enabled = false;
        return unit;
    }
    public void LoadIllustPublic(Unit unit, string colorNo)
    {
        if (unit.chara.sprite.name != colorNo) StartCoroutine(LoadIllust(unit, colorNo));
    }
    IEnumerator LoadIllust(Unit unit, string colorNo)
    {
        if (!colorNo.Contains("-"))
        {
            var illust = Addressables.LoadAssetAsync<Sprite>(colorNo);
            yield return illust;
            unit.chara.sprite = illust.Result;
        }
    }
    void CpuCreate(int frame)
    {
        //ユニット生成
        Unit UnitCreate(int jobNum, string job)
        {
            Unit unit;
            if (pun)
            {
                unit = PhotonNetwork.Instantiate(job, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<Unit>();
                unit.gameObject.transform.SetParent(unitListObject.transform);
            }
            else
            {
                unit = (Instantiate(jobPrefab[jobNum], unitListObject.transform) as GameObject).GetComponent<Unit>();
            }
            unit.cost = 3;
            unit.heal = true;
            if (!debug) unit.unitInfo.SetActive(false);
            UnitEnemy(unit, true, true);
            RamdonStatus(unit, cpuLevel-cpuRandom, cpuLevel+cpuRandom);
            unit.unitNum = unitNumEnemy;
            unitNumEnemy++;
            unitListEnemy.Add(unit.gameObject);
            DfToNow(unit);
            return unit;
        }
        if (frame ==0 || frame % 2600 == 0)
        {
            Unit unit = UnitCreate(1, "剣");
            float ramdom = UnityEngine.Random.value - 0.5f;
            if (ramdom >= 0) unit.unitInfo.transform.position = new Vector3(15 + ramdom * 40, 61.5f);
            else unit.unitInfo.transform.position = new Vector3(-15 + ramdom * 40, 61.5f);
            unit.unitInfo.transform.localScale = new Vector3(0.7f, 0.7f, 1);
            unit.pin.transform.position = unit.unitInfo.transform.position;
        }
        if (frame == 0 || frame % 2400 == 0)
        {
            Unit unit = UnitCreate(2, "槍");
            float ramdom = UnityEngine.Random.value - 0.5f;
            if (ramdom >= 0) unit.unitInfo.transform.position = new Vector3(15 + ramdom * 30, 61.5f);
            else unit.unitInfo.transform.position = new Vector3(-15 + ramdom * 30, 61.5f);
            unit.unitInfo.transform.localScale = new Vector3(0.7f, 0.7f, 1);
            unit.pin.transform.position = unit.unitInfo.transform.position;
        }
        if (frame == 0 || frame % 6000 == 0)
        {
            Unit unit = UnitCreate(3, "弓");
            float ramdom = UnityEngine.Random.value - 0.5f;
            if (ramdom >= 0) unit.unitInfo.transform.position = new Vector3(ramdom * 40, 61.5f);
            else unit.unitInfo.transform.position = new Vector3(ramdom * 40, 61.5f);
            unit.unitInfo.transform.localScale = new Vector3(0.7f, 0.7f, 1);
        }
        if (frame == 0 || (frame+1100) % 2200 == 0)
        {
            Unit unit = UnitCreate(4, "斧");
            float ramdom = UnityEngine.Random.value - 0.5f;
            if (ramdom >= 0) unit.unitInfo.transform.position = new Vector3(45 + ramdom * 20, 61.5f);
            else unit.unitInfo.transform.position = new Vector3(-45 + ramdom * 20, 61.5f);
            unit.unitInfo.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            unit.pin.transform.position = unit.unitInfo.transform.position;
        }
        if (frame == 0 || (frame + 3000) % 6000 == 0)
        {
            Unit unit = UnitCreate(5, "魔");
            float ramdom = UnityEngine.Random.value - 0.5f;
            if (ramdom >= 0) unit.unitInfo.transform.position = new Vector3(35 + ramdom * 30, 61.5f);
            else unit.unitInfo.transform.position = new Vector3(-35 + ramdom * 30, 61.5f);
            unit.unitInfo.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            unit.pin.transform.position = unit.unitInfo.transform.position;
            unit.skillAct = 0;
            unit.magicNow += 2;
            unit.magic.text = unit.magicNow.ToString();
            unit.powerNow -= 2;
            unit.power.text = unit.powerNow.ToString();
            switch (UnityEngine.Random.Range(1, 3))
            {
                case 1:
                    unit.actTypeNow = "連射";
                    break;
                case 2:
                    unit.actTypeNow = "拡散";
                    break;
                case 3:
                    unit.actTypeNow = "貫通";
                    break;
            }
            unit.card = new Card();
            switch (UnityEngine.Random.Range(1, 5))
            {
                case 1:
                    unit.card.color = "赤";
                    break;
                case 2:
                    unit.card.color = "緑";
                    break;
                case 3:
                    unit.card.color = "青";
                    break;
                case 4:
                    unit.card.color = "白";
                    break;
                case 5:
                    unit.card.color = "黒";
                    break;
            }

        }
        //CPUの移動先
        for (int i = 0; i < unitListEnemy.Count; i++)
        {
            Unit unit = unitListEnemy[i].GetComponent<Unit>();
            if (unit.heal && unit.hpNow < 9000) continue;
            if (unit.pinMoveDisable > 0) continue;
            unit.pin.transform.position = new Vector3(unit.unitInfo.transform.position.x, -50, 0);
            if (unit.breakCheck) continue;
            if (unit.jobNow == 1)
            {
                if (unit.skillAct > 0.9)
                {
                    float targetDistanse = 0;
                    for (int j = 0; unitList.Count > j; j++)
                    {
                        Unit unitEnemy = unitList[j].GetComponent<Unit>();
                        float distanse = (unit.unitInfo.transform.position - unitEnemy.unitInfo.transform.position).magnitude;

                        if (distanse < 45 && !unitEnemy.heal && !(unitEnemy.jobDf == 2 && !unitEnemy.breakCheck && unitEnemy.battleTarget.Count == 0))
                        {
                            if (targetDistanse == 0)
                            {
                                targetDistanse = distanse;
                                unit.pin.transform.position = unitEnemy.unitInfo.transform.position;
                            }
                            else if (distanse < targetDistanse)
                            {
                                targetDistanse = distanse;
                                unit.pin.transform.position = unitEnemy.unitInfo.transform.position;
                            }
                        }
                    }
                }
            }
            else if (unit.jobNow == 2 && !unit.heal)
            {
                float targetDistanse = 0;
                if (unit.unitInfo.transform.position.y > 10)
                {
                    for (int j = 0; unitListEnemy.Count > j; j++)
                    {

                        Unit unitArrow = unitListEnemy[j].GetComponent<Unit>();
                        if (unitArrow.jobDf == 3 && unitArrow.hpNow > 7000)
                        {
                            float distanse = (unit.unitInfo.transform.position - unitArrow.unitInfo.transform.position).magnitude;
                            if (distanse < 30)
                            {
                                if (targetDistanse == 0)
                                {
                                    targetDistanse = distanse;
                                    unit.pin.transform.position = unitArrow.unitInfo.transform.position + new Vector3(0, -5, 0);
                                }
                                else if (distanse < targetDistanse)
                                {
                                    targetDistanse = distanse;
                                    unit.pin.transform.position = unitArrow.unitInfo.transform.position + new Vector3(0, -5, 0);

                                }
                            }
                        }
                    }
                }
                for (int j = 0; unitList.Count > j; j++)
                {
                    Unit unitEnemy = unitList[j].GetComponent<Unit>();
                    float distanse = (unit.unitInfo.transform.position - unitEnemy.unitInfo.transform.position).magnitude;
                    if (distanse < 40 && unitEnemy.breakCheck)
                    {
                        unit.pin.transform.position = unitEnemy.unitInfo.transform.position;
                    }
                }
                targetDistanse = 0;
                for (int j = 0; unitList.Count > j; j++)
                {
                    Unit unitEnemy = unitList[j].GetComponent<Unit>();
                    float distanse = (unit.unitInfo.transform.position - unitEnemy.unitInfo.transform.position).magnitude;

                    if (!unitEnemy.heal && distanse < 30 && unitEnemy.jobNow == 1)
                    {
                        if (targetDistanse == 0)
                        {
                            targetDistanse = distanse;
                            unit.pin.transform.position = unitEnemy.unitInfo.transform.position;
                        }
                        else if (distanse < targetDistanse)
                        {
                            targetDistanse = distanse;

                        }
                    }
                }
            }
            else if (unit.jobNow == 3 && !unit.heal)
            {
                if (unit.skillAct > 1) unit.pin.transform.position = new Vector3(unit.unitInfo.transform.position.x, -25, 0);
                for (int j = 0; unitList.Count > j; j++)
                {
                    Unit unitEnemy = null;
                    float distanse = 100;
                    float arrowLong = 60 + unit.skillAct * 3f;
                    for (int k = 0; unitList.Count > k; k++)
                    {
                        Unit unitEnemyNow = unitList[k].GetComponent<Unit>();
                        float distanseNow = (unit.unitInfo.transform.position - unitEnemyNow.unitInfo.transform.position).magnitude;
                        if (!unitEnemyNow.heal && distanse > distanseNow)
                        {
                            unitEnemy = unitEnemyNow;
                            distanse = distanseNow;
                        }
                    }

                    if (unitEnemy != null)
                    {
                        if (distanse < arrowLong / 2 * 1.1f)
                        {
                            unit.pin.transform.position = unitEnemy.unitInfo.transform.position;
                        }
                        if ((distanse < arrowLong / 2 * 0.9f) || unit.arrowTarget.Count > 0)
                        {
                            unit.pin.transform.position = unit.unitInfo.transform.position;
                        }
                    }
                }
            }
            else if (unit.jobNow == 4)
            {
                for (int j = 0; unitList.Count > j; j++)
                {
                    Unit unitEnemy = unitList[j].GetComponent<Unit>();
                    float distanse = (unit.unitInfo.transform.position - unitEnemy.unitInfo.transform.position).magnitude;
                    if (distanse < 30 && unitEnemy.breakCheck)
                    {
                        unit.pin.transform.position = unitEnemy.unitInfo.transform.position;
                    }
                }
            }
            else if (unit.jobNow == 5 && !unit.heal)
            {
                unit.pin.transform.position = new Vector3(unit.unitInfo.transform.position.x, -20, 0);
                Unit unitEnemy = null;
                float distanse = 80;
                float actLong = 26;
                if (unit.actTypeNow == "連射") actLong += unit.skillAct * 1.5f;
                if (unit.actTypeNow == "拡散") actLong += unit.skillAct * 1.0f;
                if (unit.actTypeNow == "貫通") actLong += unit.skillAct * 2.5f;
                for (int j = 0; unitList.Count > j; j++)
                {
                    Unit unitEnemyNow = unitList[j].GetComponent<Unit>();
                    float distanseNow = (unit.unitInfo.transform.position - unitEnemyNow.unitInfo.transform.position).magnitude;
                    if (!unitEnemyNow.heal && distanse > distanseNow)
                    {
                        unitEnemy = unitEnemyNow;
                        distanse = distanseNow;
                    }
                }
                if (unitEnemy != null)
                {
                    if (distanse >= actLong)
                    {
                        unit.pin.transform.position = unitEnemy.unitInfo.transform.position;
                    }
                    if (distanse < actLong)
                    {
                        unit.pin.transform.position = unit.unitInfo.transform.position;
                        unit.jobDirection.transform.rotation = Quaternion.Euler(0, 0, +180 - Mathf.Atan2(unit.unitInfo.transform.position.x - unitEnemy.unitInfo.transform.position.x, unit.unitInfo.transform.position.y - unitEnemy.unitInfo.transform.position.y) * Mathf.Rad2Deg);
                        unit.actAngle.transform.rotation = unit.jobDirection.transform.rotation;
                        if (unit.actCoolTime == 0) Act(unit);
                    }
                    if (distanse < actLong * 0.6f)
                    {
                        unit.pin.transform.position = new Vector3(unit.unitInfo.transform.position.x, 55f, 0);
                        if(unit.unitInfo.transform.position.y > 35 && unit.unitInfo.transform.position.x > unitEnemy.unitInfo.transform.position.x) unit.pin.transform.position = new Vector3(60, unit.unitInfo.transform.position.y, 0);
                        if (unit.unitInfo.transform.position.y > 35 && unit.unitInfo.transform.position.x < unitEnemy.unitInfo.transform.position.x) unit.pin.transform.position = new Vector3(-60, unit.unitInfo.transform.position.y, 0);
                    }
                }
            }
            if (unit.unitInfo.transform.position.y > 5) if (unit.hpNow < 100 * unit.unitInfo.transform.position.y + 1000) unit.pin.transform.position = new Vector3(unit.unitInfo.transform.position.x, 61.5f, 0);
        }
    }
    public void UnitEnemy(Unit unit, bool noRevival, bool cpu)
    {
        unit.breakAttack.transform.localPosition = new Vector3(0, -10, 0);
        unit.power.color = new Color(0.3f, 0.3f, 0.8f);
        unit.magic.color = new Color(0.3f, 0.3f, 0.8f);
        unit.skill.color = new Color(0.3f, 0.3f, 0.8f);
        unit.jobDirection.transform.rotation = Quaternion.Euler(0, 0, 180);
        unit.enemy = true;
        if (!debug) unit.pin.SetActive(false);
        if (!debug) unit.root.SetActive(false);
        unit.noRevival = noRevival;
        unit.cpu = cpu;
        if (unit.jobDf == 2 || unit.jobDf == 3) unit.jobArea.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.5f, 0.5f);
        if (unit.jobDf == 3) unit.arrow.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.5f);
        unit.punPlayer = 1;
        if(unit.card.tacticsAreaSpin != "可能")unit.tacticsArea.transform.localPosition = -unit.tacticsArea.transform.localPosition;
    }
    void RamdonStatus(Unit unit, int min, int max)
    {
        int p = UnityEngine.Random.Range(min, max);
        int m = UnityEngine.Random.Range(min, max);
        int s = UnityEngine.Random.Range(min, max);
        unit.powerDf = p;
        unit.magicDf = m-2;
        unit.skillDf = s;
        unit.power.text = p.ToString();
        unit.magic.text = (m-2).ToString();
        unit.skill.text = s.ToString();
    }
    void StageClick()
    {
        //ユニット移動
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 200f))
            {
                SEManager.Instance.Play(SEPath.CLICK);
                Vector3 center = new Vector3(0, 0, 0);
                int selectNum = 0;
                for (int i = 0; playerList.Count > i; i++)
                {
                    Unit unit = playerList[i].GetComponent<Unit>();

                    if (unit.select)
                    {
                        center += unit.unitInfo.transform.position;
                        selectNum++;
                    }
                }
                center /= selectNum;
                Vector3 move = hit.point - center;
                if (hit.point.y > -55)
                {
                    for (int i = 0; playerList.Count > i; i++)
                    {
                        Unit unit = playerList[i].GetComponent<Unit>();
                        if (unit.select)
                        {
                            Vector3 pinPoint = unit.unitInfo.transform.position + move;
                            if (selectNum > 1)
                            {
                                if (pinPoint.x > 55) pinPoint = new Vector3(55, pinPoint.y);
                                if (pinPoint.x < -55) pinPoint = new Vector3(-55, pinPoint.y);
                                if (pinPoint.y > 50) pinPoint = new Vector3(pinPoint.x, 50);
                                if (pinPoint.y < -50) pinPoint = new Vector3(pinPoint.x, -50);
                            }
                            if (PinCheak(pinPoint, i, unit.enemy))
                            {
                                unit.PinMove(pinPoint);
                            }
                        }
                    }
                }
                else if (hit.point.y <= -60)
                {
                    for (int i = 0; playerList.Count > i; i++)
                    {
                        Unit unit = playerList[i].GetComponent<Unit>();
                        if (unit.select)
                        {
                            unit.PinMove(new Vector3(unit.unitInfo.transform.position.x + move.x, -61.5f));
                        }
                    }
                }
            }
            //検証用敵ユニット移動
            if (debug && PhotonNetwork.IsMasterClient || debug && !pun)
            {
                for (int i = 0; unitListEnemy.Count > i; i++)
                {
                    Unit unit = unitListEnemy[i].GetComponent<Unit>();
                    if (unit.select)
                    {
                        if (Physics.Raycast(ray, out hit, 200f))
                        {
                            unit.PinMove(hit.point);
                        }
                    }
                }
            }
        }

    }
    public bool PinCheak(Vector3 point, int unitNum, bool enemy)
    {
        //ピンの位置が近すぎないように
        /*
        if (enemy)
        {
            for (int i = 0; unitListEnemy.Count > i; i++)
            {
                if (unitNum != i)
                {
                    Unit unit = unitListEnemy[i].GetComponent<Unit>();
                    float distanse = (unit.pin.transform.position - point).magnitude;
                    if (distanse < 5) return false;
                }
            }
        }
        else
        {
            for (int i = 0; unitList.Count > i; i++)
            {
                if (unitNum != i)
                {
                    Unit unit = unitList[i].GetComponent<Unit>();
                    float distanse = (unit.pin.transform.position - point).magnitude;
                    if (distanse < 5) return false;
                }
            }
        }
        */
        return true;
    }
    public void SelectClear()
    {
        //選択全解除
        for (int i = 0; unitList.Count > i; i++)
        {
            unitList[i].GetComponent<Unit>().SelectReset();
        }
        for (int i = 0; unitListEnemy.Count > i; i++)
        {
            unitListEnemy[i].GetComponent<Unit>().SelectReset();
        }
    }
    private void Update()
    {

        //右クリック
        if (Input.GetMouseButtonDown(1))
        {
            SEManager.Instance.Play(SEPath.CLICK);
            SelectClear();
        }
        //中クリック
        if (Input.GetMouseButtonDown(2) || Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.LeftShift))
        {
            SEManager.Instance.Play(SEPath.CLICK);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 200f))
            {
                for (int i = 0; playerList.Count > i; i++)
                {
                    Unit unit = playerList[i].GetComponent<Unit>();
                    if (unit.select)
                    {
                        unit.UnitAngle(hit.point);
                    }
                }
            }
            AllAct();
        }
        //全選択
        if (Input.GetKey(KeyCode.Q))
        {
            for (int i = 0; playerList.Count > i; i++)
            {
                playerList[i].GetComponent<Unit>().UnitSelect();
            }
        }
        //番号キー部隊選択
        if (Input.GetKey(KeyCode.Alpha1) || Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Alpha3) || Input.GetKey(KeyCode.Alpha4) || Input.GetKey(KeyCode.Alpha5) || Input.GetKey(KeyCode.Alpha6) || Input.GetKey(KeyCode.Alpha7) || Input.GetKey(KeyCode.Alpha8) || Input.GetKey(KeyCode.Alpha9))
        {
            if (!Input.GetKey(KeyCode.Tab) && !Input.GetKey(KeyCode.LeftShift)) SelectClear();
            if (Input.GetKey(KeyCode.Alpha1) && playerList.Count >= 1)
            {
                playerList[0].GetComponent<Unit>().UnitSelect();
            }
            if (Input.GetKey(KeyCode.Alpha2) && playerList.Count >= 2)
            {
                playerList[1].GetComponent<Unit>().UnitSelect();
            }
            if (Input.GetKey(KeyCode.Alpha3) && playerList.Count >= 3)
            {
                playerList[2].GetComponent<Unit>().UnitSelect();
            }
            if (Input.GetKey(KeyCode.Alpha4) && playerList.Count >= 4)
            {
                playerList[3].GetComponent<Unit>().UnitSelect();
            }
            if (Input.GetKey(KeyCode.Alpha5) && playerList.Count >= 5)
            {
                playerList[4].GetComponent<Unit>().UnitSelect();
            }
            if (Input.GetKey(KeyCode.Alpha6) && playerList.Count >= 6)
            {
                playerList[5].GetComponent<Unit>().UnitSelect();
            }
            if (Input.GetKey(KeyCode.Alpha7) && playerList.Count >= 7)
            {
                playerList[6].GetComponent<Unit>().UnitSelect();
            }
            if (Input.GetKey(KeyCode.Alpha8) && playerList.Count >= 8)
            {
                playerList[7].GetComponent<Unit>().UnitSelect();
            }
            if (Input.GetKey(KeyCode.Alpha9) && playerList.Count >= 9)
            {
                playerList[8].GetComponent<Unit>().UnitSelect();
            }
            return;
        }
        if (start)
        {
            //戦術
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Space))
            {
                if (tactics.tacticsButton.interactable) tactics.Activate();
            }
            //ロック
            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                if (tactics.onLock.activeSelf)
                {
                    tactics.UnLock();
                }
                else
                {
                    tactics.Lock();
                }
            }
            //作戦
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Z))
            {
                if (commander.commandButton[0].interactable) commander.ActivateButtun0();
            }
            //戦術
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.X))
            {
                if (commander.commandButton[1].interactable) commander.ActivateButtun1();
            }
            //アクト
            if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.LeftShift))
            {
                AllAct();
            }
            //移動指示
            if (Input.GetKey(KeyCode.W))
            {
                Forward();
            }
            if (Input.GetKey(KeyCode.S))
            {
                Center();
            }
            if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.X))
            {
                Back();
            }
            if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Z))
            {
                Stop();
            }
        }
    }
    public void AllSelect()
    {
        for (int i = 0; playerList.Count > i; i++)
        {
            playerList[i].GetComponent<Unit>().UnitSelect();
        }
    }
    public void AllAct()
    {
        for (int i = 0; playerList.Count > i; i++)
        {
            Unit unit = playerList[i].GetComponent<Unit>();
            if (unit.select)
            {
                unit.ActButton();
            }
        }
    }
    public void Forward()
    {
        for (int i = 0; playerList.Count > i; i++)
        {
            Unit unit = playerList[i].GetComponent<Unit>();
            if (unit.select)
            {
                unit.PinMove(new Vector3(unit.unitInfo.transform.position.x, 49.5f));
                if (pun && !PhotonNetwork.IsMasterClient) unit.PinMove(new Vector3(unit.unitInfo.transform.position.x, -49.5f));
            }
        }
    }
    public void Center()
    {
        for (int i = 0; playerList.Count > i; i++)
        {
            Unit unit = playerList[i].GetComponent<Unit>();
            if (unit.select)
            {
                unit.PinMove(new Vector3(unit.unitInfo.transform.position.x, 0));
            }
        }
    }
    public void Back()
    {
        for (int i = 0; playerList.Count > i; i++)
        {
            Unit unit = playerList[i].GetComponent<Unit>();
            if (unit.select)
            {
                unit.PinMove(new Vector3(unit.unitInfo.transform.position.x, -61.5f));
                if (pun && !PhotonNetwork.IsMasterClient) unit.PinMove(new Vector3(unit.unitInfo.transform.position.x, 61.5f));
            }
        }
    }
    public void Stop()
    {
        for (int i = 0; playerList.Count > i; i++)
        {
            Unit unit = playerList[i].GetComponent<Unit>();
            if (unit.select)
            {
                unit.PinMove(unit.unitInfo.transform.position);
            }
        }
    }
    void BattleSe(int battleNum)
    {
        if (battleNum > 0) battleNum += 1;
        if (battleNum > 5) battleNum = 5;
        int random10 = UnityEngine.Random.Range(0, 9);
        int seNum = battleNum + random10;

        if (seNum > 10)
        {
            int random5 = UnityEngine.Random.Range(0, 4);
            switch (random5)
            {
                case 0:
                    SE(SEPath.BATTLE0, 0.5f);
                    break;
                case 1:
                    SE(SEPath.BATTLE1, 0.5f);
                    break;
                case 2:
                    SE(SEPath.BATTLE2, 0.5f);
                    break;
                case 3:
                    SE(SEPath.BATTLE3, 0.5f);
                    break;
                case 4:
                    SE(SEPath.BATTLE4, 0.5f);
                    break;
            }
        }
    }
    public IEnumerator DeathCutin(Unit unit)
    {
        if (pun && PhotonNetwork.IsMasterClient) tactics.punManager.DeathCutin(unit);
        cutinBack.SetActive(true);
        unit.chara.gameObject.SetActive(true);
        float distanse = (unit.unitInfo.transform.position).magnitude;
        for (int i = 0; i < 10; i++)
        {
            unit.chara.transform.position = Vector3.MoveTowards(unit.chara.transform.position, new Vector3(0, 0, 0), distanse / 10);
            unit.chara.GetComponent<RectTransform>().sizeDelta = unit.chara.GetComponent<RectTransform>().sizeDelta + new Vector2(10, 10);
            unit.chara.color = unit.chara.color + new Color(0, 0, 0, 0.05f);
            yield return new WaitForSecondsRealtime(0.02f);
        }
        deathAnime.gameObject.SetActive(true);
        deathAnime.speed = 0.5f;
        deathAnime.Play("血しぶき");
        int j = 0;
        SEManager.Instance.Play(SEPath.DEATH);
        while (deathAnime.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f || j < 10)
        {
            if (j < 10) unit.chara.color = new Color(1, 1 - 0.1f * j, 1 - 0.1f * j);
            yield return new WaitForSeconds(0.02f);
            j++;
        }
        deathAnime.gameObject.SetActive(false);
        for (int i = 0; i < 10; i++)
        {
            unit.chara.transform.position = Vector3.MoveTowards(unit.chara.transform.position, unit.unitInfo.transform.position, distanse / 10);
            unit.chara.GetComponent<RectTransform>().sizeDelta = unit.chara.GetComponent<RectTransform>().sizeDelta + new Vector2(-10, -10);
            unit.chara.color = unit.chara.color + new Color(0, 0, 0, -0.05f);
            yield return new WaitForSecondsRealtime(0.02f);
        }
        unit.chara.transform.position = unit.unitInfo.transform.position;
        if (!unit.select) unit.chara.gameObject.SetActive(false);
        cutinBack.SetActive(false);
        if (pun)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                while (tactics.punManager.cutinSync == -1)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else
            {
                tactics.punManager.CutinSync();
            }
        }
    }
    void DfToNow(Unit unit)
    {
        unit.jobNow = unit.jobDf;
        unit.actTypeNow = unit.actTypeDf;
        unit.hpMaxNow = unit.hpMaxDf;
        unit.powerNow = unit.powerDf;
        unit.magicNow = unit.magicDf;
        unit.skillNow = unit.skillDf;
        unit.speedNow = unit.speedDf;
        unit.breakNow = unit.breakDf;
        unit.breakSpeedNow = unit.breakSpeedDf;
        unit.reviveNow = unit.reviveDf;
        unit.reviveHpNow = unit.reviveHpDf;
        unit.hpMaxNow = unit.hpMaxDf;
        unit.atkNow = unit.atkDf;
        unit.defNow = unit.defDf;
        unit.mDefNow = unit.mDefDf;
        unit.actSpeedNow = unit.actSpeedDf;
        unit.healSpeedNow = unit.healSpeedDf;
    }
    public IEnumerator UnitDeath(Unit unit)
    {
        if (!unit.noRevival && !unit.cpu) yield return DeathCutin(unit);
        unit.hpNow = -unit.reviveNow * 100;
        unit.death = true;
        unit.heal = true;
        if (!unit.enemy) unit.unitInfo.transform.position = new Vector3(unit.unitInfo.transform.position.x, -61.5f);
        if (!unit.enemy) unit.pin.transform.position = new Vector3(unit.unitInfo.transform.position.x, -61.5f);
        if (unit.enemy) unit.unitInfo.transform.position = new Vector3(unit.unitInfo.transform.position.x, 61.5f);
        if (unit.enemy) unit.pin.transform.position = new Vector3(unit.unitInfo.transform.position.x, 61.5f);
        if (!debug && unit.enemy) unit.unitInfo.SetActive(false);
        if (unit.jobNow == 1) unit.trail.enabled = false;
        unit.unitInfo.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        //noRevivulのオブジェクト削除判定なし
    }
    void UnitActCheack()
    {
        int battleSeNum = 0;
        //初期化
        for (int a = 0; a < 2; a++)
        {
            List<GameObject> unitLists = null;
            if (a == 0) unitLists = unitList;
            if (a == 1) unitLists = unitListEnemy;
            for (int i = 0; unitLists.Count > i; i++)
            {
                //初期化
                Unit unit = unitLists[i].GetComponent<Unit>();
                unit.ring.color = new Color(255 / 255, 255 / 255, 255 / 255);
                unit.breakCheck = false;
                unit.stopCheak = false;
                unit.breakSpeedPer = 1;
                unit.speedUpFix = 0;
                unit.actTarget = new List<Unit>();
                unit.battleTarget = new List<Unit>();
                unit.jobArea.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                foreach (GameObject area in unit.actArea) area.SetActive(false);
                unit.ice -= 0.01f * frameRate;
                if (unit.ice < 0) unit.ice = 0;
            }
        }
        //判定
        for (int a = 0; a < 2; a++)
        {
            List<GameObject> unitLists = null;
            List<GameObject> unitListsEnemy = null;
            if (a == 0) unitLists = unitList;
            if (a == 1) unitLists = unitListEnemy;
            if (a == 0) unitListsEnemy = unitListEnemy;
            if (a == 1) unitListsEnemy = unitList;
            for (int i = 0; unitLists.Count > i; i++)
            {
                Unit unit = unitLists[i].GetComponent<Unit>();
                if (!unit.heal)
                {
                    if (a == 0 && unit.unitInfo.transform.position.y >= 49.5 && Math.Abs(unit.unitInfo.transform.position.x) <= 60) unit.breakCheck = true;
                    if (a == 1 && unit.unitInfo.transform.position.y <= -49.5 && Math.Abs(unit.unitInfo.transform.position.x) <= 60) unit.breakCheck = true;
                    if (unit.unitInfo.transform.position == unit.pin.transform.position) unit.stopCheak = true;
                    for (int j = 0; unitListsEnemy.Count > j; j++)
                    {
                        Unit unitEnemy = unitListsEnemy[j].GetComponent<Unit>();
                        float distanse = (unit.unitInfo.transform.position - unitEnemy.unitInfo.transform.position).magnitude;
                        if (!unitEnemy.heal && distanse <= Constants.unitSizeFix * (unit.unitSize + unitEnemy.unitSize) / 2 - 1)
                        {
                            unit.ring.color = new Color(255 / 255, 0 / 255, 0 / 255);
                            unit.battleTarget.Add(unitEnemy);
                            battleSeNum++;
                            if (unit.jobNow == 1 && unit.skillAct >= 1)
                            {
                                unit.ring.color = new Color(0 / 255, 0 / 255, 255 / 255);
                                unit.actTarget.Add(unitEnemy);
                                unit.skillAct = 0;
                            }
                        }
                    }
                    if ( (unit.jobNow == 2 && unit.battleTarget.Count == 0) && !unit.breakCheck || (unit.jobNow == 2 && unit.battleAct>0))
                    {
                        float spireLong = 18 + unit.skillNow * 1.2f - Constants.unitSizeFix;
                        unit.jobArea.GetComponent<RectTransform>().sizeDelta = new Vector2(spireLong / unit.unitSize + Constants.unitSizeFix, spireLong / unit.unitSize + Constants.unitSizeFix);
                        for (int j = 0; unitListsEnemy.Count > j; j++)
                        {
                            Unit unitEnemy = unitListsEnemy[j].GetComponent<Unit>();
                            float distanse = (unit.jobArea.transform.position - unitEnemy.unitInfo.transform.position).magnitude;
                            if (!unitEnemy.heal && distanse <= (Constants.unitSizeFix * (unit.unitSize + unitEnemy.unitSize) + spireLong) /2f)
                            {
                                unit.ring.color = new Color(0 / 255, 0 / 255, 255 / 255);
                                StartCoroutine(unit.SpireAnime(unitEnemy.unitInfo.transform.position, j, distanse, spireLong));
                                unit.actTarget.Add(unitEnemy);
                            }
                        }
                    }
                    if ((unit.jobNow == 3 && unit.battleTarget.Count == 0) && !unit.breakCheck || (unit.jobNow == 3 && unit.battleAct > 0))
                    {
                        float arrowLong = 60 + unit.skillAct * 3f - Constants.unitSizeFix;
                        unit.jobArea.GetComponent<RectTransform>().sizeDelta = new Vector2(arrowLong / unit.unitSize + Constants.unitSizeFix, arrowLong / unit.unitSize + Constants.unitSizeFix);
                        unit.jobArea2.GetComponent<RectTransform>().sizeDelta = new Vector2((arrowLong - (arrowLong * arrowLong * arrowLong - (arrowLong * arrowLong * (arrowLong - 0.0008f)))) / unit.unitSize + Constants.unitSizeFix, (arrowLong - (arrowLong * arrowLong * arrowLong - (arrowLong * arrowLong * (arrowLong - 0.0008f)))) / unit.unitSize +  Constants.unitSizeFix);
                        if (unit.stopCheak)
                        {
                            for (int k = 0; k < unit.arrowTargetNum; k++)
                            {
                                Unit targetUnit = null;
                                float targetDistanse = 0;
                                float targetDirection = 0;
                                for (int j = 0; unitListsEnemy.Count > j; j++)
                                {
                                    Unit unitEnemy = unitListsEnemy[j].GetComponent<Unit>();
                                    bool already = false;
                                    foreach (Unit alreadyTarget in unit.actTarget)
                                    {
                                        if (unitEnemy.unitNum == alreadyTarget.unitNum)
                                        {
                                            already = true;
                                            break;
                                        }
                                    }
                                    if (already) continue;

                                    float distanse = (unit.jobArea.transform.position - unitEnemy.unitInfo.transform.position).magnitude;
                                    if (!unitEnemy.heal && distanse <= (Constants.unitSizeFix * (unit.unitSize + unitEnemy.unitSize) + arrowLong) / 2f - 2)
                                    {
                                        float direction = Math.Abs(unit.arrowDirection + Mathf.Atan2(unit.unitInfo.transform.position.x - unitEnemy.unitInfo.transform.position.x, unit.unitInfo.transform.position.y - unitEnemy.unitInfo.transform.position.y) * Mathf.Rad2Deg);
                                        if (direction > 180) direction = Math.Abs(360 - direction);

                                        bool search = false;
                                        foreach (int target in unit.arrowTarget)
                                        {
                                            if (target == unitEnemy.unitNum)
                                            {
                                                search = true;
                                                break;
                                            }
                                        }
                                        if (search)
                                        {
                                            targetUnit = unitEnemy;
                                            break;
                                        }
                                        if (targetUnit == null)
                                        {
                                            targetUnit = unitEnemy;
                                            targetDistanse = distanse;
                                            targetDirection = direction;
                                        }
                                        else if (targetDirection > direction)
                                        {
                                            targetUnit = unitEnemy;
                                            targetDirection = direction;
                                        }
                                    }
                                }
                                if (targetUnit != null)
                                {
                                    unit.arrowTarget.Add(targetUnit.unitNum);
                                    unit.ring.color = new Color(0 / 255, 0 / 255, 255 / 255);
                                    unit.actTarget.Add(targetUnit);
                                    if (frame % (20 / frameRate) == 0) unit.ArrowAnime(targetUnit.unitInfo.transform.position, targetDistanse);
                                }
                            }
                        }
                        if (unit.actTarget.Count == 0) unit.arrowTarget = new List<int>();
                    }
                    if (unit.jobNow == 5 && unit.actCoolTime == 0 && unit.skillAct >= 1 && unit.battleTarget.Count == 0)
                    {
                        int actLevel = (int)Math.Ceiling(float.Parse(unit.skill.text) / 2);
                        switch (unit.actTypeNow)
                        {
                            case "連射":
                                unit.actArea[1].SetActive(true);
                                unit.actArea[1].GetComponent<RectTransform>().sizeDelta = new Vector2(5 / unit.unitSize, (20 + 3 * actLevel) / unit.unitSize);
                                break;
                            case "拡散":
                                unit.actWideAngle.transform.localRotation = Quaternion.Euler(0, 0, -10 * (actLevel - 1));
                                for (int j = 0; j < actLevel; j++)
                                {
                                    unit.actArea[j + 2].GetComponent<RectTransform>().sizeDelta = new Vector2(5 / unit.unitSize, (20 + 2 * actLevel) / unit.unitSize);
                                    unit.actArea[j + 2].SetActive(true);
                                }
                                break;
                            case "貫通":
                                unit.actArea[1].SetActive(true);
                                unit.actArea[1].GetComponent<RectTransform>().sizeDelta = new Vector2(5 / unit.unitSize, (20 + 5 * actLevel) / unit.unitSize);
                                break;
                            case "爆発":
                                unit.actArea[1].SetActive(true);
                                unit.actArea[1].GetComponent<RectTransform>().sizeDelta = new Vector2(5 / unit.unitSize, (20 + 3 * actLevel) / unit.unitSize);
                                break;
                        }
                    }
                    if (unit.jobNow == 3 && unit.actCoolTime == 0 && unit.skillAct >= 1 && unit.battleTarget.Count == 0 && unit.actTypeNow != "")
                    {
                        switch (unit.actTypeNow)
                        {
                            case "砲撃":
                                unit.actArea[0].SetActive(true);
                                unit.actArea[0].GetComponent<RectTransform>().sizeDelta = new Vector2((20 + 2 * unit.skillAct) / unit.unitSize, (20 + 2 * unit.skillAct) / unit.unitSize);
                                unit.actArea[0].transform.localPosition = new Vector3(0, (60 + unit.skillAct * 3f) / 2, 0) / unit.unitSize;
                                break;
                        }
                    }
                }
                if (unit.actCoolTime > 0) unit.ring.color = new Color(255 / 255, 255 / 255, 0 / 255);
                unit.actCoolTime -= 0.01f * frameRate;
                if (unit.actCoolTime < 0) unit.actCoolTime = 0;
                if (unit.jobNow == 5 && unit.actCoolTime == 0)
                {
                    if (unit.battleTarget.Count != 0 || unit.breakCheck && unit.stopCheak) {
                        unit.skillAct += 0.004f * (unit.actSpeedNow/2 + unit.skillNow * 5) / 100 * frameRate;
                    }
                    else
                    {
                        unit.skillAct += 0.004f * (unit.actSpeedNow + unit.skillNow * 5) / 100 * frameRate;
                    }
                    if (unit.skillAct > unit.skillNow) unit.skillAct = unit.skillNow;
                    unit.skill.text = Math.Floor(unit.skillAct).ToString();
                }
            }
        }
        foreach (Unit unit in actUnit)
        {
            if (unit.actCoolTime == 0 && unit.skillAct >= 1 && unit.battleTarget.Count == 0 && unit.actTypeNow != "" && !unit.heal && !unit.breakCheck)
            {
                unit.ring.color = new Color(0 / 255, 0 / 255, 255 / 255);
                if (unit.jobNow == 5)
                {
                    unit.actCoolTime = 1f;
                    int actLevel = (int)Math.Ceiling(float.Parse(unit.skill.text) / 2);
                    unit.skillAct -= actLevel;
                    unit.skill.text = Math.Floor(unit.skillAct).ToString();
                    switch (unit.actTypeNow)
                    {
                        case "連射":
                            for (int i = 0; i < actLevel; i++)
                            {
                                ActObject actObj;
                                if (pun)
                                {
                                    actObj = PhotonNetwork.Instantiate("魔弾", new Vector3(999, 999, 0), Quaternion.identity).GetComponent<ActObject>();
                                }
                                else
                                {
                                    actObj = (Instantiate(unit.shoot[0], unit.shoot[0].transform.parent) as GameObject).GetComponent<ActObject>();
                                }
                                actObj.enemy = unit.enemy;
                                actObj.unitNum = unit.unitNum;
                                actObj.lebel = actLevel;
                                actObj.power = unit.powerNow;
                                actObj.magic = unit.magicNow;
                                actObj.atk = unit.atkNow;
                                actObj.rate = 1;
                                actObj.speed = 350;
                                actObj.type = 1;
                                actObj.targetPoint = unit.actArea[1].transform.GetChild(0).position;
                                actObj.ColorChange(unit.card.color);
                                actObj.stopTime = (i + 0.6f) / actLevel;
                                actObj.transform.position = unit.unitInfo.transform.position;
                                actObj.transform.rotation = unit.jobDirection.transform.rotation;
                                actObj.gameObject.SetActive(true);
                                actObj.tacticsID = unit.actTacticsID;
                                actList.Add(actObj);
                                SE(SEPath.MAGIC, 1);
                            }
                            break;
                        case "拡散":
                            unit.actCoolTime -= 0.5f;
                            for (int i = 0; i < actLevel; i++)
                            {
                                ActObject actObj;
                                if (pun)
                                {
                                    actObj = PhotonNetwork.Instantiate("魔弾", new Vector3(999, 999, 0), Quaternion.identity).GetComponent<ActObject>();
                                }
                                else
                                {
                                    actObj = (Instantiate(unit.shoot[0], unit.shoot[0].transform.parent) as GameObject).GetComponent<ActObject>();
                                }
                                actObj.enemy = unit.enemy;
                                actObj.unitNum = unit.unitNum;
                                actObj.lebel = actLevel;
                                actObj.power = unit.powerNow;
                                actObj.magic = unit.magicNow;
                                actObj.atk = unit.atkNow;
                                actObj.rate = 0.9f + 0.1f * actLevel;
                                actObj.speed = 350;
                                actObj.type = 1;
                                actObj.targetPoint = unit.actArea[i + 2].transform.GetChild(0).position;
                                actObj.ColorChange(unit.card.color);
                                actObj.stopTime = 0.2f;
                                actObj.transform.position = unit.unitInfo.transform.position;
                                actObj.transform.rotation = unit.jobDirection.transform.rotation;
                                actObj.transform.Rotate(0, 0, ((-0.5f) * (actLevel - 1) + ((actLevel - 1) - i)) * (-20));
                                actObj.gameObject.SetActive(true);
                                actObj.tacticsID = unit.actTacticsID;
                                actList.Add(actObj);
                                SE(SEPath.MAGIC, 1);
                            }
                            break;
                        case "貫通":
                            {
                                ActObject actObj;
                                if (pun)
                                {
                                    actObj = PhotonNetwork.Instantiate("魔弾", new Vector3(999, 999, 0), Quaternion.identity).GetComponent<ActObject>();
                                }
                                else
                                {
                                    actObj = (Instantiate(unit.shoot[0], unit.shoot[0].transform.parent) as GameObject).GetComponent<ActObject>();
                                }
                                actObj.enemy = unit.enemy;
                                actObj.unitNum = unit.unitNum;
                                actObj.lebel = actLevel;
                                actObj.power = unit.powerNow;
                                actObj.magic = unit.magicNow;
                                actObj.atk = unit.atkNow;
                                actObj.rate = 0.6f * actLevel;
                                actObj.speed = 650 + 50*actLevel;
                                actObj.type = 2;
                                actObj.targetPoint = unit.actArea[1].transform.GetChild(0).position;
                                actObj.ColorChange(unit.card.color);
                                actObj.stopTime = 0.7f;
                                actObj.transform.position = unit.unitInfo.transform.position;
                                actObj.transform.rotation = unit.jobDirection.transform.rotation;
                                actObj.gameObject.SetActive(true);
                                actObj.tacticsID = unit.actTacticsID;
                                actList.Add(actObj);
                                SE(SEPath.MAGIC, 1);
                            }
                            break;
                        case "爆発":
                            {
                                unit.actCoolTime -= 0.5f;
                                ActObject actObj;
                                if (pun)
                                {
                                    actObj = PhotonNetwork.Instantiate("魔弾", new Vector3(999, 999, 0), Quaternion.identity).GetComponent<ActObject>();
                                }
                                else
                                {
                                    actObj = (Instantiate(unit.shoot[0], unit.shoot[0].transform.parent) as GameObject).GetComponent<ActObject>();
                                }
                                actObj.enemy = unit.enemy;
                                actObj.unitNum = unit.unitNum;
                                actObj.lebel = actLevel;
                                actObj.power = unit.powerNow;
                                actObj.magic = unit.magicNow;
                                actObj.atk = unit.atkNow;
                                actObj.rate = actLevel * 0.5f;
                                actObj.speed = 500;
                                actObj.type = 3;
                                actObj.targetPoint = unit.actArea[1].transform.GetChild(0).position;
                                actObj.ColorChange(unit.card.color);
                                actObj.stopTime = 0;
                                actObj.transform.position = unit.unitInfo.transform.position;
                                actObj.transform.rotation = unit.jobDirection.transform.rotation;
                                actObj.gameObject.SetActive(true);
                                actObj.tacticsID = unit.actTacticsID;
                                actObj.damageArea = (15 + 3 * actLevel) / 2;
                                actList.Add(actObj);
                                SE(SEPath.MAGIC, 1);
                            }
                            break;
                    }
                }
                if (unit.jobNow == 3)
                {
                    switch (unit.actTypeNow)
                    {
                        case "砲撃":
                            ActObject actObj;
                            if (pun)
                            {
                                actObj = PhotonNetwork.Instantiate("砲弾", new Vector3(999, 999, 0), Quaternion.identity).GetComponent<ActObject>();
                            }
                            else
                            {
                                actObj = (Instantiate(unit.shoot[1], unit.shoot[1].transform.parent) as GameObject).GetComponent<ActObject>();
                            }
                            actObj.enemy = unit.enemy;
                            actObj.unitNum = unit.unitNum;
                            actObj.power = unit.powerNow;
                            actObj.magic = unit.magicNow;
                            actObj.atk = unit.atkNow;
                            actObj.rate = int.Parse(unit.skill.text);
                            actObj.type = 11;
                            actObj.speed = 0;
                            actObj.damageArea = unit.actArea[0].GetComponent<RectTransform>().sizeDelta.x / 2;
                            actObj.startPoint = unit.unitInfo.transform.position;
                            actObj.targetPoint = unit.actArea[0].transform.position;
                            actObj.halfPoint = actObj.targetPoint - actObj.startPoint * 0.50f + actObj.startPoint;
                            actObj.halfPoint.z += -unit.actArea[0].transform.localPosition.y * 0.2f;
                            actObj.stopTime = 0;
                            actObj.transform.position = unit.unitInfo.transform.position;
                            actObj.transform.rotation = unit.jobDirection.transform.rotation;
                            actObj.gameObject.SetActive(true);
                            actObj.tacticsID = unit.actTacticsID;
                            actList.Add(actObj);
                            SE(SEPath.CANON, 0.5f);
                            break;
                    }
                    unit.actCoolTime = 2f;
                    unit.skillAct -= 1;
                    unit.skill.text = Math.Floor(unit.skillAct).ToString();

                }
            }
        }
        if (frame % 10 == 0) BattleSe(battleSeNum);
    }
    public void SE(string path, float volume)
    {
        SEManager.Instance.Play(path, volume);
        if (pun) punAudio.PunSE(path, volume);
    }
    public void BGM(string path, float volume)
    {
        BGMManager.Instance.Play(path, volume);
        if (pun) punAudio.PunBGM(path, volume);
    }
    public void BGMVolume(float volume)
    {
        BGMManager.Instance.ChangeBaseVolume(volume);
        if (pun) punAudio.PunBGMVolume(volume);
    }
    public void Act(Unit unit)
    {
        actUnit.Add(unit);
    }
    Vector3 CalcLerpPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        var a = Vector3.Lerp(p0, p1, t);
        var b = Vector3.Lerp(p1, p2, t);
        return Vector3.Lerp(a, b, t);
    }
    public void DisplayHP(Unit unit)
    {
        if (unit.hpNow <= 0)
        {
            unit.hp100.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 3);
            unit.hp200.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 3);
        }
        else if (unit.hpNow <= 10000) {
            unit.hp100.GetComponent<RectTransform>().sizeDelta = new Vector2(unit.hpNow / 100, 3);
            unit.hp200.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 3);
        }
        else
        {
            unit.hp100.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 3);
            unit.hp200.GetComponent<RectTransform>().sizeDelta = new Vector2((unit.hpNow - 10000) / 2000, 3);
        }
    }
    void Result()
    {
        if (!pun)
        {
            if (hp.GetComponent<RectTransform>().sizeDelta.x > hp2.GetComponent<RectTransform>().sizeDelta.x)
            {
                tactics.result[0].text = "Win";
                tactics.result[1].text = "Lose";
            }
            else if (hp.GetComponent<RectTransform>().sizeDelta.x < hp2.GetComponent<RectTransform>().sizeDelta.x)
            {
                tactics.result[1].text = "Win";
                tactics.result[0].text = "Lose";
            }
            else
            {
                tactics.result[1].text = "Draw";
                tactics.result[0].text = "Draw";
            }
            tactics.result[0].gameObject.SetActive(true);
            tactics.result[1].gameObject.SetActive(true);
        }
        else
        {
            if (hp.GetComponent<RectTransform>().sizeDelta.x > hp2.GetComponent<RectTransform>().sizeDelta.x)
            {
                punManager.Result("Win", "Lose");
            }
            else if (hp.GetComponent<RectTransform>().sizeDelta.x < hp2.GetComponent<RectTransform>().sizeDelta.x)
            {
                punManager.Result("Lose", "Win");
            }
            else
            {
                punManager.Result("Draw", "Draw");
            }
        }
    }
    void CrystalCreate()
    {
        
        float seconds = frame * frameRate;
        if (seconds % 2000 == 0 && seconds % 4000 != 0)
        {
            int randomX = UnityEngine.Random.Range(30, 50);
            int randomY = UnityEngine.Random.Range(20, 35);
            int minus;
            if(UnityEngine.Random.Range(0, 2) == 0)
            {
                minus = 1;
            }
            else
            {
                minus = -1;
            }
            CrystalCreate2(true, new Vector3(randomX * minus, randomY, 0));
            CrystalCreate2(false, new Vector3(-randomX * minus, -randomY, 0));
        }
        if (seconds % 4000 == 0)
        {
            int randomX = UnityEngine.Random.Range(30, 50);
            int randomY = UnityEngine.Random.Range(20, 35);
            int minus;
            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                minus = 1;
            }
            else
            {
                minus = -1;
            }
            CrystalCreate2(false, new Vector3(randomX * minus, randomY, 0));
            CrystalCreate2(true, new Vector3(randomX * minus, -randomY, 0));
        }
        if (seconds % 3000 == 0 && seconds % 6000 != 0)
        {
            CrystalCreate2(false, new Vector3(0, 0, 0));
        }
        if (seconds % 6000 == 0)
        {
            CrystalCreate2(true, new Vector3(0, 0, 0));
        }
    }
    void CrystalCreate2(bool enemy,Vector3 positon)
    {
        Crystal crystal;
        if (pun)
        {
            crystal = PhotonNetwork.Instantiate("宝石", new Vector3(999, 999, 0), Quaternion.identity).GetComponent<Crystal>();
        }
        else
        {
            crystal = (Instantiate(crystalPrefab, effectListObject.transform) as GameObject).GetComponent<Crystal>();
        }
        crystal.SetPlayer(enemy);
        crystal.transform.position = positon;
        crystalList.Add(crystal.gameObject);
    }
}