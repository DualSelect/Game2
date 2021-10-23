using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PunTacticsArea : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.localPosition);
            stream.SendNext(GetComponent<RectTransform>().sizeDelta);
            stream.SendNext(GetComponent<Image>().sprite.name);
        }
        else
        {
            transform.localPosition = (Vector3)stream.ReceiveNext();
            GetComponent<RectTransform>().sizeDelta = (Vector2)stream.ReceiveNext();
            string shape = (string)stream.ReceiveNext();
            if (shape != GetComponent<Image>().sprite.name)
            {
                if (shape == "circle") GetComponent<Image>().sprite = GameObject.Find("Stage").GetComponent<Stage>().shape[0];
                if (shape == "square") GetComponent<Image>().sprite = GameObject.Find("Stage").GetComponent<Stage>().shape[1];
            }
        }
    }
}

