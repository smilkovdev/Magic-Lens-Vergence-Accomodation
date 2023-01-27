using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistantPose : MonoBehaviour
{
    public string ID;

    string id => string.IsNullOrEmpty(ID) ? gameObject.name : ID;
    float cachedX;
    float cachedY;
    float cachedZ;

    private void OnEnable()
    {
        cachedX = transform.position.x;
        cachedY = transform.position.y;
        cachedZ = transform.position.z;
    }

    public void Save(string sub)
    {
        PlayerPrefs.SetFloat($"{id}.{sub}.position.x", transform.position.x);
        PlayerPrefs.SetFloat($"{id}.{sub}.position.y", transform.position.y);
        PlayerPrefs.SetFloat($"{id}.{sub}.position.z", transform.position.z);
    }

    public void Clear(string sub)
    {
        PlayerPrefs.DeleteKey($"{id}.{sub}.position.x");
        PlayerPrefs.DeleteKey($"{id}.{sub}.position.y");
        PlayerPrefs.DeleteKey($"{id}.{sub}.position.z");

        transform.position = new Vector3(cachedX, cachedY, cachedZ);
    }

    public void Load(string sub)
    {
        var posX = PlayerPrefs.GetFloat($"{id}.{sub}.position.x", float.MaxValue);
        var posY = PlayerPrefs.GetFloat($"{id}.{sub}.position.y", float.MaxValue);
        var posZ = PlayerPrefs.GetFloat($"{id}.{sub}.position.z", float.MaxValue);

        transform.position = new Vector3(
            posX == float.MaxValue ? transform.position.x : posX,
            posY == float.MaxValue ? transform.position.y : posY,
            posZ == float.MaxValue ? transform.position.z : posZ);
    }
}
