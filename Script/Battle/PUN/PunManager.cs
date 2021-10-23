using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using System;

public class PunManager : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback, IPunObservable
{
    bool start = false;
    GameObject hp;
    GameObject hp2;
    Text time;
    GameObject[] tacticsTimeUnit = new GameObject[6];
    GameObject[] tacticsTimeName = new GameObject[6];
    GameObject[] tacticsTimeCount = new GameObject[6];
    Tactics tactics;
    Commander commander;
    Stage stage;
    public int cutinSync;
    bool match = false;
    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
    {
        stage = GameObject.Find("Stage").GetComponent<Stage>();
        hp = stage.hp;
        hp2 = stage.hp2;
        time = GameObject.Find("BattleTime").GetComponent<Text>();
        tactics = GameObject.Find("Righter").GetComponent<Tactics>();
        tactics.punManager = this;
        commander = GameObject.Find("Righter").GetComponent<Commander>();
        commander.punManager = this;
        GameObject[] skillInfo = new GameObject[6];
        skillInfo[0] = GameObject.Find("SkillA1");
        skillInfo[1] = GameObject.Find("SkillA2");
        skillInfo[2] = GameObject.Find("SkillA3");
        skillInfo[3] = GameObject.Find("SkillB1");
        skillInfo[4] = GameObject.Find("SkillB2");
        skillInfo[5] = GameObject.Find("SkillB3");
        for (int i = 0; i < skillInfo.Length; i++)
        {
            tacticsTimeName[i] = skillInfo[i].transform.GetChild(0).gameObject;
            tacticsTimeUnit[i] = skillInfo[i].transform.GetChild(1).gameObject;
            tacticsTimeCount[i] = skillInfo[i].transform.GetChild(2).gameObject;
        }

        photonView.RPC(nameof(RPCName), RpcTarget.Others, PlayerPrefs.GetString("playerName"));
        if (info.photonView.IsMine)
        {

        }
        else
        {
            photonView.RPC(nameof(RPCStart), RpcTarget.MasterClient, stage.deck,commander.commandNo);
        }
        start = true;
    }
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (start)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(hp.GetComponent<RectTransform>().sizeDelta);
                stream.SendNext(hp2.GetComponent<RectTransform>().sizeDelta);
                stream.SendNext(time.text);
                stream.SendNext(tactics.tacticsPointNowEnemy);
                foreach (GameObject name in tacticsTimeName)
                {
                    stream.SendNext(name.GetComponent<Text>().text);
                }
                foreach (GameObject count in tacticsTimeCount)
                {
                    stream.SendNext(count.GetComponent<Text>().text);
                }
                foreach (GameObject unit in tacticsTimeUnit)
                {
                    stream.SendNext(unit.activeSelf);
                    stream.SendNext(unit.GetComponent<Image>().sprite.name);
                }
            }
            else
            {
                hp.GetComponent<RectTransform>().sizeDelta = (Vector2)stream.ReceiveNext();
                hp2.GetComponent<RectTransform>().sizeDelta = (Vector2)stream.ReceiveNext();
                time.text = (string)stream.ReceiveNext();
                tactics.tacticsPointNowEnemy = (float)stream.ReceiveNext();
                tactics.tacticsPoint.text = (Math.Floor(tactics.tacticsPointNowEnemy * 10) / 10).ToString();
                foreach (GameObject name in tacticsTimeName)
                {
                    name.GetComponent<Text>().text = (string)stream.ReceiveNext();
                }
                foreach (GameObject count in tacticsTimeCount)
                {
                    count.GetComponent<Text>().text = (string)stream.ReceiveNext();
                }
                foreach (GameObject unit in tacticsTimeUnit)
                {
                    unit.SetActive((bool)stream.ReceiveNext());
                    string colorNo = (string)stream.ReceiveNext();
                    if (unit.activeSelf && unit.GetComponent<Image>().sprite.name != colorNo) StartCoroutine(LoadIllust(unit, colorNo));
                }
            }
        }
    }
    IEnumerator LoadIllust(GameObject unit, string colorNo)
    {
        var illust = Addressables.LoadAssetAsync<Sprite>(colorNo);
        yield return illust;
        unit.GetComponent<Image>().sprite = illust.Result;
    }
    public void PunTacticsActivate(Unit unit, List<Unit> target)
    {
        //List<int> targetNum = new List<int>();
        //List<int> targetNumEnemy = new List<int>();
        int[] targetNum = new int[target.Count];
        bool[] targetEnemy = new bool[target.Count];
        for (int i = 0; i < target.Count; i++)
        {
            targetNum[i] = target[i].unitNum;
            targetEnemy[i] = target[i].enemy;
        }
        photonView.RPC(nameof(RPCTacticsActivate), RpcTarget.MasterClient, unit.unitNum, targetNum, targetEnemy);
    }
    [PunRPC]
    private void RPCTacticsActivate(int unitNum, int[] targetNum, bool[] targetEnemy)
    {
        tactics.PunEnemyActivate(unitNum, targetNum, targetEnemy);
    }
    public void TacticsCostUpdate()
    {
        photonView.RPC(nameof(RPCTacticsCostUpdate), RpcTarget.Others);
    }
    [PunRPC]
    private void RPCTacticsCostUpdate()
    {
        tactics.TacticsPossibleButton();
        tactics.UnLock();
    }
    public void DeathCutin(Unit unit)
    {
        cutinSync = -1;
        photonView.RPC(nameof(RPCDeathCutin), RpcTarget.Others, unit.enemy, unit.unitNum);
    }
    [PunRPC]
    public void RPCDeathCutin(bool enemy, int unitNum)
    {
        Unit unit;
        if (!enemy) unit = tactics.stage.unitList.Find(u => u.GetComponent<Unit>().unitNum == unitNum).GetComponent<Unit>();
        else unit = tactics.stage.unitListEnemy.Find(u => u.GetComponent<Unit>().unitNum == unitNum).GetComponent<Unit>();
        StartCoroutine(tactics.stage.DeathCutin(unit));
    }
    public void CutinSync()
    {
        photonView.RPC(nameof(RPCCutinSync), RpcTarget.MasterClient);
    }
    [PunRPC]
    void RPCCutinSync()
    {
        cutinSync = 1;
    }
    public void TacticsCutin(Unit unit)
    {
        cutinSync = -1;
        photonView.RPC(nameof(RPCTacticsCutin), RpcTarget.Others, unit.enemy, unit.unitNum);
    }
    [PunRPC]
    void RPCTacticsCutin(bool enemy, int unitNum)
    {
        Unit unit;
        if (!enemy) unit = tactics.stage.unitList.Find(u => u.GetComponent<Unit>().unitNum == unitNum).GetComponent<Unit>();
        else unit = tactics.stage.unitListEnemy.Find(u => u.GetComponent<Unit>().unitNum == unitNum).GetComponent<Unit>();
        StartCoroutine(tactics.TacticsCutin(unit, true, true));
    }
    public void CommandCutin(Command command)
    {
        cutinSync = -1;
        photonView.RPC(nameof(RPCCommandCutin), RpcTarget.Others, command.no);
    }
    [PunRPC]
    void RPCCommandCutin(int no)
    {
        Debug.Log("");
        Command command = commander.commandMaster.CommandList.Find(c => c.no == no);
        StartCoroutine(commander.CommandCutin(command, true, true));
    }
    public void CommandName(int i,string name)
    {
        photonView.RPC(nameof(RPCCommandName), RpcTarget.Others, i,name);
    }
    [PunRPC]
    void RPCCommandName(int i,string name)
    {
        commander.commandName[i].text = name;
    }
    public void Command(int no)
    {
        photonView.RPC(nameof(RPCCommand), RpcTarget.MasterClient, no);
    }
    [PunRPC]
    void RPCCommand(int no)
    {
        commander.PunCommand(no);
    }
    public void CommandEnable(int i)
    {
        photonView.RPC(nameof(RPCCommandEnable), RpcTarget.Others, i);
    }
    [PunRPC]
    void RPCCommandEnable(int i)
    {
        if(commander.activate==0) commander.commandButton[i].interactable = true;
    }
    public void CommandDisable(int i)
    {
        photonView.RPC(nameof(RPCCommandDisable), RpcTarget.Others, i);
    }
    [PunRPC]
    void RPCCommandDisable(int i)
    {
        commander.commandButton[i].interactable = false;
    }
    public void CommandUsed()
    {
        photonView.RPC(nameof(RPCCommandUsed), RpcTarget.Others);
    }
    [PunRPC]
    void RPCCommandUsed()
    {
        commander.used.SetActive(true);
    }
    public void BattleStart()
    {
        photonView.RPC(nameof(RPCBattleStart), RpcTarget.Others);
    }
    [PunRPC]
    void RPCBattleStart()
    {
        stage.enemyStage[3].SetActive(false);
        tactics.buttonStop.SetActive(false);
        stage.start = true;
    }
    public void Result(string a,string b)
    {
        photonView.RPC(nameof(RPCResult), RpcTarget.All,a,b);
    }
    [PunRPC]
    void RPCResult(string a, string b)
    {
        tactics.result[0].text = a;
        tactics.result[1].text = b;
        tactics.result[0].gameObject.SetActive(true);
        tactics.result[1].gameObject.SetActive(true);
    }
    [PunRPC]
    void RPCStart(string[] unitArray,int[] commandArray)
    {
        if (!match)
        {
            match = true;
            stage.deckEnemy = unitArray;
            commander.commandNoEnemy = commandArray;
            commander.PunCommandInitial();
            StartCoroutine(stage.Prepare());
        }
    }
    public void RealTimeStart()
    {
        photonView.RPC(nameof(RPCRealTimeStart), RpcTarget.All);
    }
    [PunRPC]
    void RPCRealTimeStart()
    {
        tactics.buttonStop.SetActive(false);
    }
    [PunRPC]
    void RPCName(string name)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (tactics.playerRed.text =="ê‘åR") tactics.playerRed.text = PlayerPrefs.GetString("playerName");
            if (tactics.playerBlue.text == "ê¬åR") tactics.playerBlue.text = name;
        }
        else
        {
            if (tactics.playerRed.text == "ê‘åR") tactics.playerRed.text = name;
            if (tactics.playerBlue.text == "ê¬åR") tactics.playerBlue.text = PlayerPrefs.GetString("playerName");
        }
    }
}
