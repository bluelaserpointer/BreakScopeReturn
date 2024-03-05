using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacableGadget : HandEquipment
{
    [Header("Debug")]
    [SerializeField]
    bool _realtimeIK;

    [Header("Spawning")]
    [SerializeField]
    GameObject _spawningPrefab;

    [Header("Performance")]
    [SerializeField]
    float _fireCD;
    [SerializeField]
    float _reloadTime;

    [Header("Positioning")]
    [SerializeField]
    Transform _leftHandGoal;
    [SerializeField]
    Transform _rightHandGoal;

    [Header("Sound")]
    [SerializeField]
    AudioSource _placeSESource;
    [SerializeField]
    AudioSource _reloadSESource;
    
    public GameObject SpawningPrefab => _spawningPrefab;
    //TODO: add placeTime;
    public float ReloadTime => _reloadTime;
    public Transform LeftHandGoal => _leftHandGoal;
    public Transform RightHandGoal => _rightHandGoal;
    public AudioSource PlaceSESource => _placeSESource;
    public AudioSource ReloadSESource => _reloadSESource;
    public TransformRelator CentreRelRightHand { get; private set; }
    protected override void Awake()
    {
        base.Awake();
        UpdateAnchorRelation();
    }
    private void Update()
    {
        if (_realtimeIK)
            UpdateAnchorRelation();
    }
    void UpdateAnchorRelation()
    {
        CentreRelRightHand = new TransformRelator(transform, _rightHandGoal);
    }
}
