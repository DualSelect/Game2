using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActObject : MonoBehaviour
{
    public bool enemy;
    public int unitNum;
    public int lebel;
    public int power;
    public int magic;
    public float rate;
    public int speed;
    public float atk;
    public int actDamage;
    public int type;//1:ñÇñ@íe 2:ñÇñ@ä—í íe 3:îöî≠ 11:ñCíe 
    public Vector3 startPoint;
    public Vector3 halfPoint;
    public Vector3 targetPoint;
    public string color;
    public float stopTime;
    public float damageArea;
    public List<Unit> hitUnits = new List<Unit>();
    public string tacticsID;

    public void ColorChange(string col)
    {
        color = col;
        switch (col)
        {
            case "ê‘":
                gameObject.GetComponent<Image>().color = new Color(1, 0, 0,0.7f);
                break;
            case "óŒ":
                gameObject.GetComponent<Image>().color = new Color(0, 1, 0, 0.7f);
                break;
            case "ê¬":
                gameObject.GetComponent<Image>().color = new Color(0, 0, 1, 0.7f);
                break;
            case "îí":
                gameObject.GetComponent<Image>().color = new Color(1, 1, 0, 0.7f);
                break;
            case "çï":
                gameObject.GetComponent<Image>().color = new Color(1, 0, 1, 0.7f);
                break;
        }

    }
}
