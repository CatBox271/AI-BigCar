using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICardModeSendCar : MonoBehaviour
{
    public Button button;
    public Transform OwnCarParent;
    public CardManager cardManager;//这份手牌属于谁，右边发车不动左边的牌
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
        if (cardManager != null) cardManager.DiscardHand();
        print("send");
        loading.gameObject.SetActive(true);
        print(loading.gameObject.name);
    }
    private void OnSendingFinish()
    {
        loading.gameObject.SetActive(false);
        model.SetCarState(3);
    }
}
