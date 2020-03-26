using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [Inject] private CardsScriptableObject cardsScriptableObject;
    [SerializeField] private GameObject moveButtonPrefab;

    public override void InstallBindings()
    {
        Container.BindFactory<CardPrefab, CardPrefab.Factory>().FromComponentInNewPrefab(cardsScriptableObject.prefab);
        Container.BindFactory<MoveButton, MoveButton.Factory>().FromComponentInNewPrefab(moveButtonPrefab);
    }
}