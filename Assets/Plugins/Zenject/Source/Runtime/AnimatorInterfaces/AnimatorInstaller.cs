using UnityEditor.Animations;
using UnityEngine;

namespace Zenject
{
    public class AnimatorInstaller : Installer<AnimatorController, AnimatorInstaller>
    {
        readonly AnimatorController _animator;

        public AnimatorInstaller(AnimatorController animator)
        {
            _animator = animator;
        }

        public override void InstallBindings()
        {
            Container.Bind<AnimatorIkHandlerManager>().FromNewComponentOn(GameObject.Find(""));
            Container.Bind<AnimatorIkHandlerManager>().FromNewComponentOn(GameObject.Find(""));
        }
    }
}

