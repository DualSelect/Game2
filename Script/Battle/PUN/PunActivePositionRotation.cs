using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PunActivePositionRotation : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gameObject.activeSelf);
            stream.SendNext(transform.localPosition);
            stream.SendNext(transform.localRotation);
        }
        else
        {
            gameObject.SetActive((bool)stream.ReceiveNext());
            transform.localPosition = (Vector3)stream.ReceiveNext();
            transform.localRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
