using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunCommonView : MonoBehaviour,IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // ���g�����L����I�u�W�F�N�g�̃f�[�^�𑗐M����
            stream.SendNext(transform.localPosition);
            stream.SendNext(transform.localRotation);
            stream.SendNext(transform.localScale);
        }
        else
        {
            // ���v���C���[�����L����I�u�W�F�N�g�̃f�[�^����M����
            transform.localPosition = (Vector3)stream.ReceiveNext();
            transform.localRotation = (Quaternion)stream.ReceiveNext();
            transform.localScale = (Vector3)stream.ReceiveNext();
        }
    }
}
