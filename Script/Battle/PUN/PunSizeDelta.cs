using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PunSizeDelta : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(GetComponent<RectTransform>().sizeDelta);
        }
        else
        {
            GetComponent<RectTransform>().sizeDelta = (Vector2)stream.ReceiveNext();
        }
    }
}
