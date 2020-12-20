using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.FieldCompleteMainMenu
{
    public class LoginUserManager : MonoBehaviour
    {
        [Header("RESOURCES")]
        public SwitchToMainPanels switchPanelMain;
        public UIElementSound soundScript;
        public Animator wrongAnimator;
        public Text usernameText;
        public Text passwordText;

        [Header("SETTINGS")]
        public string username;
        public string password;

        public void Login()
        {
            if (usernameText.text == username && passwordText.text == password)
            {
                switchPanelMain.Animate();
            }
            else
            {
                wrongAnimator.Play("Notification In");
                soundScript.Notification();
            }
        }
    }
}