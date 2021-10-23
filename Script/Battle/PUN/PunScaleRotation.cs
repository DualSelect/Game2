using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PunScaleRotation : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.localScale);
            stream.SendNext(transform.localRotation);
        }
        else
        {
            transform.localScale = (Vector3)stream.ReceiveNext();
            transform.localRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
