using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Michsky.UI.FieldCompleteMainMenu
{
    public class ListTabsManagerWOB : MonoBehaviour
    {
        [Header("PANEL LIST")]
        public List<GameObject> panels = new List<GameObject>();

        // [Header("PANEL ANIMS")]
        private string panelFadeIn = "MP Panel Fade-in";
        private string panelFadeOut = "MP Panel Fade-out";

        private GameObject currentPanel;
        private GameObject nextPanel;

        [Header("SETTINGS")]
        public int currentPanelIndex = 0;

        private Animator currentPanelAnimator;
        private Animator nextPanelAnimator;


        void Start()
        {
            currentPanel = panels[currentPanelIndex];
            currentPanelAnimator = currentPanel.GetComponent<Animator>();
            currentPanelAnimator.Play(panelFadeIn);
        }

        public void PanelAnim(int newPanel)
        {
            if (newPanel != currentPanelIndex)
            {
                currentPanel = panels[currentPanelIndex];

                currentPanelIndex = newPanel;
                nextPanel = panels[currentPanelIndex];

                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                nextPanelAnimator = nextPanel.GetComponent<Animator>();

                currentPanelAnimator.Play(panelFadeOut);
                nextPanelAnimator.Play(panelFadeIn);
            }
        }
    }
}