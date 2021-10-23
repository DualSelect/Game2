using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PunTrail : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gameObject.GetComponent<TrailRenderer>().enabled);
            stream.SendNext(gameObject.GetComponent<TrailRenderer>().startWidth);
        }
        else
        {
            gameObject.GetComponent<TrailRenderer>().enabled = (bool)stream.ReceiveNext();
            gameObject.GetComponent<TrailRenderer>().startWidth = (float)stream.ReceiveNext();
        }
    }
}
