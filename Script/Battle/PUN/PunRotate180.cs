using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PunRotate180 : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.localRotation);
        }
        else
        {
            transform.localRotation = (Quaternion)stream.ReceiveNext();
            transform.Rotate(0,0,180);
        }
    }
}
