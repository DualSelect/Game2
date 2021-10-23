using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PunRotate : MonoBehaviour, IPunObservable, IPunInstantiateMagicCallback
{
    bool enemy = false;
    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
    {
        enemy = gameObject.transform.parent.gameObject.transform.parent.gameObject.GetComponent<Unit>().enemy;
    }
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (!enemy)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(transform.localRotation);
            }
            else
            {
                transform.localRotation = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}
