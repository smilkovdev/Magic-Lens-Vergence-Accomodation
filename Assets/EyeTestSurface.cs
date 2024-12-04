using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class EyeTestSurface : MonoBehaviour
{
    public SurfaceType surfaceType;
    public LetterType letterType;
    public TMP_Text tmpText;
    public int order;

    readonly string[] sloanLetters = new[] { "C", "D", "H", "K", "N", "O", "R", "S", "V" };
    readonly int[] landholtRotations = new[] { 0, 90, 180, 270 };
    float previousZEuler;
    string previousText;

    public enum SurfaceType
    {
        NEAR,
        FAR,
    }

    public enum LetterType
    {
        SLOAN,
        LANDHOLT
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            NextRandomSymbol(-1);
        }
    }

    public void NextRandomSymbol(float visibleSeconds)
    {
        switch (letterType)
        {
            case LetterType.SLOAN: 
                tmpText.text = sloanLetters[Random.Range(0, sloanLetters.Length)];
                tmpText.rectTransform.localEulerAngles = Vector3.zero;
                break;
            case LetterType.LANDHOLT:
                tmpText.text = "C";
                tmpText.rectTransform.localEulerAngles = Vector3.forward * 
                    landholtRotations[Random.Range(0, landholtRotations.Length)];
                break;
        }

        previousZEuler = tmpText.rectTransform.localEulerAngles.z;
        previousText = tmpText.text;

        if (visibleSeconds > 0f)
            Invoke(nameof(Clear), visibleSeconds);
    }

    public void Clear()
    {
        tmpText.text = order == 1 ? "?" : string.Empty;
        tmpText.color = Color.red;
        tmpText.rectTransform.localEulerAngles = Vector3.zero;
    }

    public void SetLetterType(int type)
    {
        letterType = (LetterType)type;
    }

    public IEnumerator WaitForArrowInput(string displayText = "")
    {
        var wait = true;
        while (wait)
        {
            var down = Input.GetKeyUp(KeyCode.DownArrow);
            var up = Input.GetKeyUp(KeyCode.UpArrow);
            var left = Input.GetKeyUp(KeyCode.LeftArrow);
            var right = Input.GetKeyUp(KeyCode.RightArrow);
            var isLandholt = letterType == LetterType.LANDHOLT;

            if (down || up || left || right)
            {
                tmpText.text = isLandholt ? "C" : "!";
                wait = false;
                CancelInvoke(nameof(Clear));
            }
            else
            {
                if (tmpText.text == string.Empty)
                    tmpText.text = displayText;

                yield return null;
            }

            if (down && isLandholt) tmpText.rectTransform.localEulerAngles = Vector3.forward * 270;
            else if (up && isLandholt) tmpText.rectTransform.localEulerAngles = Vector3.forward * 90;
            else if (left && isLandholt) tmpText.rectTransform.localEulerAngles = Vector3.forward * 180;
            else if (right && isLandholt) tmpText.rectTransform.localEulerAngles = Vector3.forward * 0;
        }

        tmpText.color = IsCorrect ? Color.green : Color.gray;
    }

    public IEnumerator WaitForLetterInput(string displayText)
    {
        if (letterType != LetterType.SLOAN)
            yield break;

        var wait = true;
        while (wait)
        {
            if (tmpText.text == string.Empty)
                tmpText.text = displayText;

            foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyUp(kcode) && (int)kcode < 123)
                {
                    tmpText.text = kcode.ToString();
                    wait = false;
                    break;
                }
            }
            yield return null;
        }

        tmpText.color = IsCorrect ? Color.green : Color.gray;
    }

    public IEnumerator WaitForSpace()
    {
        while (!Input.GetKeyUp(KeyCode.Space))
        {
            yield return null;
        }
        tmpText.text = string.Empty;
    }

    public bool IsCorrect
    {
        get
        {
            switch (letterType)
            {
                case LetterType.SLOAN:
                    return tmpText.text == previousText;
                case LetterType.LANDHOLT:
                    return tmpText.rectTransform.localEulerAngles.z == previousZEuler;
                default:
                    return false;
            }
        }
    }

    public string Question
    {
        get
        {
            switch (letterType)
            {
                case LetterType.SLOAN:
                    return previousText;
                case LetterType.LANDHOLT:
                    return previousZEuler.ToString();
                default: 
                    return string.Empty;
            }
        }
    }

    public string Answer
    {
        get
        {
            switch (letterType)
            {
                case LetterType.SLOAN:
                    return tmpText.text;
                case LetterType.LANDHOLT:
                    return tmpText.transform.localEulerAngles.z.ToString();
                default:
                    return string.Empty;
            }
        }
    }

    // New method for setting the symbol based on a specific orientation
   public void SetSymbol(string orientation)
    {
        tmpText.text = "C"; // Ensure the symbol is set to a Landolt C

        switch (orientation)
        {
            case "Up":
                tmpText.rectTransform.localEulerAngles = Vector3.forward * 90;
                break;
            case "Down":
                tmpText.rectTransform.localEulerAngles = Vector3.forward * 270;
                break;
            case "Left":
                tmpText.rectTransform.localEulerAngles = Vector3.forward * 180;
                break;
            case "Right":
                tmpText.rectTransform.localEulerAngles = Vector3.forward * 0;
                break;
            default:
                UnityEngine.Debug.LogWarning("Unknown orientation specified.");
                tmpText.rectTransform.localEulerAngles = Vector3.zero; // Default fallback
                break;
        }

        previousZEuler = tmpText.rectTransform.localEulerAngles.z;
        previousText = tmpText.text;
    }

}
