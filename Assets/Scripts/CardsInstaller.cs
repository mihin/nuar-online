using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "CardsInstaller", menuName = "Installers/CardsInstaller")]
public class CardsInstaller : ScriptableObjectInstaller<CardsInstaller>
{
    [SerializeField] private CardsScriptableObject cardsScriptableObject;

    public override void InstallBindings()
    {
        Container.BindInstance(cardsScriptableObject).AsSingle();
        Container.BindFactory<Card, Card.Factory>();
    }
}