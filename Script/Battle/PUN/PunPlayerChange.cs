using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PunPlayerChange : MonoBehaviour, IPunObservable
{
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            Unit unit = GetComponent<Unit>();
            stream.SendNext(unit.unitNum);
            stream.SendNext(unit.punPlayer);
            stream.SendNext(unit.death);
            stream.SendNext(unit.heal);
            stream.SendNext(unit.activateNow);
            stream.SendNext(unit.activateTimes);

        }
        else
        {
            Unit unit = GetComponent<Unit>();
            unit.unitNum = (int)stream.ReceiveNext();
            int receive = (int)stream.ReceiveNext();
            if (unit.punPlayer == -1)
            {
                unit.punPlayer = receive;
                unit.PunPlayer();
            }
            unit.death = (bool)stream.ReceiveNext();
            unit.heal = (bool)stream.ReceiveNext();
            unit.activateNow = (bool)stream.ReceiveNext();
            unit.activateTimes = (int)stream.ReceiveNext();
        }
    }
}
