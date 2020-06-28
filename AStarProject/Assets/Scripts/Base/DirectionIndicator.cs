using UnityEngine;

namespace Assets.Scripts.Base
{
    public class DirectionIndicator : MonoBehaviour {

        public void SetDirection(Vector2 direction)
        {
            float z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, z - 90f);
        }
    }
}
