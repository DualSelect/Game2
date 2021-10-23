using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

public class PunCharaSprite : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(GetComponent<Image>().sprite.name);
            stream.SendNext(transform.localScale);
        }
        else
        {
            Unit unit = transform.parent.gameObject.transform.parent.gameObject.GetComponent<Unit>();
            Stage stage = unit.gameObject.transform.parent.gameObject.transform.parent.GetComponent<Stage>();
            string colorNo = (string)stream.ReceiveNext();
            if(colorNo != "Dummy") stage.LoadIllustPublic(unit, colorNo);
            transform.localScale = (Vector3)stream.ReceiveNext();
        }
    }
}
