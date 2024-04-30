using UnityEngine;
using UnityEngine.Events;

namespace Utils
{
    public class UnityEventFunctionWrapper : MonoBehaviour
    {
        public UnityEvent awakeEvent;
        public UnityEvent startEvent;
        public UnityEvent onEnableEvent;
        public UnityEvent onDisableEvent;
        public UnityEvent onDestroyEvent;
    
        private void Awake()
        {
            awakeEvent.Invoke();
        }
        
        private void Start()
        {
            startEvent.Invoke();
        }
    
        private void OnEnable()
        {
            onEnableEvent.Invoke();
        }
        
        private void OnDisable()
        {
            onDisableEvent.Invoke();
        }
        
        private void OnDestroy()
        {
            onDestroyEvent.Invoke();
        }
    }
}
