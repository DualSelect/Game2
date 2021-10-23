using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crystal : MonoBehaviour
{
    public bool enemy;
    public void SetPlayer(bool b)
    {
        enemy = b;
        if (!enemy)
        {
            GetComponent<Image>().color = new Color(1f, 0.5f, 0.5f);
        }
        else
        {
            GetComponent<Image>().color = new Color(0.5f, 0.5f, 1f);
        }
    }
}
