using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PunActivePosition : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gameObject.activeSelf);
            stream.SendNext(transform.localPosition);
        }
        else
        {
            gameObject.SetActive((bool)stream.ReceiveNext());
            transform.localPosition = (Vector3)stream.ReceiveNext();
        }
    }
}
