using UnityEngine;

namespace Michsky.UI.FieldCompleteMainMenu
{
    public class DevConsoleUI : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField]
        private GameObject consoleObject;
        [SerializeField]
        private KeyCode hotkey = KeyCode.F12;

        private bool isOn;
        private bool isReady;

        private void Start()
        {
            SetVisible(false);
            isReady = true;
        }

        private void Update()
        {
            if (Input.GetKeyDown(hotkey))
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            SetVisible(!isOn);
        }

        public void SetVisible(bool visible)
        {
            consoleObject.SetActive(visible);
            isOn = visible;
        }

    }
}