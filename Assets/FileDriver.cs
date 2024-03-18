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
    public string optionalPath = "";
    public TMP_InputField nameInput;
    public TMP_InputField visibleSecondsInput;
    public TMP_Dropdown accommodationDropdown;
    public TMP_Dropdown vergenceDropdown;
    public Slider nearScaleSlider;
    public Slider farScaleSlider;
    public Slider depthSlider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void WriteToFile(int displayNum, SurfaceType displayType, string question, string answer, bool correct, int round, double answerTimeSeconds, LetterType letterType)
    {
        // datetime; name; accommodation; vergence; visible_seconds; display_num; display_type; question; answer; correct; round; answer_seconds; letter_type; near_scale; far_scale; depth
        // 11/03/1990 10:59:20; Geert; NEAR; FAR; 1.5; 0; FAR; V; U; 0; 1; 0.987654321; SLOAN; 0.5; 1.2; 0.70
        var path = Path.Combine(Application.persistentDataPath, optionalPath, $"{nameInput.text}.csv");

        if (!File.Exists(path))
            File.AppendAllLines(path, new string[] { "sep=;", "datetime; name; accommodation; vergence; visible_seconds; display_num; display_type; question; answer; correct; round; answer_seconds; letter_type; near_scale; far_scale; depth" } );

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
            answerTimeSeconds.ToString(),
            letterType.ToString(),
            nearScaleSlider.value.ToString(),
            farScaleSlider.value.ToString(),
            depthSlider.value.ToString(),
        };

        File.AppendAllLines(path, new string[] { string.Join(";", values) });
    }
}
