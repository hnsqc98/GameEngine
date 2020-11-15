using UnityEngine;
using System.Collections;

public class DynamicCircuitToggle : MonoBehaviour 
{
    public DynamicCircuit[] displays;
    private int[] numLines;
    private int currentIndex = -1;

    void Start()
    {
        numLines = new int[displays.Length];
        for (int i = 0; i < displays.Length; i++)
            numLines[i] = displays[i].numberOfLines;
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10f, 10f, 140f, 50f), "Switch Displays"))
            SwitchDisplays();
    }

    private void SwitchDisplays()
    {
        currentIndex = (currentIndex + 1) % displays.Length;

        for (int i = 0; i < displays.Length; i++)
        {
            if (i == currentIndex)
            {
                displays[i].numberOfLines = numLines[i];
                displays[i].enabled = true;
            }
            else
                displays[i].numberOfLines = 0;
        }
    }

}
