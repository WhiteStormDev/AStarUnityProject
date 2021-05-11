using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts.Game
{
	public enum FollowType
	{
		Lerp,
		Damp
	}
    [RequireComponent(typeof(Camera))]
    public class GameCamera : MonoBehaviour
    {
		public FollowType FollowType = FollowType.Damp;
		public Vector3 TargetOffset;
        public float ForwardXOffset;
        public Transform Target;
        public float SmoothTimeOnInit;
        public float SmoothTime;
        public bool ClampToViewZone = true;
        public float SafeZonePercent = 0.25f;
        public Bounds ViewZone = new Bounds(Vector3.zero, new Vector3(15f, 10f, 0f));
        public Camera OrthographicCamera { get; private set; }

        public Vector2 TargetPosition => _targetPosition;

        private Vector3 _currentVelocity;
        private Vector2 _targetPosition;
       
        private float _currentXOffset;
        private Bounds _currentViewZone;
		private float _currentSmoothTime;
        void Awake()
        {
            _currentViewZone = ViewZone;
            OrthographicCamera = GetComponent<Camera>();
            _currentSmoothTime = SmoothTime;
        }
        private void Start()
        {
			//TargetAnimatorFlip = Target.GetComponent<FlipAnimator>();
			GameInput.SafeZonePercent = SafeZonePercent;
        }
        void FixedUpdate()
        {
            if (Target != null)
            {
	            Vector3 targetPosition = Target.position + TargetOffset + new Vector3(_currentXOffset, 0, -10f);
                
                if (ClampToViewZone)
                    targetPosition = ClampCameraPosition(targetPosition);
                
				if (FollowType == FollowType.Damp)
					transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, _currentSmoothTime, Mathf.Infinity, Time.fixedDeltaTime);
				else
				if (FollowType == FollowType.Lerp)
					transform.position = Vector3.Lerp(transform.position, targetPosition, _currentSmoothTime * Time.fixedDeltaTime);
                   
                _targetPosition = targetPosition;
            }
        }
        
        public void SetPosition(Vector2 position)
        {
            transform.position = ClampCameraPosition(new Vector3(position.x, position.y, -10f));
        }

        private Vector3 ClampCameraPosition(Vector3 position, float zOffset = -100f)
        {
            float orthographicSize = OrthographicCamera.orthographicSize;
            float aspect = OrthographicCamera.aspect;

            var cameraSize = new Vector2(orthographicSize * aspect, orthographicSize);
            var min = (Vector2)_currentViewZone.min + cameraSize;
            var max = (Vector2)_currentViewZone.max - cameraSize;

            return new Vector3(
                Mathf.Clamp(position.x, min.x, max.x),
                Mathf.Clamp(position.y, min.y, max.y),
                zOffset);
        }

        public Rect GetViewWorldRect()
        {
            float height = 2f * OrthographicCamera.orthographicSize;
            float width = height * OrthographicCamera.aspect;

            var r = new Rect(0f, 0f, width, height);
            r.center = transform.position;

            return r;
        }

        public Vector3 ScreenToWorld(Vector2 screenPosition)
        {
            var position = OrthographicCamera.ScreenToWorldPoint(screenPosition);
            position.z = 0f;

            return position;
        }

        public void ResetCameraViewZone()
        {
            _currentViewZone = ViewZone;
        }
        public void SetCameraViewZone(Bounds viewZone)
        {
            _currentViewZone = viewZone;
        }

#if UNITY_EDITOR
        private  void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(ViewZone.center, ViewZone.size);
            Gizmos.color = Color.red;
           
            Gizmos.DrawLine(
                new Vector3(ViewZone.min.x, ViewZone.min.y + ViewZone.size.y * SafeZonePercent), 
                new Vector3(ViewZone.max.x, ViewZone.max.y - ViewZone.size.y * (1 - SafeZonePercent))
                );
        }
#endif
    }
}
