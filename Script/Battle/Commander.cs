using KanKikuchi.AudioManager;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Commander : MonoBehaviour
{
    public Stage stage;
    public Tactics tactics;
    public int[] commandNo;
    public int[] commandNoEnemy;
    public Text[] commandName;
    public Button[] commandButton;
    public CommandMaster commandMaster;
    public int activate;
    public int activateEnemy;
    public float activateTime;
    public float activateTimeEnemy;
    public PunManager punManager;
    public GameObject used;

    public Fade commandFade;
    public GameObject commandGroup;
    public Text cutinName;
    public Text cutinTime;
    public Text cutinText;
    public GameObject cutinBack;


    public void CommandInitial()
    {
        for (int i = 0; i < commandNo.Length; i++)
        {
            Command command = commandMaster.CommandList.Find(c => c.no == commandNo[i]);
            commandName[i].text = command.name;
            CommandPassive(commandNo[i], false);
        }
        for (int i = 0; i < commandNoEnemy.Length; i++)
        {
            Command command = commandMaster.CommandList.Find(c => c.no == commandNoEnemy[i]);
            CommandPassive(commandNoEnemy[i], true);
        }
    }
    public void PunCommandInitial()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < commandNoEnemy.Length; i++)
            {
                Command command = commandMaster.CommandList.Find(c => c.no == commandNoEnemy[i]);
                punManager.CommandName(i, command.name);
            }
        }
    }
    public void ActivateButtun0()
    {
        foreach (Button button in commandButton)
        {
            button.interactable = false;
        }
        if (activate == 0) activate = commandNo[0];
        if (stage.pun)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                punManager.Command(commandNo[0]);
            }
        }
    }
    public void ActivateButtun1()
    {
        foreach (Button button in commandButton)
        {
            button.interactable = false;
        }
        if (activate == 0) activate = commandNo[1];
        if (stage.pun)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                punManager.Command(commandNo[1]);
            }
        }
    }
    public void PunCommand(int no)
    {
        if (activateEnemy == 0) activateEnemy = no;
    }
    public IEnumerator CommandUpdate()
    {
        if (activate != 0 && activateTime > 0)
        {
            activateTime -= 0.1f;
            if (activateTime <= 0)yield return CommandEffect(false, false);

        }
        if (activateEnemy != 0 && activateTimeEnemy > 0)
        {
            activateTimeEnemy -= 0.1f;
            if (activateTimeEnemy <= 0)yield return CommandEffect(false, true);
        }

        if (activate == 0)
        {
            if (CommandPossible(commandNo[0], false))
            {
                Command command = commandMaster.CommandList.Find(c => c.no == commandNo[0]);
                if (command.force == "ã≠êß") activate = command.no;
                if (command.force == "îCà”") commandButton[0].interactable = true;
            }
            else
            {
                commandButton[0].interactable = false;
            }
        }
        if (activate == 0)
        {
            if (CommandPossible(commandNo[1], false))
            {
                Command command = commandMaster.CommandList.Find(c => c.no == commandNo[1]);
                if (command.force == "ã≠êß") activate = command.no;
                if (command.force == "îCà”") commandButton[1].interactable = true;
            }
            else
            {
                commandButton[1].interactable = false;
            }
        }
        if (stage.pun)
        {
            if (activateEnemy == 0)
            {
                if (CommandPossible(commandNoEnemy[0], true))
                {
                    Command command = commandMaster.CommandList.Find(c => c.no == commandNoEnemy[0]);
                    if (command.force == "ã≠êß") activateEnemy = command.no;
                    if (command.force == "îCà”") punManager.CommandEnable(0);
                }
                else
                {
                    if (stage.pun) punManager.CommandEnable(0);
                }
            }
            if (activateEnemy == 0)
            {
                if (CommandPossible(commandNoEnemy[1], true))
                {
                    Command command = commandMaster.CommandList.Find(c => c.no == commandNoEnemy[1]);
                    if (command.force == "ã≠êß") activateEnemy = command.no;
                    if (command.force == "îCà”") punManager.CommandEnable(1);
                }
                else
                {
                    if (stage.pun) punManager.CommandEnable(1);
                }
            }
        }
    }
    public bool CommandPossible(int no, bool enemy)
    {
        List<GameObject> playerList;
        if (!enemy) playerList = stage.unitList;
        else playerList = stage.unitListEnemy;
        switch (no)
        {
            case 1:
                {
                    foreach (GameObject obj in playerList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        if (!unit.death) return false;
                    }
                }
                break;
            case 2:
                {
                    float hp = 0;
                    int i = 0;
                    foreach (GameObject obj in playerList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        if (!unit.death)
                        {
                            hp += unit.hpNow;
                            i++;
                        }
                    }
                    if (i > 0)
                    {
                        hp /= i;
                        if (hp > 3500) return false;
                    }
                }
                break;
            case 3:
                {
                    int i = 0;
                    foreach (TacticsNow now in tactics.tacticsNows)
                    {
                        if (now.activateUnit.enemy == enemy) i++;
                    }
                    if (i < 3) return false;
                }
                break;
            case 4:
                {
                    if (!enemy && activateEnemy != -1) return false;
                    if (enemy && activate != -1) return false;
                }
                break;
            case 5:
                {
                    int i = 0;
                    foreach (TacticsNow now in tactics.tacticsNows)
                    {
                        if (now.activateUnit.enemy != enemy) i++;
                    }
                    if (i < 2) return false;
                }
                break;
            case 7:
                {
                    if (!enemy && tactics.tacticsPointNow < 9) return false;
                    if (enemy && tactics.tacticsPointNowEnemy < 9) return false;
                }
                break;
        }
        return true;
    }
    public IEnumerator CommandEffect(bool start, bool enemy)
    {
        int no;
        if (!enemy) no = activate;
        else no = activateEnemy;
        if (no < 0) no *= -1;
        List<GameObject> playerList;
        if (!enemy) playerList = stage.unitList;
        else playerList = stage.unitListEnemy;
        List<GameObject> enemyList;
        if (!enemy) enemyList = stage.unitListEnemy;
        else enemyList = stage.unitList;
        Command command = commandMaster.CommandList.Find(c => c.no == no);

        if (start)
        {
            yield return new WaitForSeconds(0.01f);
            if (!enemy) activateTime = commandMaster.CommandList.Find(c => c.no == no).count;
            else activateTimeEnemy = commandMaster.CommandList.Find(c => c.no == no).count;
            if (!enemy) used.SetActive(true);
            else if(stage.pun)punManager.CommandUsed();
            yield return CommandCutin(command, true, false);
        }

        switch (no)
        {
            case 1:
                if (start)
                {
                    foreach (GameObject obj in playerList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        if (unit.death) unit.hpNow /= 2;
                    }
                }
                break;
            case 2:
                if (start)
                {
                    foreach (GameObject obj in playerList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        if (!unit.death && unit.hpNow < unit.hpMaxNow)
                        {
                            unit.hpNow += 2000;
                            if (unit.hpNow > unit.hpMaxNow) unit.hpNow = unit.hpMaxNow;
                            stage.DisplayHP(unit);
                        }
                    }
                }
                break;
            case 3:
                if (start)
                {
                    foreach (GameObject obj in playerList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        unit.powerNow += 1;
                        unit.magicNow += 1;
                        unit.skillNow += 1;
                        unit.power.text = unit.powerNow.ToString();
                        unit.magic.text = unit.magicNow.ToString();
                        if (unit.jobNow == 1)
                        {
                            if (unit.skillAct > unit.skillNow) unit.skillAct = unit.skillNow;
                            unit.skill.text = Math.Floor(unit.skillAct).ToString();
                        }
                        else if (unit.jobNow == 3)
                        {
                            unit.skillAct += 1;
                            unit.skill.text = Math.Floor(unit.skillAct).ToString();
                        }
                        else if (unit.jobNow == 5)
                        {
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
                    foreach (GameObject obj in playerList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        unit.powerNow -= 1;
                        unit.magicNow -= 1;
                        unit.skillNow -= 1;
                        unit.power.text = unit.powerNow.ToString();
                        unit.magic.text = unit.magicNow.ToString();
                        if (unit.jobNow == 1)
                        {
                            if (unit.skillAct > unit.skillNow) unit.skillAct = unit.skillNow;
                            unit.skill.text = Math.Floor(unit.skillAct).ToString();
                        }
                        else if (unit.jobNow == 3)
                        {
                            unit.skillAct -= 1;
                            unit.skill.text = Math.Floor(unit.skillAct).ToString();
                        }
                        else if (unit.jobNow == 5)
                        {
                            if (unit.skillAct > unit.skillNow) unit.skillAct = unit.skillNow;
                            unit.skill.text = Math.Floor(unit.skillAct).ToString();
                        }
                        else
                        {
                            unit.skill.text = unit.skillNow.ToString();
                        }
                    }
                }
                break;
            case 4:
                if (start)
                {
                    foreach (GameObject obj in playerList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        unit.skillNow += 2;
                        if (unit.jobNow == 1)
                        {
                            if (unit.skillAct > unit.skillNow) unit.skillAct = unit.skillNow;
                            unit.skill.text = Math.Floor(unit.skillAct).ToString();
                        }
                        else if (unit.jobNow == 3)
                        {
                            unit.skillAct += 2;
                            unit.skill.text = Math.Floor(unit.skillAct).ToString();
                        }
                        else if (unit.jobNow == 5)
                        {
                            unit.skillAct += 1;
                            if (unit.skillAct > unit.skillNow) unit.skillAct = unit.skillNow;
                            unit.skill.text = Math.Floor(unit.skillAct).ToString();
                        }
                        else
                        {
                            unit.skill.text = unit.skillNow.ToString();
                        }
                    }
                }
                break;
            case 5:
                if (start)
                {
                    foreach (GameObject obj in playerList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        unit.powerNow += 3;
                        unit.magicNow += 3;
                        unit.power.text = unit.powerNow.ToString();
                        unit.magic.text = unit.magicNow.ToString();
                        if (!unit.death && unit.hpNow < unit.hpMaxNow)
                        {
                            unit.hpNow += 2000;
                            if (unit.hpNow > unit.hpMaxNow) unit.hpNow = unit.hpMaxNow;
                            stage.DisplayHP(unit);
                        }
                    }
                }
                else
                {
                    foreach (GameObject obj in playerList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        unit.powerNow -= 3;
                        unit.magicNow -= 3;
                        unit.power.text = unit.powerNow.ToString();
                        unit.magic.text = unit.magicNow.ToString();
                    }
                }
                break;
            case 6:
                if (start)
                {
                    foreach (GameObject obj in playerList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        unit.powerNow += 2;
                        unit.magicNow += 1;
                        unit.power.text = unit.powerNow.ToString();
                        unit.magic.text = unit.magicNow.ToString();
                    }
                    if (!enemy) tactics.tacticsPointNow += 1;
                    else tactics.tacticsPointNowEnemy += 1;
                }
                else
                {
                    foreach (GameObject obj in playerList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        unit.powerNow -= 2;
                        unit.magicNow -= 1;
                        unit.power.text = unit.powerNow.ToString();
                        unit.magic.text = unit.magicNow.ToString();
                    }
                }
                break;
            case 7:
                if (start)
                {
                    foreach (GameObject obj in playerList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        unit.speedNow += 30;
                    }
                }
                else
                {
                    foreach (GameObject obj in playerList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        unit.speedNow -= 30;
                    }
                }
                break;
            case 8:
                if (start)
                {
                    foreach (GameObject obj in enemyList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        unit.speedNow -= 40;
                    }
                }
                else
                {
                    foreach (GameObject obj in enemyList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        unit.speedNow += 40;
                    }
                }
                break;
            case 9:
                if (start)
                {
                    foreach (GameObject obj in playerList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        unit.magicNow += 2;
                        unit.magic.text = unit.magicNow.ToString();
                    }
                }
                else
                {
                    foreach (GameObject obj in playerList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        unit.magicNow -= 2;
                        unit.magic.text = unit.magicNow.ToString();
                    }
                }
                break;
            case 10:
                if (start)
                {
                    foreach (GameObject obj in playerList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        if (!unit.death && unit.hpNow < unit.hpMaxNow)
                        {
                            unit.hpNow += 5000;
                            if (unit.hpNow > unit.hpMaxNow) unit.hpNow = unit.hpMaxNow;
                            stage.DisplayHP(unit);
                        }
                    }
                }
                break;
            case 11:
                if (start)
                {
                    foreach (GameObject obj in playerList)
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        if (unit.death) unit.hpNow += 22;
                    }
                }
                break;
        }

        if (start)
        {
            yield return CommandCutin(command,false,true);
            if (!enemy) activate *= -1;
            else activateEnemy *= -1;
        }
    }
    void CommandPassive(int no, bool enemy)
    {
        List<GameObject> playerList;
        if (!enemy) playerList = stage.unitList;
        else playerList = stage.unitListEnemy;
        List<GameObject> enemyList;
        if (!enemy) enemyList = stage.unitListEnemy;
        else enemyList = stage.unitList;
        switch (no)
        {
            case 8:
                foreach (GameObject obj in enemyList)
                {
                    Unit unit = obj.GetComponent<Unit>();
                    unit.speedDf -= 3;
                    unit.speedNow -= 3;
                }
                break;
            case 9:
                foreach (GameObject obj in playerList)
                {
                    Unit unit = obj.GetComponent<Unit>();
                    unit.magicDf += 1;
                    unit.magicNow += 1;
                    unit.magic.text = unit.magicNow.ToString();
                }
                break;
            case 10:
                foreach (GameObject obj in playerList)
                {
                    Unit unit = obj.GetComponent<Unit>();
                    unit.healSpeedDf += 3;
                    unit.healSpeedNow += 3;
                }
                break;
            case 11:
                foreach (GameObject obj in playerList)
                {
                    Unit unit = obj.GetComponent<Unit>();
                    unit.reviveDf -= 3;
                    unit.reviveNow -= 3;
                }
                break;
        }
    }
    public IEnumerator CommandCutin(Command command,bool start,bool end)
    {


        if (start)
        {
            if (stage.pun && PhotonNetwork.IsMasterClient) punManager.CommandCutin(command);
            cutinName.text = command.name;
            cutinTime.text = command.count.ToString();
            cutinText.text = command.active;
            cutinBack.GetComponent<Image>().enabled = true;
            commandGroup.SetActive(true);
            BGMManager.Instance.ChangeBaseVolume(0.2f);
            SEManager.Instance.Play(SEPath.TACTICSSTART);
            yield return commandFade.FadeoutCoroutine(5f, null);
        }

        if (end)
        {
            yield return new WaitForSecondsRealtime(1f);
            SEManager.Instance.Play(SEPath.TACTICSEND, delay: 3);
            yield return commandFade.FadeinCoroutine(5f, null);
            BGMManager.Instance.ChangeBaseVolume(0.4f);
            commandGroup.SetActive(false);
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
}
