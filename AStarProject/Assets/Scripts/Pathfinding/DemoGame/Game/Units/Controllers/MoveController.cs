using Assets.Scripts.Game;
using Game.Units.Views;
using UnityEngine;

namespace Game.Units.Controllers
{
    public class MoveController : MonoBehaviour
    {
        public Rigidbody2D PhysicBody;
        public UnitController UnitController;
        public UnitView View;
        
        [Header("Move Settings")]
        public bool IsInputAllowed = true;
        public bool CanMove = true;
        public bool IsScaledMoveSpeed = true;
        public float Sensivity = 1f;
        public float MoveSpeed = 5f;
        
        private Vector2 _touchStartPosition;
        
        private float _scaledMoveSpeed
        {
            get
            {
                var val = IsScaledMoveSpeed? MoveSpeed * (Vector2.Distance(GameInput.Instance.TouchPosition, _touchStartPosition) * Sensivity / Screen.width) : MoveSpeed;
                if (val > MoveSpeed)
                    val = MoveSpeed;
                return val;
            }
        }
        private void OnEnable()
        {
            GameInput.Instance.TouchStartEvent += OnTouchStart;
            GameInput.Instance.TouchEndEvent += OnTouchEnd;
            GameInput.Instance.TouchMoveZeroEvent += OnMoveZero;
        }

        private void OnDisable()
        {
            GameInput.Instance.TouchStartEvent -= OnTouchStart;
            GameInput.Instance.TouchEndEvent -= OnTouchEnd;
            GameInput.Instance.TouchMoveZeroEvent -= OnMoveZero;
        }

        private void FixedUpdate()
        {
            if (UnitController.IsDead)
                return;

            if (!IsInputAllowed)
                return;

            if (!CanMove)
                return;
            
            if (!GameInput.Instance.MoveZero && GameInput.Instance.TouchPressed)
            {
                var direction = (GameInput.Instance.TouchPosition - _touchStartPosition).normalized;

                Debug.Log("TOUCH START: " + _touchStartPosition + "\nTOUCH CURR: " + GameInput.Instance.TouchPosition);
                if (Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.y)) > GameInput.TapDistanceRatioThreshold)
                {
                    View.LookAt(direction);
                    PhysicBody.velocity = direction * _scaledMoveSpeed;
                    View.SetSpeed(_scaledMoveSpeed);
                }
            }
            else
            {
                View.SetSpeed(0);
                PhysicBody.velocity = Vector2.zero;
            }
        }
        
        private void OnTouchStart(Vector2 position)
        {
            if (UnitController.IsDead)
                return;

            _touchStartPosition = position;
        }
		
        private void OnTouchEnd(Vector2 position)
        {
            if (UnitController.IsDead)
                return;
            
            View.SetSpeed(0);
        }
        
        private void OnMoveZero(Vector2 position)
        {
            if (UnitController.IsDead)
                return;
        }
    }
}