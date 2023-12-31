using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public abstract class Stage : MonoBehaviour
{
    [Header("Start")]
    [SerializeField]
    Transform _initialSpawnAnchor;

    [Header("EventSignal")]
    [SerializeField]
    Transform _eventSignalParent;

    [Header("Destory")]
    [SerializeField]
    UnityEvent _onDestroy;
    public Player Player => _player;
    public Transform InitialSpawnAnchor => _initialSpawnAnchor;
    public List<Unit> NpcUnits => _npcUnits;
    public List<Unit> AliveNpcUnits => _aliveNpcUnits;
    public UnityEvent OnDestroy => _onDestroy;

    readonly List<Unit> _npcUnits = new();
    readonly List<Unit> _aliveNpcUnits = new();

    [SerializeField, HideInInspector]
    Player _player;

    public void Init(Player playerPrefab)
    {
        _player = Instantiate(playerPrefab, transform);
        _player.InitialInit();
        Player.transform.SetPositionAndRotation(InitialSpawnAnchor.position, InitialSpawnAnchor.rotation);
        _npcUnits.Clear();
        _aliveNpcUnits.Clear();
        foreach (var unitObj in GameObject.FindGameObjectsWithTag(Unit.TAG_NPC_UNIT))
        {
            if (unitObj.GetComponentInParent<Stage>() == this && unitObj.TryGetComponent(out Unit unit))
            {
                _npcUnits.Add(unit);
                if (!unit.IsDead)
                {
                    _aliveNpcUnits.Add(unit);
                    unit.onDead.AddListener(() =>
                    {
                        _aliveNpcUnits.Remove(unit);
                    });
                }
            }
        }
        _npcUnits.ForEach(unit => unit.InitialInit());
    }
    protected virtual void Start()
    {
    }
    public virtual void GameClear()
    {
        _npcUnits.ForEach(unit => unit.enabled = false);
    }
    public void Destroy()
    {
        OnDestroy.Invoke();
        Destroy(gameObject);
    }
    public EventSignal FindEventSignal(string eventName)
    {
        return _eventSignalParent.Find(eventName).GetComponent<EventSignal>();
    }
    public void InvokeEventSignal(string eventName)
    {
        FindEventSignal(eventName).onEvent.Invoke();
    }
    public abstract string Serialize();
    public abstract void Deserialize(string data);
}
