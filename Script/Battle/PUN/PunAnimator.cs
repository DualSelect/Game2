using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PunAnimator : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.localPosition);
            stream.SendNext(gameObject.activeSelf);
            Animator animator = GetComponent<Animator>();
            for (int i = 0; i < animator.layerCount; i++)
            {
                stream.SendNext(animator.GetLayerWeight(i));
            }
        }
        else
        {
            transform.localPosition = (Vector3)stream.ReceiveNext();
            gameObject.SetActive((bool)stream.ReceiveNext());
            Animator animator = GetComponent<Animator>();
            for (int i = 0; i < animator.layerCount; i++)
            {
                animator.SetLayerWeight(i, (float)stream.ReceiveNext());
            }
        }
    }
}
