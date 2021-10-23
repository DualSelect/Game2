using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
public class PunActivePositionRotationColor : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gameObject.activeSelf);
            stream.SendNext(transform.localPosition);
            stream.SendNext(transform.localRotation);
            stream.SendNext(GetComponent<Image>().color.r);
            stream.SendNext(GetComponent<Image>().color.g);
            stream.SendNext(GetComponent<Image>().color.b);
            stream.SendNext(GetComponent<Image>().color.a);
        }
        else
        {
            gameObject.SetActive((bool)stream.ReceiveNext());
            transform.localPosition = (Vector3)stream.ReceiveNext();
            transform.localRotation = (Quaternion)stream.ReceiveNext();
            GetComponent<Image>().color = new Color((float)stream.ReceiveNext(), (float)stream.ReceiveNext(), (float)stream.ReceiveNext(), (float)stream.ReceiveNext());
        }
    }
}
