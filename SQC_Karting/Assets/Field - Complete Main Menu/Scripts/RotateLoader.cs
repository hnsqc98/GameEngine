using UnityEngine;

namespace Michsky.UI.FieldCompleteMainMenu
{
    public class RotateLoader : MonoBehaviour
    {
        public float time = 1.0f;
        public float repeatRate = 1.0f;

        void Start()
        {
            InvokeRepeating("LoaderRotate", time, repeatRate);
        }

        void LoaderRotate()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.Rotate(new Vector3(0, 0, -30));

            // rectTransform.Rotate(Vector3.forward * (speed * Time.deltaTime));
        }
    }
}