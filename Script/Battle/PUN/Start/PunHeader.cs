using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PunHeader : MonoBehaviour, IPunInstantiateMagicCallback
{
    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
    {
        GameObject header = GameObject.Find("Header");
        transform.SetParent(header.transform);
        transform.position = new Vector3(50, 50, 50);
    }
}
