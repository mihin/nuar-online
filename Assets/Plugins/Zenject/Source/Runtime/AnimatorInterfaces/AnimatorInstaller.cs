using UnityEngine;

namespace Zenject
{
    public class AnimatorInstaller : Installer<object, AnimatorInstaller>
    {
        readonly object _animator;

        public AnimatorInstaller(object animator)
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

