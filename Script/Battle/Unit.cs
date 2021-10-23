using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KanKikuchi.AudioManager;
using Photon.Pun;

public class Unit : MonoBehaviourPunCallbacks
{
    //オブジェクト
    public bool AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA;
    public GameObject pin;
    public GameObject root;
    public GameObject unitInfo;
    public GameObject hp100;
    public GameObject hp200;
    public GameObject hpDeath;
    public GameObject breakFrame;
    public GameObject breakGauge;
    public GameObject jobAngle;
    public GameObject jobArea;
    public GameObject jobArea2;
    public TrailRenderer trail;
    public Animator attack;
    public Animator revival;
    public Animator cure;
    public Animator breakAttack;
    public Animator statusUp;
    public Animator statusDown;
    public GameObject arrow;
    public GameObject jobDirection;
    public Animator[] spire;
    public GameObject[] shoot;
    public Text power;
    public Text magic;
    public Text skill;
    public Image ring;
    public Image chara;
    public Text charaName;
    public Image tacticsArea;
    public Image tacticsRegion;
    public GameObject tacticsTarget;
    public GameObject tacticsAngle;
    public GameObject[] rotate180;
    public GameObject actAngle;
    public GameObject[] actArea;
    public GameObject actWideAngle;

    //基本ステータス
    public bool BBBBBBBBBBBBBBBBBBBBBBBBBBBBBB;
    public Card card;
    public string tacticsId;
    public int cost;
    public int jobDf;
    public int powerDf;
    public int magicDf;
    public int skillDf;
    public float speedDf;
    public float breakDf;
    public float breakSpeedDf;
    public int reviveDf;
    public int reviveHpDf;
    public int hpMaxDf;
    public float healSpeedDf;
    public float atkDf;
    public float defDf;
    public float mDefDf;
    public float actSpeedDf;
    public string actTypeDf;

    //現在ステータス
    public bool CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC;
    public int jobNow;
    public int powerNow;
    public int magicNow;
    public int skillNow;
    public float speedNow;
    public float breakNow;
    public float breakSpeedNow;
    public int reviveNow;
    public int reviveHpNow;
    public int hpMaxNow;
    public float healSpeedNow;
    public float atkNow;
    public float defNow;
    public float mDefNow;
    public float actSpeedNow;
    public string actTypeNow;

    //特殊ステータス
    public int arrowTargetNum;
    public int stealth;
    public float unitSize;
    public string actTacticsID;
    public int battleThrough;
    public int defenderThrough;
    public int pinMoveDisable;
    public int battleAct;


    //変動値
    public bool DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD;
    public float hpNow = 10000;
    public float speedUpFix = 0; 
    public float skillAct = 0;
    public int start = 0;
    public float breakSpeedPer = 1;
    public List<int> arrowTarget = new List<int>();
    public float arrowDirection = 180;
    public int activateTimes = 0;
    public float actCoolTime;
    public float ice;

    //判定値(そのフレームでの状態:誰々に弓を打った、突撃したetc)
    public bool EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE;
    public List<Unit> battleTarget;
    public List<Unit> actTarget;
    public List<Unit> defeatTarget;
    public bool stopCheak;
    public bool breakCheck;
    public bool breakSuccess;

    //管理値
    public bool FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF;
    public bool death;
    public bool heal;
    public int unitNum;
    public bool select;
    public bool enemy;
    public int punPlayer;
    public bool noRevival;
    public bool cpu;
    public bool activateNow;




    Stage stage = null;



    void Start()
    {
        stage = GameObject.Find("Stage").GetComponent<Stage>();
    }


    public void UnitDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 200f))
        {
            float distance = (unitInfo.transform.position - hit.point).magnitude;
            if(distance < 4 && jobDf == 3)
            {
                UnitAngle(hit.point);
            }
            else if (stage.PinCheak(hit.point, unitNum,enemy))
            {
                PinMove(hit.point);
                UnitAngle(hit.point);
            }
        }
    }
    public void UnitClick()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        if (!Input.GetKey(KeyCode.Tab) && !Input.GetKey(KeyCode.LeftShift)) stage.SelectClear();
        UnitSelect();
    }

    public void UnitSelect()
    {
        select = true;
        chara.gameObject.SetActive(true);
        if(stage.start)stage.tactics.TacticsChange(this);
        if (!enemy)
        {
            power.color = new Color(255 / 255, 25 / 255, 25 / 255);
            magic.color = new Color(255 / 255, 25 / 255, 25 / 255);
            skill.color = new Color(255 / 255, 25 / 255, 25 / 255);
        }
        else
        {
            power.color = new Color(25 / 255, 25 / 255, 255 / 255);
            magic.color = new Color(25 / 255, 25 / 255, 255 / 255);
            skill.color = new Color(25 / 255, 25 / 255, 255 / 255);
        }
        foreach (GameObject button in actArea)
        {
            button.GetComponent<Button>().interactable = true;
            button.GetComponent<Image>().raycastTarget = true;
        }
    }
    public void SelectReset()
    {
        select = false;
        chara.gameObject.SetActive(false);
        if (!enemy)
        {
            power.color = new Color(0.8f, 0.3f, 0.3f);
            magic.color = new Color(0.8f, 0.3f, 0.3f);
            skill.color = new Color(0.8f, 0.3f, 0.3f);
        }
        else
        {
            power.color = new Color(0.3f, 0.3f, 0.8f);
            magic.color = new Color(0.3f, 0.3f, 0.8f);
            skill.color = new Color(0.3f, 0.3f, 0.8f);
        }
        foreach (GameObject button in actArea)
        {
            button.GetComponent<Button>().interactable = false;
            button.GetComponent<Image>().raycastTarget = false;
        }
    }
    public void UnitAngle(Vector3 point)
    {
        if (stage.pun && !PhotonNetwork.IsMasterClient) photonView.RPC(nameof(PunUnitAngle), RpcTarget.MasterClient, point);
        jobDirection.transform.rotation = Quaternion.Euler(0, 0, +180 - Mathf.Atan2(unitInfo.transform.position.x - point.x, unitInfo.transform.position.y - point.y) * Mathf.Rad2Deg);
        if (card.tacticsAreaSpin == "可能") tacticsAngle.transform.rotation = jobDirection.transform.rotation;
        actAngle.transform.rotation = jobDirection.transform.rotation;
        arrowDirection = -Mathf.Atan2(unitInfo.transform.position.x - point.x, unitInfo.transform.position.y - point.y) * Mathf.Rad2Deg;
        arrowTarget = new List<int>();
    }
    [PunRPC]
    private void PunUnitAngle(Vector3 point)
    {
        jobDirection.transform.rotation = Quaternion.Euler(0, 0, +180 - Mathf.Atan2(unitInfo.transform.position.x - point.x, unitInfo.transform.position.y - point.y) * Mathf.Rad2Deg);
        if (card.tacticsAreaSpin == "可能") tacticsAngle.transform.rotation = Quaternion.Euler(0, 0, +180 - Mathf.Atan2(unitInfo.transform.position.x - point.x, unitInfo.transform.position.y - point.y) * Mathf.Rad2Deg);
        actAngle.transform.rotation = jobDirection.transform.rotation;
        arrowDirection = -Mathf.Atan2(unitInfo.transform.position.x - point.x, unitInfo.transform.position.y - point.y) * Mathf.Rad2Deg;
        arrowTarget = new List<int>();
    }
    public void PinMove(Vector3 point)
    {
        if (pinMoveDisable == 0)
        {
            if (stage.pun && !PhotonNetwork.IsMasterClient) photonView.RPC(nameof(PunPinMove), RpcTarget.MasterClient, point);
            pin.transform.position = point;
            if (!enemy)
            {
                if (pin.transform.position.y > 55) pin.transform.position = new Vector3(pin.transform.position.x, 55f);
                if (pin.transform.position.y > 48 && pin.transform.position.y < 51) pin.transform.position = new Vector3(pin.transform.position.x, 49.5f, pin.transform.position.z);
                if (pin.transform.position.y <= -56) pin.transform.position = new Vector3(pin.transform.position.x, -61.5f);
            }
            else
            {
                if (pin.transform.position.y < -55) pin.transform.position = new Vector3(pin.transform.position.x, -55f);
                if (pin.transform.position.y < -48 && pin.transform.position.y > -51) pin.transform.position = new Vector3(pin.transform.position.x, -49.5f, pin.transform.position.z);
                if (pin.transform.position.y >= 56) pin.transform.position = new Vector3(pin.transform.position.x, 61.5f);
            }
            root.transform.position = new Vector2((pin.transform.position.x + unitInfo.transform.position.x) / 2, (pin.transform.position.y + unitInfo.transform.position.y) / 2);
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(80 / ((pin.transform.position - unitInfo.transform.position).magnitude + 20), (pin.transform.position - unitInfo.transform.position).magnitude);
            root.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(unitInfo.transform.position.x - pin.transform.position.x, unitInfo.transform.position.y - pin.transform.position.y) * Mathf.Rad2Deg);
        }
    }
    [PunRPC]
    public void PunPinMove(Vector3 point)
    {
        if (pinMoveDisable == 0)
        {
            pin.transform.position = point;
            if (!enemy)
            {
                if (pin.transform.position.y > 55) pin.transform.position = new Vector3(pin.transform.position.x, 55f);
                if (pin.transform.position.y > 48 && pin.transform.position.y < 51) pin.transform.position = new Vector3(pin.transform.position.x, 49.5f, pin.transform.position.z);
                if (pin.transform.position.y <= -56) pin.transform.position = new Vector3(pin.transform.position.x, -61.5f);
            }
            else
            {
                if (pin.transform.position.y < -55) pin.transform.position = new Vector3(pin.transform.position.x, -55f);
                if (pin.transform.position.y < -48 && pin.transform.position.y > -51) pin.transform.position = new Vector3(pin.transform.position.x, -49.5f, pin.transform.position.z);
                if (pin.transform.position.y >= 56) pin.transform.position = new Vector3(pin.transform.position.x, 61.5f);
            }
            root.transform.position = new Vector2((pin.transform.position.x + unitInfo.transform.position.x) / 2, (pin.transform.position.y + unitInfo.transform.position.y) / 2);
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(80 / ((pin.transform.position - unitInfo.transform.position).magnitude + 20), (pin.transform.position - unitInfo.transform.position).magnitude);
            root.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(unitInfo.transform.position.x - pin.transform.position.x, unitInfo.transform.position.y - pin.transform.position.y) * Mathf.Rad2Deg);
        }
    }
    public IEnumerator HealAnime()
    {
        if (!enemy) SEManager.Instance.Play(SEPath.MAX_HEAL, 0.5f);
        else if (stage.pun) stage.punAudio.PunSE(SEPath.MAX_HEAL, 0.5f);
        cure.gameObject.SetActive(true);
        cure.Play("回復黄");
        while (!enemy && cure.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return new WaitForSeconds(0.1f);
        }
        if(enemy) yield return new WaitForSeconds(0.7f);
        cure.gameObject.SetActive(false);
    }
    public IEnumerator RevivalAnime()
    {
        if (!enemy) SEManager.Instance.Play(SEPath.REVIVAL);
        else if(stage.pun)stage.punAudio.PunSE(SEPath.REVIVAL, 1);
        revival.gameObject.SetActive(true);
        revival.Play("復活");
        while (!enemy && revival.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return new WaitForSeconds(0.1f);
        }
        if (enemy) yield return new WaitForSeconds(0.7f);
        revival.gameObject.SetActive(false);
        chara.color = new Color(1, 1, 1, 0.4f);
        if(stage.pun) photonView.RPC(nameof(PunCharaColorReset), RpcTarget.Others);
    }
    [PunRPC]
    public void PunCharaColorReset()
    {
        chara.color = new Color(1, 1, 1, 0.4f);
    }
    public IEnumerator BreakAnime()
    {
        stage.SE(SEPath.BREAK,1);

        breakAttack.gameObject.SetActive(true);
        breakAttack.Play("打撃");
        while (breakAttack.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return new WaitForSeconds(0.1f);
        }
        breakAttack.gameObject.SetActive(false);
    }
    public IEnumerator AttackAnime(Vector3 vector3)
    {
        attack.transform.position = vector3;
        float z = Mathf.Atan2(unitInfo.transform.position.x - vector3.x, unitInfo.transform.position.y - vector3.y) * Mathf.Rad2Deg;
        if (z > 0) attack.transform.rotation = Quaternion.Euler(0, 0, 180 - z);
        else attack.transform.rotation = Quaternion.Euler(0, 0, -180 - z);

        stage.SE(SEPath.ATTACK,1);

        attack.gameObject.SetActive(true);
        attack.Play("突撃");
        while (attack.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return new WaitForSeconds(0.1f);
        }
        attack.gameObject.SetActive(false);
    }
    public IEnumerator SpireAnime(Vector3 vector3,int enemyNum,float distance,float spireLong)
    {
        while (enemyNum > 9) enemyNum -= 10;
        if (!spire[enemyNum].gameObject.activeSelf)
        {
            stage.SE(SEPath.RANCE, 0.7f);

            Vector3 fixVector = (vector3 - jobArea.transform.position) /distance;
            spire[enemyNum].transform.position = jobArea.transform.position + fixVector * (spireLong/2 + unitSize * Stage.Constants.unitSizeFix) / 2;
            spire[enemyNum].transform.localScale = new Vector3(8, spireLong/2 + 5, 1);


            float z = Mathf.Atan2(jobArea.transform.position.x - vector3.x, jobArea.transform.position.y - vector3.y) * Mathf.Rad2Deg;
            if (z > 0) spire[enemyNum].transform.rotation = Quaternion.Euler(0, 0, 180-z);
            else spire[enemyNum].transform.rotation = Quaternion.Euler(0,0, -180-z);

            spire[enemyNum].gameObject.SetActive(true);
            spire[enemyNum].Play("槍");
            while (spire[enemyNum].GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return new WaitForSeconds(0.1f);
            }
            spire[enemyNum].gameObject.SetActive(false);
        }
    }
    public void ArrowAnime( Vector3 enemy,float distanse)
    {
        stage.SE(SEPath.ARROW, 0.5f);
        GameObject arrowObj;
        if (stage.pun)
        {
            if(!this.enemy)arrowObj = PhotonNetwork.Instantiate("矢", new Vector3(0, 0, 0), Quaternion.identity);
            else arrowObj = PhotonNetwork.Instantiate("青矢", new Vector3(0, 0, 0), Quaternion.identity);
        }
        else
        {
            arrowObj = Instantiate(arrow, arrow.transform.parent) as GameObject;
        }
        int x = UnityEngine.Random.Range(-5, 5);
        int y = UnityEngine.Random.Range(-5, 5);
        Vector3 start = new Vector3(unitInfo.transform.position.x + x, unitInfo.transform.position.y + y);
        Vector3 end = new Vector3(enemy.x + x, enemy.y + y);
        Vector3 half = end - start * 0.50f + start;
        half.z += -distanse*0.5f;
        StartCoroutine(LerpThrow(arrowObj, start, half, end, 30+distanse*0.3f));
    }
    IEnumerator LerpThrow(GameObject target, Vector3 start, Vector3 half, Vector3 end, float duration)
    {
        float z = Mathf.Atan2(start.x - end.x, start.y - end.y) * Mathf.Rad2Deg;

        target.SetActive(true);
        float startTime = Time.timeSinceLevelLoad;
        float rate = 0f;
        while (true)
        {
            if (rate >= 1.0f)
            {
                if (stage.pun)
                {
                    PhotonNetwork.Destroy(target);
                }
                else
                {
                    Destroy(target);
                }
                yield break;
            }
            float diff = Time.timeSinceLevelLoad - startTime;
            rate = diff / (duration / 60f);
            target.transform.position = CalcLerpPoint(start, half, end, rate);


            if (z > 0) target.transform.rotation = Quaternion.Euler((Math.Abs(z) - 90) * (rate - 0.5f), (90-Math.Abs(Math.Abs(z)-90))*(rate-0.5f), -z+180);
            else target.transform.rotation = Quaternion.Euler((Math.Abs(z) - 90) * (rate - 0.5f), -(90 - Math.Abs(Math.Abs(z) - 90)) * (rate - 0.5f), 360-z+180);

            yield return new WaitForSeconds(0.01f);
        }
    }
    Vector3 CalcLerpPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        var a = Vector3.Lerp(p0, p1, t);
        var b = Vector3.Lerp(p1, p2, t);
        return Vector3.Lerp(a, b, t);
    }
    public void PunPlayer()
    {
        if(stage==null)stage = GameObject.Find("Stage").GetComponent<Stage>();
        transform.SetParent(stage.unitListObject.transform);
        foreach(GameObject rotate in rotate180)
        {
            rotate.transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        if (punPlayer == 0)
        {
            enemy = false;
            stage.unitList.Add(gameObject);
            breakAttack.transform.localPosition = new Vector3(0, 10, 0);
            chara.transform.rotation = Quaternion.Euler(0, 0, 180);
            StartCoroutine(CardCheck());
        }
        else if(punPlayer == 1 || punPlayer == -1)
        {
            enemy = true;
            stage.unitListEnemy.Add(gameObject);
            breakAttack.transform.localPosition = new Vector3(0, -13, 0);
            power.color = new Color(0.3f, 0.3f, 0.8f);
            magic.color = new Color(0.3f, 0.3f, 0.8f);
            skill.color = new Color(0.3f, 0.3f, 0.8f);
            pin.transform.position = unitInfo.transform.position;
            pin.SetActive(true);
            chara.transform.rotation = Quaternion.Euler(0, 0, 180);
            pin.transform.rotation = Quaternion.Euler(0, 0, 180);
            root.SetActive(true);
            actAngle.SetActive(true);
            tacticsTarget.transform.localPosition = new Vector3(0,9,0);
            tacticsTarget.transform.rotation = Quaternion.Euler(0, 0, 180);
            if(punPlayer == 1) unitInfo.GetComponent<Image>().raycastTarget = true;
            if (punPlayer == 1) unitInfo.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
            jobArea.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.5f, 0.5f);
            StartCoroutine(PinRoot());
            StartCoroutine(CardCheck());
        }
    }
    IEnumerator PinRoot()
    {
        bool preDeath = false;
        while (true)
        {
            if (!preDeath && death)
            {
                pin.transform.position = unitInfo.transform.position;
            }
            root.transform.position = new Vector2((pin.transform.position.x + unitInfo.transform.position.x) / 2, (pin.transform.position.y + unitInfo.transform.position.y) / 2);
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(80 / ((pin.transform.position - unitInfo.transform.position).magnitude + 20), (pin.transform.position - unitInfo.transform.position).magnitude);
            root.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(unitInfo.transform.position.x - pin.transform.position.x, unitInfo.transform.position.y - pin.transform.position.y) * Mathf.Rad2Deg);
            preDeath = death;
            yield return new WaitForSecondsRealtime(0.05f);
        }
    }
    IEnumerator CardCheck()
    {
        while (!cpu)
        {
            if (chara.sprite.name!="UIMask")
            {
                card = stage.cardMaster.CardList.Find(c => c.colorNo == chara.sprite.name);
            }
            yield return new WaitForSecondsRealtime(0.05f);
        }
    }
    public void ActButton()
    {
        if (stage.pun) photonView.RPC(nameof(RPCActButton), RpcTarget.MasterClient);
        else stage.Act(this);
    }
    [PunRPC]
    void RPCActButton()
    {
        stage.Act(this);
    }
    public void ForcePinMove(Vector3 point)
    {
        if (stage.pun && PhotonNetwork.IsMasterClient) photonView.RPC(nameof(PunPinMove), RpcTarget.Others, point);
        pin.transform.position = point;
        if (!enemy)
        {
            if (pin.transform.position.y > 55) pin.transform.position = new Vector3(pin.transform.position.x, 55f);
            if (pin.transform.position.y > 48 && pin.transform.position.y < 51) pin.transform.position = new Vector3(pin.transform.position.x, 49.5f, pin.transform.position.z);
            if (pin.transform.position.y <= -56) pin.transform.position = new Vector3(pin.transform.position.x, -61.5f);
        }
        else
        {
            if (pin.transform.position.y < -55) pin.transform.position = new Vector3(pin.transform.position.x, -55f);
            if (pin.transform.position.y < -48 && pin.transform.position.y > -51) pin.transform.position = new Vector3(pin.transform.position.x, -49.5f, pin.transform.position.z);
            if (pin.transform.position.y >= 56) pin.transform.position = new Vector3(pin.transform.position.x, 61.5f);
        }
        root.transform.position = new Vector2((pin.transform.position.x + unitInfo.transform.position.x) / 2, (pin.transform.position.y + unitInfo.transform.position.y) / 2);
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(80 / ((pin.transform.position - unitInfo.transform.position).magnitude + 20), (pin.transform.position - unitInfo.transform.position).magnitude);
        root.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(unitInfo.transform.position.x - pin.transform.position.x, unitInfo.transform.position.y - pin.transform.position.y) * Mathf.Rad2Deg);
    }
}