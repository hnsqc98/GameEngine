using UnityEngine;

namespace NaughtyCharacter
{
	public static class CharacterAnimatorParamId
	{
        public static readonly int ForwardSpeed = Animator.StringToHash("ForwardSpeed");
        public static readonly int RightSpeed = Animator.StringToHash("RightSpeed");

        public static readonly int MovementState = Animator.StringToHash("MovementState");
        public static readonly int UpperState = Animator.StringToHash("UpperState");
        public static readonly int IdlePose = Animator.StringToHash("IdlePose");

		public static readonly int IsSprinting = Animator.StringToHash("IsSprintng");
	}

	public class CharacterAnimator : MonoBehaviour
	{
		private Animator _animator;
		private Character _character;
		private float _currentUpperbodyWeight;
		private float _targetUpperbodyWeight;
		private float _currentDampVelocity;
        public Transform GunHandTransform;

		public bool UseRootMotion { get { return _animator.applyRootMotion; } }

		private void Awake()
		{
			_animator = GetComponentInChildren<Animator>();
			_character = GetComponent<Character>();
		}

		public void SetUpperBodyWeight(float weight)
		{
			_targetUpperbodyWeight = weight;
		}

        private void OnAnimatorIK(int layerIndex)
        {
            if (layerIndex == 1 && GunHandTransform)
            {
                _animator.SetIKPosition(AvatarIKGoal.LeftHand, GunHandTransform.position);
                _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
            }
        }

        public void UpdateState()
		{
			_currentUpperbodyWeight = Mathf.SmoothDamp(_currentUpperbodyWeight, _targetUpperbodyWeight, ref _currentDampVelocity, 0.2f);
			_animator.SetLayerWeight(1, _currentUpperbodyWeight);

            Vector3 velocity = _character.HorizontalVelocity;
            float forwardSpeed = Vector3.Dot(_character.transform.forward, velocity);
            float rightSpeed = Vector3.Dot(_character.transform.right, velocity);

            _animator.SetFloat(CharacterAnimatorParamId.ForwardSpeed, forwardSpeed, 0.15f, Time.deltaTime);
            _animator.SetFloat(CharacterAnimatorParamId.RightSpeed, rightSpeed, 0.15f, Time.deltaTime);
            _animator.SetInteger(CharacterAnimatorParamId.MovementState, (int)_character.MovementState);
            _animator.SetInteger(CharacterAnimatorParamId.UpperState, (int)_character.UpperState);
		}
	}
}
