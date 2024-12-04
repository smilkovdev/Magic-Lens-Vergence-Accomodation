using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EyeTestSurface;
using Random = UnityEngine.Random;

public class ExperimentDriver : MonoBehaviour
{
    public TMP_InputField visualizeTimeInput;
    public TMP_InputField visualizeTimeIncrementInput;
    public TMP_InputField visualizeTimeMinInput;
    public TMP_Text countdownTimerText;
    public TMP_Text highScoreText;
    public int countdownTime;
    public float timeout;
    public int roundsPerVisualizeTime;
    public FileDriver fileOutput;
    public TMP_Dropdown vergenceDropdown;
    public Slider vergenceSlider;
    public TMP_Dropdown letterTypeDropdown;
    public ActivateAllDisplays displayManager;
    public TMP_Dropdown accommodationDropdown;

    Coroutine procedureCoroutine;
    int round;

    // Define orientations for Landolt C shapes
    private readonly List<string> orientations = new List<string> { "Up", "Down", "Left", "Right" };

    float nearDisparity
    {
        get
        {
            return accommodationDropdown.value == 0 ? 0f : displayManager.NearDistance - displayManager.FarDistance;
        }
    }

    float farDisparity
    {
        get
        {
            return accommodationDropdown.value == 0 ? displayManager.FarDistance - displayManager.NearDistance : 0f;
        }
    }

    float visualizeTimeDecrement => -float.Parse(visualizeTimeIncrementInput.text);
    TimeSpan highScore = TimeSpan.MaxValue;

    void Start()
    {
        countdownTimerText.text = string.Empty;
        highScoreText.text = string.Empty;
        ChangeAccommodation(accommodationDropdown.value);
    }

    public void StartProcedure()
    {
        if (!float.TryParse(visualizeTimeInput.text, out float time))
            return;

        if (fileOutput == null || string.IsNullOrEmpty(fileOutput.nameInput?.text))
            return;

        round = 0;
        highScore = TimeSpan.MaxValue;
        highScoreText.text = string.Empty;

        CancelInvoke(nameof(RepeatingRoutine));

        if (timeout < 0f)
        {
            RepeatingRoutine();
        }
        else
        {
            var repeatRate = time + countdownTime + timeout;
            InvokeRepeating(nameof(RepeatingRoutine), 0f, repeatRate);
        }
    }

    void RepeatingRoutine()
    {
        if (procedureCoroutine != null)
            StopCoroutine(procedureCoroutine);

        procedureCoroutine = StartCoroutine(VisualizeControlledLettersForTime());
    }

IEnumerator VisualizeControlledLettersForTime()
{
    if (!float.TryParse(visualizeTimeInput.text, out float time))
        yield break;

    round++;
    var eyeTestSurfaces = FindObjectsOfType<EyeTestSurface>();
    foreach (var ets in eyeTestSurfaces)
        ets.Clear();

    for (int i = 0; i < countdownTime; i++)
    {
        countdownTimerText.text = (countdownTime - i).ToString();
        yield return new WaitForSeconds(0.5f);
    }

    countdownTimerText.text = string.Empty;

    // Get symbols based on the current round (difficulty)
    List<string> symbolsToDisplay;
    if (round <= 10)
    {
        symbolsToDisplay = GetEasySymbols(eyeTestSurfaces.Length);
    }
    else if (round <= 20)
    {
        symbolsToDisplay = GetMediumSymbols(eyeTestSurfaces.Length);
    }
    else
    {
        symbolsToDisplay = GetHardSymbols(eyeTestSurfaces.Length);
    }

    // Display all symbols for a second
    for (int i = 0; i < eyeTestSurfaces.Length; i++)
    {
        eyeTestSurfaces[i].SetSymbol(symbolsToDisplay[i]);
    }
    yield return new WaitForSeconds(1f);

    // Clear the symbols after showing them
    foreach (var ets in eyeTestSurfaces)
        ets.Clear();

    var timeElapse = new Stopwatch();
    timeElapse.Start();

    // Wait for separate user input for each EyeTestSurface
    foreach (var ets in eyeTestSurfaces)
    {
        yield return StartCoroutine(WaitForSingleInput(ets));
        fileOutput.WriteToFile(ets.order, ets.surfaceType, ets.Question, ets.Answer, ets.IsCorrect, round, timeElapse.Elapsed.TotalSeconds, ets.letterType);
    }

    timeElapse.Stop();

    if (time < 0f)
    {
        yield return WaitForSpace();
        foreach (var ets in eyeTestSurfaces)
            ets.Clear();

        if (timeElapse.Elapsed < highScore && eyeTestSurfaces.All(ets => ets.IsCorrect))
        {
            highScore = timeElapse.Elapsed;
            highScoreText.text = timeElapse.Elapsed.TotalSeconds.ToString("0.000");
        }
    }
    else
    {
        highScoreText.text = string.Empty;
        if (round % roundsPerVisualizeTime == 0)
        {
            time -= visualizeTimeDecrement;
            visualizeTimeInput.text = time.ToString();
        }
    }

    if (timeout < 0f)
    {
        var minTime = float.Parse(visualizeTimeMinInput.text);
        if (time <= minTime && (round % roundsPerVisualizeTime == 0))
        {
            highScoreText.text = "DONE";
        }
        else
        {
            yield return new WaitForSeconds(1f);
            RepeatingRoutine();
        }
    }
}

IEnumerator WaitForSingleInput(EyeTestSurface ets)
{
    bool inputReceived = false;

    while (!inputReceived)
    {
        if (letterTypeDropdown.value == (int)EyeTestSurface.LetterType.SLOAN)
        {
            yield return StartCoroutine(ets.WaitForLetterInput("?"));
        }
        else
        {
            yield return StartCoroutine(ets.WaitForArrowInput("?"));
        }

        // Confirm input was received for the specific EyeTestSurface
        if (!string.IsNullOrEmpty(ets.Answer))
        {
            inputReceived = true;
        }

        yield return null; // Allow a frame delay
    }
}





    private List<string> GetEasySymbols(int count)
    {
        string orientation = orientations[Random.Range(0, orientations.Count)];
        return Enumerable.Repeat(orientation, count).ToList();
    }

    private List<string> GetMediumSymbols(int count)
    {
        List<string> result = new List<string>();
        string firstOrientation = orientations[Random.Range(0, orientations.Count)];
        string secondOrientation;
        do
        {
            secondOrientation = orientations[Random.Range(0, orientations.Count)];
        } while (secondOrientation == firstOrientation);

        for (int i = 0; i < count; i++)
        {
            result.Add(i % 2 == 0 ? firstOrientation : secondOrientation);
        }
        return result;
    }

    private List<string> GetHardSymbols(int count)
    {
        List<string> result = new List<string>();
        for (int i = 0; i < count; i++)
        {
            result.Add(orientations[i % orientations.Count]);
        }
        return result;
    }

    public IEnumerator WaitForSpace()
    {
        while (!Input.GetKeyUp(KeyCode.Space))
        {
            yield return null;
        }
        yield return null;
    }

    public void SetVergenceNearOrFar(int index)
    {
        var vergenceValue = (SurfaceType)index == SurfaceType.NEAR ? nearDisparity : farDisparity;
        vergenceSlider.value = vergenceValue;
    }

    public void SavePersistantObjects()
    {
        var sub = ((SurfaceType)accommodationDropdown.value).ToString();
        var pp = FindObjectsOfType<PersistantPose>();
        var ps = FindObjectsOfType<PersistantSlider>();
        foreach (var o in pp)
            o.Save(sub);
        foreach (var o in ps)
            o.Save(sub);
    }

    public void ClearPersistantObjects()
    {
        var sub = ((SurfaceType)accommodationDropdown.value).ToString();
        var pp = FindObjectsOfType<PersistantPose>();
        var ps = FindObjectsOfType<PersistantSlider>();
        foreach (var o in pp)
            o.Clear(sub);
        foreach (var o in ps)
            o.Clear(sub);
    }

    public void ChangeAccommodation(int value)
    {
        var sub = ((SurfaceType)value).ToString();
        var pp = FindObjectsOfType<PersistantPose>();
        var ps = FindObjectsOfType<PersistantSlider>();
        foreach (var o in pp)
            o.Load(sub);
        foreach (var o in ps)
            o.Load(sub);
    }
}
