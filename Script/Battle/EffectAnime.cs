using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectAnime : MonoBehaviour
{
    public Animator effect;
    public void EffectStart(bool pun)
    {
        gameObject.SetActive(true);
        StartCoroutine(EffectStartIE(pun));
    }

    IEnumerator EffectStartIE(bool pun)
    {
        while (effect.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return new WaitForSeconds(0.1f);
        }
        if(!pun) Destroy(gameObject);
        else PhotonNetwork.Destroy(gameObject);
    }
}
