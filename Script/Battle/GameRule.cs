using KanKikuchi.AudioManager;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameRule : MonoBehaviour
{
    public Dropdown bgmSelect;
    public InputField room;
    public InputField cpuLevel;
    public InputField cpuRandom;
    public Stage stage;
    public Commander commander;
    void Start()
    {
        room.text = PlayerPrefs.GetString("room", "A");
        cpuLevel.text = PlayerPrefs.GetString("cpuLevel", "4");
        cpuRandom.text = PlayerPrefs.GetString("cpuRandom", "2");
        BGMChange();
    }
    void BGMChange()
    {
        bgmSelect.value= PlayerPrefs.GetInt("bgm", 0);
        switch (PlayerPrefs.GetInt("bgm", 0))
        {
            case 0:
                BGMManager.Instance.Play(BGMPath.BATTLE0, 0.8f);
                break;
            case 1:
                BGMManager.Instance.Play(BGMPath.BATTLE1, 0.8f);
                break;
            case 2:
                BGMManager.Instance.Play(BGMPath.BATTLE2, 0.8f);
                break;
        }
    }
    public void BGMSelect(int i)
    {
        PlayerPrefs.SetInt("bgm", i);
        BGMChange();
    }
    public void Back()
    {
        if(stage.pun)PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Deck");
    }
    public void Cpu()
    {
        stage.cpu = true;
        stage.pun = false;
        stage.tactics.playerRed.text = PlayerPrefs.GetString("playerName");
        stage.tactics.playerBlue.text = "CPU";
        GameStart();
    }
    public void Macth()
    {
        stage.cpu = false;
        stage.pun = true;
        GameStart();
    }
    public void Debug()
    {
        stage.cpu = false;
        stage.pun = false;
        stage.debug = true;
        for (int i = 0; i < stage.deck.Length; i++)
        {
            stage.deckEnemy[i] = PlayerPrefs.GetString("unit" + i.ToString());
        }
        GameStart();
    }
    void GameStart()
    {
        PlayerPrefs.SetString("room", room.text);
        PlayerPrefs.SetString("cpuLevel", cpuLevel.text);
        PlayerPrefs.SetString("cpuRandom", cpuRandom.text);
        commander.commandNo[0] = PlayerPrefs.GetInt("command0");
        commander.commandNo[1] = PlayerPrefs.GetInt("command1");
        for(int i=0; i < stage.deck.Length; i++)
        {
            stage.deck[i] = PlayerPrefs.GetString("unit"+i.ToString());
        }
        stage.room = room.text;
        stage.cpuLevel = int.Parse(cpuLevel.text);
        stage.cpuRandom = int.Parse(cpuRandom.text);
        gameObject.SetActive(false);
        stage.GameStart();
    }
}
