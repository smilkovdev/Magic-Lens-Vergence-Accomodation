using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleMe : MonoBehaviour
{
    /*public string ID;

    string id => string.IsNullOrEmpty(ID) ? gameObject.name : ID;

    void Start()
    {
        var posX = PlayerPrefs.GetFloat($"{id}.scale.x", float.MaxValue);
        var posY = PlayerPrefs.GetFloat($"{id}.scale.y", float.MaxValue);
        var posZ = PlayerPrefs.GetFloat($"{id}.scale.z", float.MaxValue);

        transform.localScale = new Vector3(
            posX == float.MaxValue ? transform.localScale.x : posX,
            posY == float.MaxValue ? transform.localScale.y : posY,
            posZ == float.MaxValue ? transform.localScale.z : posZ);
    }*/

    public void SetScale(float value)
    {
        transform.localScale = Vector3.one * value;
    }
    /*
    public void Save()
    {
        PlayerPrefs.SetFloat($"{id}.scale.x", transform.localScale.x);
        PlayerPrefs.SetFloat($"{id}.scale.y", transform.localScale.y);
        PlayerPrefs.SetFloat($"{id}.scale.z", transform.localScale.z);
    }*/
}
