using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PunUnitSet : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.localPosition);
            stream.SendNext(transform.localScale);
        }
        else
        {

            Vector3 nextPosition = (Vector3)stream.ReceiveNext();
            transform.localScale = (Vector3)stream.ReceiveNext();
            float distanse = (transform.position - nextPosition).magnitude;
            transform.position = Vector3.MoveTowards(transform.position, nextPosition, distanse);
            if (nextPosition.y < -60)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }
    }
}
