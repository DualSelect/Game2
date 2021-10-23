using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PunPositionRotationSizeDelta : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.localPosition);
            stream.SendNext(transform.localRotation);
            stream.SendNext(GetComponent<RectTransform>().sizeDelta);
        }
        else
        {
            transform.localPosition = (Vector3)stream.ReceiveNext();
            transform.localRotation = (Quaternion)stream.ReceiveNext();
            GetComponent<RectTransform>().sizeDelta = (Vector2)stream.ReceiveNext();
        }
    }
}
