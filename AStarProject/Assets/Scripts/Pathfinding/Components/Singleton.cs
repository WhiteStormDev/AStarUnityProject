using System;

namespace Pathfinding.Components
{
    public class Singleton<T> where T : Singleton<T>, new()
    {
        private static T _instance;

        public static T Instance
        {
            get { return _instance ?? (_instance = new T()); }
        }

        public static bool Try(Action<T> callback)
        {
            if (_instance == null)
                return false;
            callback(_instance);
            return true;
        }

        public static void ReleaseInstance()
        {
            if (_instance != null)
                _instance.DoRelease();
            _instance = null;
        }

        protected virtual void DoRelease()
        {
        }
    }
}