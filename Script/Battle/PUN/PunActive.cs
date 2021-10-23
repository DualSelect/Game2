using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PunActive : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.IsWriting)
        {
            stream.SendNext(gameObject.activeSelf);
        }
        else
        {
            gameObject.SetActive((bool)stream.ReceiveNext());
        }
    }
}
