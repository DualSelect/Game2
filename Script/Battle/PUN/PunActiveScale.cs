using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PunActiveScale : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gameObject.activeSelf);
            stream.SendNext(transform.localScale);
        }
        else
        {
            gameObject.SetActive((bool)stream.ReceiveNext());
            transform.localScale = (Vector3)stream.ReceiveNext();
        }
    }
}
