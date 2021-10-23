using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PunActiveSizeDelta : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(GetComponent<RectTransform>().sizeDelta);
            stream.SendNext(gameObject.activeSelf);
        }
        else
        {
            GetComponent<RectTransform>().sizeDelta = (Vector2)stream.ReceiveNext();
            gameObject.SetActive((bool)stream.ReceiveNext());
        }
    }
}
