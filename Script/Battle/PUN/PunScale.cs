using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PunScale : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.localScale);
        }
        else
        {
            transform.localScale = (Vector3)stream.ReceiveNext();
        }
    }
}
