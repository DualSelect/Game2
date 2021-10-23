using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PunActivePositionSizeDelta : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gameObject.activeSelf);
            stream.SendNext(transform.localPosition);
            stream.SendNext(GetComponent<RectTransform>().sizeDelta);
        }
        else
        {
            gameObject.SetActive((bool)stream.ReceiveNext());
            transform.localPosition = (Vector3)stream.ReceiveNext();
            GetComponent<RectTransform>().sizeDelta = (Vector2)stream.ReceiveNext();
        }
    }
}

