using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PunTest : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        Debug.Log("TEST");
        if (stream.IsWriting) 
        {
            stream.SendNext(UnityEngine.Random.value);
        }
        else
        {
            Debug.Log((float)stream.ReceiveNext());
        }
    }
}
