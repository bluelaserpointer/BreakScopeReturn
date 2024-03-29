using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public abstract class Stage : MonoBehaviour
{
    [Header("Start")]
    [SerializeField]
    Player _player;
    [SerializeField]
    Transform _initialSpawnAnchor;
    [SerializeField]
    Transform _debugSpawnAnchor;

    [Header("EventSignal")]
    [SerializeField]
    Transform _eventSignalParent;

    [Header("Destory")]
    [SerializeField]
    UnityEvent _onDestroy;
    public Player Player => _player;
#if UNITY_EDITOR
    public Transform InitialSpawnAnchor => _debugSpawnAnchor;
#else
    public Transform InitialSpawnAnchor => _initialSpawnAnchor;
#endif
    public List<NpcUnit> NpcUnits => _npcUnits;
    public List<NpcUnit> AliveNpcUnits => _aliveNpcUnits;
    public UnityEvent OnDestroy => _onDestroy;

    readonly List<NpcUnit> _npcUnits = new();
    readonly List<NpcUnit> _aliveNpcUnits = new();

    public void Init()
    {
        _player.Init(true);
        Player.transform.SetPositionAndRotation(InitialSpawnAnchor);
        _npcUnits.Clear();
        _aliveNpcUnits.Clear();
    }
    public void OnNewNpcUnitAdded(NpcUnit newNpcUnit)
    {
        if (_npcUnits.Contains(newNpcUnit))
            return;
        _npcUnits.Add(newNpcUnit);
        if (!newNpcUnit.IsDead)
        {
            _aliveNpcUnits.Add(newNpcUnit);
            newNpcUnit.onDead.AddListener(() =>
            {
                _aliveNpcUnits.Remove(newNpcUnit);
            });
        }
    }
    protected virtual void Start()
    {
    }
    public virtual void GameClear()
    {
        Player.CutscenePause = true;
        GameManager.Instance.Stage.NpcUnits.ForEach(unit => unit.CutscenePause = true);
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
