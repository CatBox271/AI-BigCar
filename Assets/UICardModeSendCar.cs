using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICardModeSendCar : MonoBehaviour
{
    public Button button;
    public Transform OwnCarParent;
    private SpriteRenderer loading;
    private 车辆样板加载 model;
    private OwnCar car;
    private void Awake()
    {
        car = OwnCarParent.GetComponentInChildren<OwnCar>();
        loading = OwnCarParent.GetChild(1).GetComponent<SpriteRenderer>();
        model = OwnCarParent.GetComponentInChildren<车辆样板加载>();
        button.onClick.AddListener(OnButtonDown);
        car.SendingFinish += OnSendingFinish;
    }
    private void OnButtonDown()
    {
        if (car.Sending) return;
        car.SendCar = true;
        print("send");
        loading.gameObject.SetActive(true);
        print(loading.gameObject.name);
    }
    private void OnSendingFinish()
    {
        loading.gameObject.SetActive(false);
        model.SetCarState(2);
    }
}
