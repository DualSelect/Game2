using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PunEffectTransform : MonoBehaviour, IPunInstantiateMagicCallback
{
    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
    {

         this.transform.SetParent(GameObject.Find("EffectList").transform);

    }
}
