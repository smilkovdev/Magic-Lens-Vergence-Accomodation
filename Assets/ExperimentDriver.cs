using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using TMPro;
using Unity.VisualScripting;
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
    //public int maxRounds;
    public FileDriver fileOutput;
    public TMP_Dropdown vergenceDropdown;
    public Slider vergenceSlider;
    public TMP_Dropdown letterTypeDropdown;
    public ActivateAllDisplays displayManager;
    public TMP_Dropdown accommodationDropdown;

    Coroutine procedureCoroutine;
    int round;
    const int MAX_ROUNDS = 30;

    // Define orientations for Landolt C shapes
    private readonly List<string> orientations = new List<string> { "Up", "Down", "Left", "Right" };

    float nearDisparity
    {
        get
        {
            if(accommodationDropdown.value == 0) // NEAR
            {
                return 0f;
            }
            else // FAR
            {
                return displayManager.NearDistance - displayManager.FarDistance;
            }
        }
    }
    float farDisparity
    {
        get
        {
            if (accommodationDropdown.value == 0) // NEAR
            {
                return displayManager.FarDistance - displayManager.NearDistance;
            }
            else // FAR
            {
                return 0f;
            }
        }
    }
    float visualizeTimeDecrement => -float.Parse(visualizeTimeIncrementInput.text);

    //bool calibrated;
    TimeSpan highScore = TimeSpan.MaxValue;

    // Start is called before the first frame update
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
        highScoreText.text = String.Empty;

        CancelInvoke(nameof(RepeatingRoutine));

        if (timeout < 0f)
        {
            // Do not automatically repeat
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
        if (round >= MAX_ROUNDS)  // Stop after 30 rounds
        {
            CancelInvoke(nameof(RepeatingRoutine));
            return;
        }

        if (procedureCoroutine != null)
            StopCoroutine(procedureCoroutine);

        procedureCoroutine = StartCoroutine(VisualizeRandomLettersForTime());
    }



    IEnumerator VisualizeRandomLettersForTime()
    {
        if (round >= MAX_ROUNDS) // Stop execution after 30 rounds
            yield break;

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

        // Precompute orientations for all surfaces
        List<string> orientationsToUse;
        if (round <= 10)
        {
            orientationsToUse = GetEasySymbols(eyeTestSurfaces.Length);
        }
        else if (round <= 20)
        {
            orientationsToUse = GetMediumSymbols(eyeTestSurfaces.Length);
        }
        else
        {
            orientationsToUse = GetHardSymbols(eyeTestSurfaces.Length);
        }

        // Assign symbols to each surface
        foreach (var ets in eyeTestSurfaces)
        {
            ets.NextRandomSymbol(time, round, orientationsToUse);
        }

        // ---

        var timeElapse = new Stopwatch();
        timeElapse.Start();

        //var elapsedTimes = new Dictionary<int, double>();
        foreach (var ets in eyeTestSurfaces.OrderBy(e => e.order))
        {
            if (time >= 0f)
            {

                if (letterTypeDropdown.value == (int)LetterType.SLOAN)
                {
                    yield return ets.WaitForLetterInput("?");
                    fileOutput.WriteToFile(ets.order, ets.surfaceType, ets.Question, ets.Answer, ets.IsCorrect, round, timeElapse.Elapsed.TotalSeconds, ets.letterType);
                }
                else
                {
                    yield return ets.WaitForArrowInput(displayText: "?");
                    fileOutput.WriteToFile(ets.order, ets.surfaceType, ets.Question, ets.Answer, ets.IsCorrect, round, timeElapse.Elapsed.TotalSeconds, ets.letterType);
                }
            }

            yield return null;
        }

        if(time < 0f)
        {
            yield return WaitForSpace();
            timeElapse.Stop();
            foreach (var ets in eyeTestSurfaces.OrderBy(e => e.order))
            ets.Clear();

            foreach (var ets in eyeTestSurfaces.OrderBy(e => e.order))
            {
                if (letterTypeDropdown.value == (int)LetterType.SLOAN)
                {
                    yield return ets.WaitForLetterInput("?");
                }
                else
                {
                    yield return ets.WaitForArrowInput(displayText: "?");
                }
                fileOutput.WriteToFile(ets.order, ets.surfaceType, ets.Question, ets.Answer, ets.IsCorrect, round, timeElapse.Elapsed.TotalSeconds, ets.letterType);

                yield return null;
            }

            if(timeElapse.Elapsed < highScore && eyeTestSurfaces.All(ets => ets.IsCorrect))
            {
                highScore = timeElapse.Elapsed;
                highScoreText.text = timeElapse.Elapsed.TotalSeconds.ToString("0.000");
            }
        }
        else
        {
            highScoreText.text = String.Empty;
            if(round % roundsPerVisualizeTime == 0)
            {
                time -= visualizeTimeDecrement;
                visualizeTimeInput.text = (time).ToString();
            }
        }

        if (round >= MAX_ROUNDS) // Stop after 30 rounds
        {
            highScoreText.text = "DONE";
            CancelInvoke(nameof(RepeatingRoutine));
            yield break;
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
        //vergenceObject.SetPositionZ(vergenceValue);
        //vergenceSlider.SetValueWithoutNotify(vergenceValue);
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
