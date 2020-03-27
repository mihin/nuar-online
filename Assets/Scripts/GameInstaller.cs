using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject moveButtonPrefab;

    public override void InstallBindings()
    {
        Container.BindFactory<CardPrefab, CardPrefab.Factory>().FromComponentInNewPrefab(cardPrefab);
        Container.BindFactory<MoveButton, MoveButton.Factory>().FromComponentInNewPrefab(moveButtonPrefab);
    }
}