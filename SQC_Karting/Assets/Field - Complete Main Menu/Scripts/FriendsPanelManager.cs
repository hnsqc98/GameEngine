using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Michsky.UI.FieldCompleteMainMenu
{
    public class FriendsPanelManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private Animator panelAnimator;
        private CanvasGroup cg;
        private bool isOpen = false;
        public bool isMobile;

        void Start()
        {
            panelAnimator = this.GetComponent<Animator>();
            cg = this.GetComponent<CanvasGroup>();

            if(isMobile == true)
            {
                cg.interactable = false;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            panelAnimator.Play("Friends Panel In");
        }

         public void OnPointerClick(PointerEventData eventData)
        {            
            if(isOpen == true)
            {
                panelAnimator.Play("Friends Panel Out");
                isOpen = false;
            }

            else
            {
                panelAnimator.Play("Friends Panel In");
                isOpen = true;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            panelAnimator.Play("Friends Panel Out");
        }
    }
}