using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
public class PunPositionColor : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.localPosition);
            stream.SendNext(GetComponent<Image>().color.r);
            stream.SendNext(GetComponent<Image>().color.g);
            stream.SendNext(GetComponent<Image>().color.b);
            stream.SendNext(GetComponent<Image>().color.a);
        }
        else
        {
            transform.localPosition = (Vector3)stream.ReceiveNext();
            GetComponent<Image>().color = new Color((float)stream.ReceiveNext(), (float)stream.ReceiveNext(), (float)stream.ReceiveNext(), (float)stream.ReceiveNext());
        }
    }
}
