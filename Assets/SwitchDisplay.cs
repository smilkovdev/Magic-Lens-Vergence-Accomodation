using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwitchDisplay : MonoBehaviour
{
    public List<Camera> CamerasTargetDisplay;

    [SerializeField]
    Canvas canvas;
    [SerializeField]
    TMPro.TMP_Dropdown dropdown; 

    private void OnEnable()
    {
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        if (dropdown == null)
        {
            dropdown = GetComponent<TMPro.TMP_Dropdown>();
        }

        dropdown.AddOptions(Display.displays.Select((d, i) => $"Display {i} ({d.renderingWidth}x{d.renderingHeight})").ToList());
        dropdown.SetValueWithoutNotify(canvas.targetDisplay);
    }

    public void ChangeDisplay(int displayIndex)
    {
        foreach(var display in FindObjectsOfType<SwitchDisplay>())
        {
            display.ChangeDisplayWithoutNotify(canvas.targetDisplay, displayIndex);
        }

        ChangeDisplayWithoutNotify(displayIndex);
    }

    public void ChangeDisplayWithoutNotify(int displayIndex, int? targetDisplay= null)
    {
        if (targetDisplay != null && targetDisplay != canvas.targetDisplay)
            return;

        canvas.targetDisplay = displayIndex;
        dropdown.SetValueWithoutNotify(displayIndex);

        if (CamerasTargetDisplay != null && CamerasTargetDisplay.Any())
        {
            CamerasTargetDisplay.ForEach(c => c.targetDisplay = displayIndex);
        }
    }
}
