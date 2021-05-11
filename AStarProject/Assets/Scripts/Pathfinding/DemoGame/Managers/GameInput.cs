using System;
using Base;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public enum SwipeDirection
{
	Right,
	Up,
	Left,
	Down
}

[DontDestroySingleton]
public class GameInput : MonoSingleton<GameInput>
{
	public delegate Vector2 StartPointClamp(Vector2 startPoint);
	public StartPointClamp StartPointClampAlg;

	private delegate void InputUpdate();


	private const int EmptyTouchFingerId = -1;
	private const int MouseTouchFingerId = 0;

	public static float MoveDistanceThreshold = 15f;
	private static float MoveDistanceThresholdSqr = MoveDistanceThreshold * MoveDistanceThreshold;

	public static float TapDistanceRatioThreshold = 0.014f;
	public static float TapLongDuration = 0.12f;
	public static float TapSuperLongDuration = 0.5f;

	public static float SafeZonePercent = 0.2f;

	private const float SwipeDetectInterval = 0.05f;
	public static float SwipeDistanceRatioThreshold = 0.04f;
	public static float SwipeMaxDuration = 0.4f;


	public event Action<Vector2> TouchStartEvent;
	public event Action<Vector2> TouchMoveEvent;
	public event Action<Vector2> TouchEndEvent;
	public event Action<Vector2> TouchMoveZeroEvent;
	//public event Action<Vector2> TouchMoveAboveZeroEvent;

	public event Action<Vector2, float> TapEvent; 
	public event Action<Vector2, float> LongTapEvent;
	public event Action<SwipeDirection> SwipeEvent;
	public event Action<Vector2, float> SuperLongTapEvent;

	public bool Active { get; private set; }

	public bool MoveZero { get { return new Vector2(_startTouchPosition.x - _lastTouchPosition.x, _startTouchPosition.y - _lastTouchPosition.y).sqrMagnitude <= MoveDistanceThresholdSqr; } }
	public bool TouchPressed { get { return _inGameTouchFingerId != EmptyTouchFingerId; } }
	public float TouchPressedTime { get; private set; }
	public Vector2 TouchPosition { get { return _lastTouchPosition; } }

	private bool _isLongTapped = false;
	private bool _isSuperLongTapped = false;
	private InputUpdate _inputUpdate;
	private int _inGameTouchFingerId = EmptyTouchFingerId;
	private Vector2 _startTouchPosition;
	private Vector2 _lastTouchPosition;
	private Vector2 _lastSwipePosition;
	private bool _swipped;
	private float _swipeDetectionTimer;
	private int _touchCount = 0;

	protected override void OnCreate()
	{
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
	_inputUpdate = UpdateTouches;
#else
		_inputUpdate = UpdateMouse;
#endif

		Active = true;
		_isLongTapped = false;
		_isSuperLongTapped = false;
		_swipeDetectionTimer = 0f;
	}

	private void OnSceneChanged(Scene pre, Scene next)
	{
		_inGameTouchFingerId = EmptyTouchFingerId;
	}

	private void OnEnable()
	{
		SceneManager.activeSceneChanged += OnSceneChanged;
	}

	private void OnDisable()
	{
		SceneManager.activeSceneChanged -= OnSceneChanged;
	}
	private void Update()
	{
		if (Active)
		{

			if (_inputUpdate != null)
				_inputUpdate();
			DetectSwipe();


			TouchPressedTime = TouchPressed ? TouchPressedTime + Time.unscaledDeltaTime : 0f;
		}
		else
			_inGameTouchFingerId = EmptyTouchFingerId;
	}

	private void DetectSwipe()
	{
		if (SwipeEvent == null)
			return;

		if (TouchPressed)
		{
			if (_swipped)
				return;

			if (_swipeDetectionTimer <= 0f)
			{

				_swipeDetectionTimer = SwipeDetectInterval;

				if (_lastSwipePosition.x > 0f && _lastSwipePosition.y > 0f)
				{
					if (DistanceRatioSwipe(_lastSwipePosition, _lastTouchPosition) >= SwipeDistanceRatioThreshold && TouchPressedTime <= SwipeMaxDuration)
					{
						_swipped = true;
						var delta = _lastTouchPosition - _lastSwipePosition;

						if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
							SwipeEvent(delta.x > 0f ? SwipeDirection.Right : SwipeDirection.Left);
						else
							SwipeEvent(delta.y > 0f ? SwipeDirection.Up : SwipeDirection.Down);
					}
				}
				
				_lastSwipePosition = _lastTouchPosition;
				

				
			}
			else
				_swipeDetectionTimer -= Time.unscaledDeltaTime;
		}
		else
		{
			_lastSwipePosition = Vector2.zero;
			_swipeDetectionTimer = 0f;
		}
	}

	private float DistanceRatio(Vector2 start, Vector2 end)
	{
		float width = (float)Screen.width;
		float height = (float)Screen.height;

		start.Set(start.x / width, start.y / height);
		end.Set(end.x / width, end.y / height);

		return Vector2.Distance(start, end);
	}
	private float DistanceRatioSwipe(Vector2 start, Vector2 end)
	{
		float width = (float)Screen.width;
		float height = (float)Screen.height;

		start.Set(start.x / width, start.y / height);
		end.Set(end.x / width, end.y / height);

		return Vector2.Distance(start, end);
	}

	private void UpdateMouse()
	{
		if (Input.GetMouseButtonDown(0))
		{
			if (IsGameMouseInput() && !TouchPressed)
			{
				_inGameTouchFingerId = MouseTouchFingerId;
				_lastTouchPosition = Input.mousePosition;
				_startTouchPosition = StartPointClampAlg != null ? StartPointClampAlg(_lastTouchPosition) : _lastTouchPosition;

				if (TouchStartEvent != null)
					TouchStartEvent(_startTouchPosition);
			}
		}

		if (TouchPressed)
		{
			var currentPosition = Input.mousePosition;
			if (TouchPressedTime >= TapLongDuration && TouchPressedTime <= TapLongDuration * 1.3f)
				DetectLongTap();
			if (TouchPressedTime >= TapSuperLongDuration && TouchPressedTime < TapSuperLongDuration * 1.3f)
				DetectSuperLongTap();
			//DetectSwipe();
			if (Input.GetMouseButtonUp(0))
			{
				_inGameTouchFingerId = EmptyTouchFingerId;
				_lastTouchPosition = currentPosition;

				DetectTap();
				_swipped = false;
				TouchEndEvent?.Invoke(_lastTouchPosition);
			}
			else
			{
				//var sqrMagnitude = new Vector2(currentPosition.x - _lastTouchPosition.x, currentPosition.y - _lastTouchPosition.y).sqrMagnitude;
				var sqrMagnitude = new Vector2(_startTouchPosition.x - _lastTouchPosition.x, _startTouchPosition.y - _lastTouchPosition.y).sqrMagnitude;
				if (/*DistanceRatioSwipe(_startTouchPosition, currentPosition) >= TapDistanceRatioThreshold*/ sqrMagnitude > MoveDistanceThresholdSqr)
				{
					_lastTouchPosition = currentPosition;

					TouchMoveEvent?.Invoke(_lastTouchPosition);
				}
				else
				{
					_lastTouchPosition = currentPosition;
					TouchMoveZeroEvent?.Invoke(_lastTouchPosition);
				}
			}
		}
	}

	private void UpdateTouches()
	{

		for (int i = 0; i < Input.touchCount; i++)
		{
			var touch = Input.GetTouch(i);
			var touchPhase = touch.phase;
			var touchFingerId = touch.fingerId;

			if (IsGameTouchInput(touchFingerId) && !TouchPressed)
			{
				_inGameTouchFingerId = MouseTouchFingerId;
				_lastTouchPosition = touch.position;

				_startTouchPosition = StartPointClampAlg != null ? StartPointClampAlg(_lastTouchPosition) : _lastTouchPosition;
					
				if (TouchStartEvent != null)
					TouchStartEvent(_startTouchPosition);
			}

			if (TouchPressed)
			{
				var currentPosition = touch.position;
				if (TouchPressedTime >= TapLongDuration && TouchPressedTime <= TapLongDuration * 1.3f)
					DetectLongTap();
				if (TouchPressedTime >= TapSuperLongDuration && TouchPressedTime < TapSuperLongDuration * 1.3f)
					DetectSuperLongTap();
				if (touchPhase == TouchPhase.Ended)
				{
					_inGameTouchFingerId = EmptyTouchFingerId;
					_lastTouchPosition = currentPosition;

					DetectTap();
					_swipped = false;
					TouchEndEvent?.Invoke(_lastTouchPosition);
				}
				else if (touchPhase == TouchPhase.Moved)
				{
					//var sqrMagnitude = new Vector2(currentPosition.x - _lastTouchPosition.x, currentPosition.y - _lastTouchPosition.y).sqrMagnitude;
					var sqrMagnitude = new Vector2(_startTouchPosition.x - _lastTouchPosition.x, _startTouchPosition.y - _lastTouchPosition.y).sqrMagnitude;
					if (/*DistanceRatioSwipe(_startTouchPosition, currentPosition) >= TapDistanceRatioThreshold*/ sqrMagnitude > MoveDistanceThresholdSqr)
					{
						_lastTouchPosition = currentPosition;

						TouchMoveEvent?.Invoke(_lastTouchPosition);
					}
					else
					{
						_lastTouchPosition = currentPosition;
						TouchMoveZeroEvent?.Invoke(_lastTouchPosition);
					}
				}
			}
		}
		//for (int i = 0; i < Input.touchCount; i++)
		//{
		//    var touch = Input.GetTouch(i);
		//    var touchPhase = touch.phase;
		//    var touchFingerId = touch.fingerId;

		//    if (_inGameTouchFingerId == EmptyTouchFingerId)
		//    {
		//        //if (TouchPressedTime >= TapLongDuration)
		//        //    DetectLongTap();
		//        if (touchPhase == TouchPhase.Began && IsGameTouchInput(touchFingerId))
		//        {
		//            _inGameTouchFingerId = touchFingerId;
		//            _lastTouchPosition = touch.position;
		//            _startTouchPosition = _lastTouchPosition;

		//            if (TouchStartEvent != null)
		//                TouchStartEvent(_lastTouchPosition);

		//            break;
		//        }
		//    }
		//    else if (_inGameTouchFingerId == touchFingerId)
		//    {

		//        if (touchPhase == TouchPhase.Canceled || touchPhase == TouchPhase.Ended)
		//        {
		//            _inGameTouchFingerId = EmptyTouchFingerId;
		//            _lastTouchPosition = touch.position;

		//            DetectTap();

		//            if (TouchEndEvent != null)
		//                TouchEndEvent(_lastTouchPosition);
		//        }
		//        else if (TouchPressedTime >= TapLongDuration)
		//        {
		//            _inGameTouchFingerId = EmptyTouchFingerId;
		//            _lastTouchPosition = touch.position;

		//            if (TouchEndEvent != null)
		//                TouchEndEvent(_lastTouchPosition);
		//        }
		//        else if (touchPhase == TouchPhase.Moved)
		//        {
		//            var currentPosition = touch.position;
		//            var sqrMagnitude = new Vector2(currentPosition.x - _lastTouchPosition.x, currentPosition.y - _lastTouchPosition.y).sqrMagnitude;

		//            if (sqrMagnitude > MoveDistanceThresholdSqr)
		//            {
		//                _lastTouchPosition = currentPosition;

		//                if (TouchMoveEvent != null)
		//                    TouchMoveEvent(_lastTouchPosition);
		//            }
		//        }

		//    }
		//}
	}

	private void DetectTap()
	{
		if (TapEvent != null && DistanceRatioSwipe(_startTouchPosition, _lastTouchPosition) </* MoveDistanceThresholdSqr*/ TapDistanceRatioThreshold)
			TapEvent(_lastTouchPosition, TouchPressedTime);
		_isLongTapped = false;
		_isSuperLongTapped = false;
	}

	private void DetectSuperLongTap()
	{
		if (SuperLongTapEvent != null && !_isSuperLongTapped && DistanceRatioSwipe(_startTouchPosition, _lastTouchPosition) < /*MoveDistanceThresholdSqr*/TapDistanceRatioThreshold)
		{
			SuperLongTapEvent(_lastTouchPosition, TouchPressedTime);
			_isSuperLongTapped = true;
		}
	}

	private void DetectLongTap()
	{
		if (LongTapEvent != null && !_isLongTapped && DistanceRatioSwipe(_startTouchPosition, _lastTouchPosition) < /*MoveDistanceThresholdSqr*/TapDistanceRatioThreshold)
		{
			LongTapEvent(_lastTouchPosition, TouchPressedTime);
			_isLongTapped = true;
		}
	}

	private static bool IsGameMouseInput()
	{
		//if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
		//	return false;

		return true;
	}

	private static bool IsGameTouchInput(int fingerId)
	{
		if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId))
			return false;

		return true;
	}

	/// <summary>
	/// 1, 1 - means all screen
	/// </summary>
	/// <param name="downToUpPart"> param from 0 to 1</param>
	/// <param name="leftToRightPart">param from 0 to 1</param>
	public bool CheckScreenPartZoneClicked(Vector3 lastTouchPosition, float downToUpPart = 1, float leftToRightPart = 1)
	{
		float h = Screen.height * downToUpPart;
		float w = Screen.width * leftToRightPart;

		return lastTouchPosition.x <= w && lastTouchPosition.y <= h;
	}
}
