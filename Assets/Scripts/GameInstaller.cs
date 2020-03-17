using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private Transform grid;
    [Inject] private CardsScriptableObject cardsScriptableObject;

    public override void InstallBindings()
    {
        Container.BindFactory<CardPrefab, CardPrefab.Factory>().FromComponentInNewPrefab(cardsScriptableObject.prefab).UnderTransform(grid);
    }
}