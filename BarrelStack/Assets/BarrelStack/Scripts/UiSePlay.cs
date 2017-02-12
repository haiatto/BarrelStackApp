using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class UiSePlay : MonoBehaviour, 
    IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{

    public AudioClip Se_OnPointerEnter;
    public AudioClip Se_OnPointerClick;
    public AudioClip Se_OnPointerExit;


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Se_OnPointerEnter != null)
        {
            GetComponent<AudioSource>().PlayOneShot(Se_OnPointerEnter);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Se_OnPointerClick != null)
        {
            GetComponent<AudioSource>().PlayOneShot(Se_OnPointerClick);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Se_OnPointerExit != null)
        {
            GetComponent<AudioSource>().PlayOneShot(Se_OnPointerExit);
        }
    }
}
