using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class EyeTestSurface : MonoBehaviour
{
    public SurfaceType surfaceType;
    public LetterType letterType;
    public TMP_Text tmpText;
    public int order;
    public UnityEngine.UI.Image mazeImageUI;
    public Sprite mazeImageTop;
    public Sprite mazeImageMiddle;
    public Sprite mazeImageBottom;

    readonly string[] sloanLetters = { "C", "D", "H", "K", "N", "O", "R", "S", "V" };
    private readonly List<string> orientations = new List<string> { "Up", "Down", "Left", "Right" };

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
        LANDHOLT,
        MAZE
    }

    // Keep EyeTestSurface focused on its own logic only
    void Start()
    {
        // If needed, initialize something here
    }

    public void NextRandomSymbol(float visibleSeconds, int round, List<string> orientationsToUse)
    {
        switch (letterType)
        {
            case LetterType.SLOAN:
                tmpText.enabled = true;
                mazeImageUI.enabled = false;
                tmpText.text = sloanLetters[Random.Range(0, sloanLetters.Length)];
                tmpText.rectTransform.localEulerAngles = Vector3.zero;
                break;

            case LetterType.LANDHOLT:
                tmpText.enabled = true;
                mazeImageUI.enabled = false;
                string orientation = orientationsToUse[order % orientationsToUse.Count];
                SetOrientation(orientation);
                tmpText.text = "C";
                break;

            case LetterType.MAZE:
                // Maze display logic might be handled differently by the ExperimentDriver
                // Typically, Maze logic sets the image externally via SetMazeImage.
                tmpText.enabled = false;
                mazeImageUI.enabled = true;
                // If you were to handle it here, pick the correct sprite as done before:
                // But generally, Maze images get set by the driver, not here.
                break;
        }

        previousZEuler = tmpText.rectTransform.localEulerAngles.z;
        previousText = tmpText.text;

        // Only clear automatically if we have a positive visibleSeconds and are not Maze
        if (visibleSeconds > 0f && letterType != LetterType.MAZE)
            Invoke(nameof(Clear), visibleSeconds);
    }

    private void SetOrientation(string orientation)
    {
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
                tmpText.rectTransform.localEulerAngles = Vector3.zero; 
                break;
        }
    }

    public void Clear()
    {
        if (letterType == LetterType.MAZE)
        {
            mazeImageUI.enabled = false;
            mazeImageUI.sprite = null;
        }
        else
        {
            tmpText.text = order == 1 ? "?" : string.Empty;
            tmpText.color = Color.red;
            tmpText.rectTransform.localEulerAngles = Vector3.zero;
        }
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
            bool down = Input.GetKeyUp(KeyCode.DownArrow);
            bool up = Input.GetKeyUp(KeyCode.UpArrow);
            bool left = Input.GetKeyUp(KeyCode.LeftArrow);
            bool right = Input.GetKeyUp(KeyCode.RightArrow);
            bool isLandholt = letterType == LetterType.LANDHOLT;

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

    public void SetSymbol(string orientation)
    {
        tmpText.text = "C"; 
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
                tmpText.rectTransform.localEulerAngles = Vector3.zero; 
                break;
        }

        previousZEuler = tmpText.rectTransform.localEulerAngles.z;
        previousText = tmpText.text;
    }

    public void SetMazeImage(Sprite mazeSprite)
    {
        letterType = LetterType.MAZE;
        tmpText.enabled = false;  
        mazeImageUI.enabled = true;
        mazeImageUI.sprite = mazeSprite;
        mazeImageUI.color = Color.white;
    }

    public void ShowMazeFeedback(bool isCorrect)
    {
        mazeImageUI.color = isCorrect ? Color.green : Color.gray;
    }
}
