using Assets.Scripts.Base;
using UnityEngine;

namespace Game.Units.Views
{
    public abstract class UnitView : MonoBehaviour
    {
        [SerializeField] private GameObject _view;
        [SerializeField] private DirectionIndicator _directionIndicator;
        [SerializeField] private Animator _animator;
       
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        
        public void LookAt(Vector2 direction)
        {
            if (_directionIndicator == null) 
                return;
            
            _directionIndicator.SetDirection(direction);
        }
        
        public void SetVisability(bool isVisible)
        {
            if (_view == null)
                return;
            
            _view.SetActive(isVisible);
        }

        public void SetSpeed(float speed)
        {
            if (_animator == null)
                return;

            _animator.SetFloat(MoveSpeed, speed);
        }
    }
}
