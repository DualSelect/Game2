using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PunTransform : MonoBehaviour, IPunInstantiateMagicCallback
{
    public GameObject rotate180;
    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (!info.photonView.IsMine)
        {
            transform.SetParent(GameObject.Find("UnitList").transform);
            //gameObject.GetComponent<Unit>().unitInfo.transform.rotation = Quaternion.Euler(180, 180, 0);
            rotate180.transform.rotation = Quaternion.Euler(180, 180, 0);
        }
    }
}