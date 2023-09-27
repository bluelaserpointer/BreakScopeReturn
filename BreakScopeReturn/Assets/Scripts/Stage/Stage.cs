using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Stage : MonoBehaviour
{
    [SerializeField]
    Transform _initialSpawnAnchor;

    public Player Player { get; private set; }
    public Transform InitialSpawnAnchor => _initialSpawnAnchor;

    [SerializeField]
    public readonly List<Unit> units = new List<Unit>();
    [SerializeField]
    public readonly List<Unit> aliveUnits = new List<Unit>();

    public void Init(Player player)
    {
        Player = player;
        Player.transform.SetPositionAndRotation(InitialSpawnAnchor.position, InitialSpawnAnchor.rotation);
    }
    protected virtual void Start()
    {
        units.Clear();
        aliveUnits.Clear();
        foreach (var unitObj in GameObject.FindGameObjectsWithTag("Unit"))
        {
            if (unitObj.GetComponentInParent<Stage>() == this && unitObj.TryGetComponent(out Unit unit))
            {
                units.Add(unit);
                aliveUnits.Add(unit);
                unit.onDead.AddListener(() =>
                {
                    aliveUnits.Remove(unit);
                });
            }
        }
    }
    public virtual void GameClear()
    {
        units.ForEach(unit => unit.enabled = false);
    }
}
