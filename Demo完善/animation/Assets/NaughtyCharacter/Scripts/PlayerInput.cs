using UnityEngine;

namespace NaughtyCharacter
{
	public class PlayerInput : MonoBehaviour
	{
		public float MoveAxisDeadZone = 0.2f;

		public Vector2 MoveInput { get; private set; }
		public Vector2 LastMoveInput { get; private set; }
		public Vector2 CameraInput { get; private set; }
        public bool JumpInput { get; private set; }
        public bool SprintInput { get; private set; }
        public bool WalkInput { get; private set; }
        public bool CrouchInput { get; private set; }
        public bool ReloadInput { get; private set; }

		public bool HasMoveInput { get; private set; }

		public void UpdateInput()
		{
			// Update MoveInput
			Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
			if (Mathf.Abs(moveInput.x) < MoveAxisDeadZone)
			{
				moveInput.x = 0.0f;
			}

			if (Mathf.Abs(moveInput.y) < MoveAxisDeadZone)
			{
				moveInput.y = 0.0f;
			}

			bool hasMoveInput = moveInput.sqrMagnitude > 0.0f;

			if (HasMoveInput && !hasMoveInput)
			{
				LastMoveInput = MoveInput;
			}

			MoveInput = moveInput;
			HasMoveInput = hasMoveInput;
            JumpInput = Input.GetKeyUp(KeyCode.Space);
            SprintInput = Input.GetKey(KeyCode.LeftShift);
            WalkInput = Input.GetKey(KeyCode.LeftControl);
            CrouchInput = Input.GetKeyUp(KeyCode.C);
            ReloadInput = Input.GetKeyUp(KeyCode.R);

            UpdateCameraInput();
            UpdateTimeScale();
		}

        void UpdateCameraInput()
        {
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                CameraInput = new Vector2(mouseX, mouseY);
            }
            else
            {
                CameraInput = new Vector2(0, 0);
            }
        }

        void UpdateTimeScale()
        {
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                Time.timeScale += 0.1f;
            }
            else if (Input.GetKeyDown(KeyCode.Minus))
            {
                Time.timeScale -= 0.1f;
            }
        }

    }
}
