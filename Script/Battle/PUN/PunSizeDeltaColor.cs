using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
public class PunSizeDeltaColor : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(GetComponent<RectTransform>().sizeDelta);
            stream.SendNext(GetComponent<Image>().color.r);
            stream.SendNext(GetComponent<Image>().color.g);
            stream.SendNext(GetComponent<Image>().color.b);
            stream.SendNext(GetComponent<Image>().color.a);
        }
        else
        {
            GetComponent<RectTransform>().sizeDelta = (Vector2)stream.ReceiveNext();
            GetComponent<Image>().color = new Color((float)stream.ReceiveNext(), (float)stream.ReceiveNext(), (float)stream.ReceiveNext(), (float)stream.ReceiveNext());
        }
    }
}
