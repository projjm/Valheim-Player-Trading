using UnityEngine;

namespace PlayerTrading.Patterns
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T? _instance; 
        public static T Instance {
            get {
                if (_instance == null)
                {
                    //Debug.LogError(typeof(T).ToString() + " Instance has not been initialised");
                } 
                return _instance!; 
            }
        }

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(this);
                return;
            }
            
            _instance = (T)this;
            Init();
        }

        protected virtual void Init() { }

    }
}
