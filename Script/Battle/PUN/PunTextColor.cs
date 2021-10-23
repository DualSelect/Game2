using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
public class PunTextColor : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(GetComponent<Text>().text);
            stream.SendNext(GetComponent<Text>().color.r);
            stream.SendNext(GetComponent<Text>().color.g);
            stream.SendNext(GetComponent<Text>().color.b);
            stream.SendNext(GetComponent<Text>().color.a);
        }
        else
        {
            GetComponent<Text>().text = (string)stream.ReceiveNext();
            GetComponent<Text>().color = new Color((float)stream.ReceiveNext(), (float)stream.ReceiveNext(), (float)stream.ReceiveNext(), (float)stream.ReceiveNext());
        }
    }
}