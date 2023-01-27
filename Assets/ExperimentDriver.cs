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
    public TMP_Text countdownTimerText;
    public TMP_Text highScoreText;
    public int countdownTime;
    public float timeout;
    public int roundsPerVisualizeTime;
    public int maxRounds;
    public float visualizeTimeDecrement;
    public FileDriver fileOutput;
    public TMP_Dropdown vergenceDropdown;
    public MoveMe vergenceObject;
    public Slider vergenceSlider;
    public TMP_Dropdown letterTypeDropdown;
    public ActivateAllDisplays displayManager;
    public TMP_Dropdown accommodationDropdown;

    Coroutine procedureCoroutine;
    int round;
    readonly List<SurfaceType> readonlySurfaceTypes = new List<SurfaceType>();
    List<SurfaceType> roundVergence = new List<SurfaceType>();

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

    //bool calibrated;
    TimeSpan highScore = TimeSpan.MaxValue;

    // Start is called before the first frame update
    void Start()
    {
        countdownTimerText.text = string.Empty;
        highScoreText.text = string.Empty;

        for (int i = 0; i < roundsPerVisualizeTime / 2; i++)
        {
            readonlySurfaceTypes.Add(SurfaceType.NEAR);
            readonlySurfaceTypes.Add(SurfaceType.FAR);
        }

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
        roundVergence = new List<SurfaceType>(readonlySurfaceTypes);

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
        if (procedureCoroutine != null)
            StopCoroutine(procedureCoroutine);

        procedureCoroutine = StartCoroutine(VisualizeRandomLettersForTime());
    }


    IEnumerator VisualizeRandomLettersForTime()
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
        
        // --- Set independent variables
        foreach(var ets in eyeTestSurfaces)
            ets.NextRandomSymbol(time);

        var randomIdx = Random.Range(0, roundVergence.Count);//Random.Range(0, 2);
        var nearOrFar = roundVergence.ElementAt(randomIdx);//randomIdx == 0 ? SurfaceType.NEAR : SurfaceType.FAR;
        roundVergence.RemoveAt(randomIdx);
        vergenceDropdown.value = randomIdx;
        vergenceSlider.value = nearOrFar == SurfaceType.NEAR ? nearDisparity : farDisparity;

        SetVergenceNearOrFar(randomIdx);

        // ---

        var timeElapse = new Stopwatch();
        timeElapse.Start();

        //var elapsedTimes = new Dictionary<int, double>();
        foreach (var ets in eyeTestSurfaces.OrderBy(e => e.order))
        {
            if (letterTypeDropdown.value == (int)LetterType.SLOAN)
            {
                if (time < 0f)
                {
                    //yield return ets.WaitForSpace();
                    //elapsedTimes[ets.order] = timeElapse.Elapsed.TotalSeconds;
                }
                else
                {
                    yield return ets.WaitForLetterInput("?");
                    fileOutput.WriteToFile(ets.order, ets.surfaceType, ets.Question, ets.Answer, ets.IsCorrect, round, timeElapse.Elapsed.TotalSeconds, ets.letterType);
                }
            }
            else
            {
                yield return ets.WaitForArrowInput(displayText: "?");
                fileOutput.WriteToFile(ets.order, ets.surfaceType, ets.Question, ets.Answer, ets.IsCorrect, round, timeElapse.Elapsed.TotalSeconds, ets.letterType);
            }

            yield return null;
        }

        if(time < 0f)
        {
            if (letterTypeDropdown.value == (int)LetterType.SLOAN)
            {
                yield return WaitForSpace();
                timeElapse.Stop();
                foreach (var ets in eyeTestSurfaces.OrderBy(e => e.order))
                    ets.Clear();
                foreach (var ets in eyeTestSurfaces.OrderBy(e => e.order))
                {
                    yield return ets.WaitForLetterInput("?");
                    fileOutput.WriteToFile(ets.order, ets.surfaceType, ets.Question, ets.Answer, ets.IsCorrect, round, timeElapse.Elapsed.TotalSeconds, ets.letterType);
                }
            }
            else
            {
                timeElapse.Stop();
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
                visualizeTimeInput.text = (time - visualizeTimeDecrement).ToString();
            }
        }
        /*
        if (letterTypeDropdown.value == (int)LetterType.SLOAN && time < 0f)
        {
            foreach (var ets in eyeTestSurfaces.OrderBy(e => e.order))
            {
                yield return ets.WaitForLetterInput("?");
                fileOutput.WriteToFile(ets.order, ets.surfaceType, ets.Question, ets.Answer, ets.IsCorrect, round, elapsedTimes[ets.order], ets.letterType);
            }

            if (elapsedTimes[2] < highScore.TotalSeconds && eyeTestSurfaces.All(ets => ets.IsCorrect))
            {
                var decimalPart = elapsedTimes[2].ToString().Split('.')[1];
                highScore = new TimeSpan(0, 0, 0, (int)elapsedTimes[0], int.Parse(decimalPart));
                highScoreText.text = timeElapse.Elapsed.TotalSeconds.ToString("0.000");
            }
        }*/

        if (round % roundsPerVisualizeTime == 0)
        {
            roundVergence = new List<SurfaceType>(readonlySurfaceTypes);
        }

        if (timeout < 0f)
        {
            if (round >= maxRounds)
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
        //if (calibrated)
            vergenceObject.SetPositionZ((SurfaceType)index == SurfaceType.NEAR ? nearDisparity : farDisparity);
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
