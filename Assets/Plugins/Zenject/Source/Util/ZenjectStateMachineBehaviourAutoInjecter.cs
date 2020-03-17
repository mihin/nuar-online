using ModestTree;
using UnityEditor.Animations;
using UnityEngine;

namespace Zenject
{
    public class ZenjectStateMachineBehaviourAutoInjecter : MonoBehaviour
    {
        DiContainer _container;
        AnimatorController _animator;

        [Inject]
        public void Construct(DiContainer container)
        {
            _container = container;
            _animator = GetComponent<AnimatorController>();
            Assert.IsNotNull(_animator);
        }

        // The unity docs (https://unity3d.com/learn/tutorials/modules/beginner/5-pre-order-beta/state-machine-behaviours)
        // mention that StateMachineBehaviour's should only be retrieved in the Start method
        // which is why we do it here
        public void Start()
        {
        }
    }
}
