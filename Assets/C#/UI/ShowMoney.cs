using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowMoney : MonoBehaviour
{
    public Text txt;
    public int _ShowMoney;
    public bool UP = false;

    private void OnEnable()
    {
        UP = false;
    }
    private void Update()
    {
        int _last = PlayerPrefs.GetInt("MoneyLast", 0);
        int _new = PlayerPrefs.GetInt("Money", 0);
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _new += 100;
            PlayerPrefs.SetInt("Money", _new);
            PlayerPrefs.Save();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _new -= 100;
            PlayerPrefs.SetInt("Money", _new);
            PlayerPrefs.Save();
        }
        if (_last != _new)
        {
            if (!UP)
            {
                UP = true;
                StartCoroutine(nameof(MoneyChange), _new);
                _ShowMoney = _last; ;
            }
        }
        else
        {
            _ShowMoney = _last;
        }
        txt.text = "" + _ShowMoney;
    }

    IEnumerator MoneyChange(int To)
    {
        yield return new WaitForSeconds(0.5f);

        for (float part = 0; part < 1; part += Time.deltaTime /2f)
        {
            _ShowMoney = (int)Mathf.Lerp(_ShowMoney, To, Mathf.Pow( part,2f));
            
            
            
            ;
        }
        _ShowMoney = To;
        PlayerPrefs.SetInt("MoneyLast", To);
        PlayerPrefs.Save();
        UP = false;
    }
}
