using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using KanKikuchi.AudioManager;

public class PunAudio : MonoBehaviourPunCallbacks
{
    public void PunSE(string path,float volume)
    {
        photonView.RPC(nameof(PRCSE), RpcTarget.Others, path,volume);
    }
    public void PunBGM(string path, float volume)
    {
        photonView.RPC(nameof(PRCBGM), RpcTarget.Others,path, volume);
    }
    public void PunBGMVolume(float volume)
    {
        photonView.RPC(nameof(PRCBGMVolume), RpcTarget.Others, volume);
    }
    [PunRPC]
    void PRCSE(string path, float volume)
    {
        SEManager.Instance.Play(path,volume);
    }
    [PunRPC]
    void PRCBGM(string path, float volume)
    {
        BGMManager.Instance.Play(path, volume);
    }
    [PunRPC]
    void PRCBGMVolume(float volume)
    {
        BGMManager.Instance.ChangeBaseVolume(volume);
    }
}
