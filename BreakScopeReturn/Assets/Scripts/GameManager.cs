using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    [Header("Test")]
    [SerializeField] Stage _initialStage;

    [Header("Reference")]
    [SerializeField] Player playerPrefab;
    [SerializeField] DialogUI dialogUI;
    [SerializeField] GameObject playerDeathBlackout;
    [SerializeField] CheckPointNotifiactionUI checkPointNotification;
    [SerializeField] DirectionIndicator directionIndicator;
    [SerializeField] GameObject interactIconViewer;

    public static GameManager Instance { get; private set; }
    public Player Player => CurrentStage.Player;
    public DialogUI DialogUI => dialogUI;
    public GameObject InteractIconViewer => interactIconViewer;
    public Stage InitialStage => _initialStage;
    public Stage CurrentStage { get; private set; }
    public Stage SavedStage { get; private set; }
    public Vector3 PlayerPositionSave { get; private set; }
    public Quaternion PlayerRotationSave { get; private set; }
    public CheckPointNotifiactionUI CheckPointNotification => checkPointNotification;
    public DirectionIndicator DirectionIndicator => directionIndicator;
    public int currentRespawnAnchorPriority;
    private void Awake()
    {
        Instance = this;
        InitialStage.gameObject.SetActive(false);
        InitStage();
    }
    public void InitStage()
    {
        if (SavedStage)
            Destroy(SavedStage.gameObject);
        SavedStage = InitialStage;
        currentRespawnAnchorPriority = -1;
        PlayerPositionSave = InitialStage.InitialSpawnAnchor.position;
        PlayerRotationSave = InitialStage.InitialSpawnAnchor.rotation;
        RespawnPlayer();
    }
    public void SaveStage()
    {
        if (SavedStage)
            Destroy(SavedStage.gameObject);
        SavedStage = Instantiate(CurrentStage);
        SavedStage.name.Substring(SavedStage.name.Length - 7);
        SavedStage.name += "(Save)";
        SavedStage.gameObject.SetActive(false);
        PlayerPositionSave = Player.transform.position;
        PlayerRotationSave = Player.transform.rotation;
    }
    public void LoadStage()
    {
        if (CurrentStage)
            Destroy(CurrentStage.gameObject);
        CurrentStage = Instantiate(SavedStage);
        if (CurrentStage.Player == null)
        {
            CurrentStage.Init(Instantiate(playerPrefab, CurrentStage.transform));
        }
        CurrentStage.name.Substring(CurrentStage.name.Length - 6);
        CurrentStage.gameObject.SetActive(true);
    }
    public void RespawnPlayer()
    {
        LoadStage();
        //Player.transform.SetPositionAndRotation(PlayerPositionSave, PlayerRotationSave);
        /*
        if (currentRespawnAnchorPriority != -1)
        {
            foreach (GameObject checkPointObj in GameObject.FindGameObjectsWithTag("RespawnAnchor"))
            {
                //TODO: potentially dangerous respawn point identify
                if(checkPointObj.TryGetComponent(out CheckPoint checkPoint) && checkPoint.Priority == currentRespawnAnchorPriority)
                {
                    CurrentRespawnAnchor = checkPoint.RespawnAnchor;
                    currentRespawnAnchorPriority = checkPoint.Priority;
                }
            }
        }*/
        Player.Init();
    }
    public bool TrySetSpawnPoint(CheckPoint checkPoint)
    {
        if (checkPoint.Priority > currentRespawnAnchorPriority)
        {
            currentRespawnAnchorPriority = checkPoint.Priority;
            SaveStage();
            return true;
        }
        return false;
    }
    public void SetBlackout(bool cond)
    {
        playerDeathBlackout.SetActive(cond);
    }
}
