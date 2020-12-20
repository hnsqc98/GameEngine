using UnityEngine;

namespace Michsky.UI.FieldCompleteMainMenu
{
    public class ExitToSystem : MonoBehaviour
    {
        public void ExitGame()
        {
            Debug.Log("It's working :)");
            Application.Quit();
        }
    }
}