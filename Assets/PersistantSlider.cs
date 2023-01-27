using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class PersistantSlider : MonoBehaviour
{
    public string ID;

    string id => string.IsNullOrEmpty(ID) ? gameObject.name : ID;
    Slider slider;
    float cachedValue;

    private void OnEnable()
    {
        slider = GetComponent<Slider>();
        cachedValue = slider.value;
    }

    public void Save(string sub)
    {
        PlayerPrefs.SetFloat($"{id}.{sub}.scale", slider.value);
    }

    public void Clear(string sub)
    {
        PlayerPrefs.DeleteKey($"{id}.{sub}.scale");
        slider.value = cachedValue;
    }

    public void Load(string sub)
    {
        var scale = PlayerPrefs.GetFloat($"{id}.{sub}.scale", float.MaxValue);

        if (scale != float.MaxValue)
            slider.value = scale;
    }
}
