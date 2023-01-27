using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class ValueToTMPText : MonoBehaviour
{

    TMP_Text text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    public void ToText(float value)
    {
        text.text = value.ToString("F3");
    }
}
