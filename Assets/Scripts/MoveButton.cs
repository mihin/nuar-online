using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class MoveButton : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Left,
        Down,
        Right
    }

    [SerializeField] private Button button;
    [SerializeField] private RectTransform arrowRect;
    private int index = -1;
    private Direction direction = Direction.Up;

    public Button Button => button;
    public int Index => index;

    public event Action<int, Direction> OnClick;

    private void ClickHandler()
    {
        OnClick?.Invoke(index, direction);
    }

    public class Factory : PlaceholderFactory<MoveButton>
    {
        public MoveButton Create(Transform parent, Direction direction, int index)
        {
            MoveButton mb = base.Create();
            mb.transform.parent = parent;
            mb.transform.localScale = Vector3.one;
            mb.arrowRect.Rotate(Vector3.forward, 90f * (int)direction);
            mb.index = index;
            mb.direction = direction;
            mb.button.onClick.AddListener(mb.ClickHandler);
            return mb;
        }
    }
}
