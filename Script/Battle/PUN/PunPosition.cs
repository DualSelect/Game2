using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PunPosition : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.localPosition);
        }
        else
        { 
            transform.localPosition = (Vector3)stream.ReceiveNext();
        }
    }
}
