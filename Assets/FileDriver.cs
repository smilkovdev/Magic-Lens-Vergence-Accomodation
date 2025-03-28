using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EyeTestSurface;

public class FileDriver : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField visibleSecondsInput;
    public TMP_Dropdown accommodationDropdown;
    public TMP_Dropdown vergenceDropdown;
    public Slider nearScaleSlider;
    public Slider farScaleSlider;
    public Slider depthSlider;

    private string desktopPath;

    void Start()
    {
        // Get the Desktop path for the current user
        desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MazeGameResults");
        
        // Ensure the folder exists
        if (!Directory.Exists(desktopPath))
        {
            Directory.CreateDirectory(desktopPath);
        }
    }

    public void WriteToFile(int displayNum, SurfaceType displayType, string question, string answer, bool correct, int round, double answerTimeSeconds, LetterType letterType)
    {
        if (string.IsNullOrEmpty(nameInput.text))
        {
            Debug.LogError("No filename provided!");
            return;
        }

        // Construct the full file path on Desktop
        string fileName = $"{nameInput.text}.csv";
        string filePath = Path.Combine(desktopPath, fileName);

        // Create header if file does not exist
        if (!File.Exists(filePath))
        {
            File.AppendAllLines(filePath, new string[] 
            { 
                "sep=;",
                "datetime; name; accommodation; vergence; visible_seconds; display_num; display_type; question; answer; correct; round; answer_seconds; letter_type; near_scale; far_scale; depth" 
            });
        }

        // Format values
        var values = new string[]
        {
            DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
            nameInput.text,
            ((SurfaceType)accommodationDropdown.value).ToString(),
            ((SurfaceType)vergenceDropdown.value).ToString(),
            visibleSecondsInput.text,
            displayNum.ToString(),
            displayType.ToString(),
            question,
            answer,
            correct ? "1" : "0",
            round.ToString(),
            answerTimeSeconds.ToString("F3"), // Format time to 3 decimal places
            letterType.ToString(),
            nearScaleSlider.value.ToString(),
            farScaleSlider.value.ToString(),
            depthSlider.value.ToString(),
        };

        // Append to file
        File.AppendAllLines(filePath, new string[] { string.Join(";", values) });

        Debug.Log($"File saved to: {filePath}");
    }
}