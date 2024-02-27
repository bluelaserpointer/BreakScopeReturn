using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Stage1Scenario : Stage
{
    [Header("Event Signal Ref")]
    [SerializeField]
    EventSignal _activeRemainEnemyCounter;

    [Header("Cutscene")]
    [SerializeField]
    Camera _cutsceneCamera;
    public bool playingCutscene;

    [Header("Dialog")]
    [SerializeField]
    DialogNodeSet initialDialog;
    [SerializeField]
    DialogNodeSet firstContactDialog;
    [SerializeField]
    DialogNodeSet firstSneakKillDialog;
    [SerializeField]
    DialogNodeSet remainEnemyCounterDialog;
    [SerializeField]
    DialogNodeSet firstKillShieldManDialog;

    [Header("Achivement & Result Screen")]
    [SerializeField]
    GameObject _resultScreen;
    [SerializeField]
    Achievement[] achievements;
    [SerializeField]
    Text _clearTime;
    [SerializeField]
    Text _sneakKillRateText;
    [SerializeField]
    Text _totalDamageTakeText;

    [Header("Others")]
    [SerializeField]
    EventDropItem _firstKillDropWalkiePrefab;
    [SerializeField]
    GameObject remainEnemyCounter;

    ProjectRicochetMirror _projectRicochetMirror;
    struct ScenarioData
    {
        public float stageStartedTime;
        public bool didFirstContactGuide;
        public bool didFirstKill;
        public bool didFirstKillShieldMan;
        public int shieldManKillCount;
        public int killCount;
        public int sneakKillCount;
        public float totalDamageTaken;
    }
    ScenarioData _scenario;

    protected override void Start()
    {
        base.Start();
        _projectRicochetMirror = Player.AbilityContainer.GetComponentInChildren<ProjectRicochetMirror>();
        GameManager.Instance.DialogUI.SetDialog(initialDialog);
        Player.onDamage.AddListener(damageSouce =>
        {
            if (damageSouce.damage > 0)
            {
                _scenario.totalDamageTaken += damageSouce.damage;
            }
        });
        foreach (Unit unit in GameManager.Instance.Stage.NpcUnits)
        {
            if (unit.TryGetComponent(out NpcUnit npcUnit))
            {
                npcUnit.onDead.AddListener(() =>
                {
                    if (!GameManager.Instance.PlayingCutscene)
                        ++_scenario.killCount;
                    if (npcUnit.NeverFoundEnemy)
                    {
                        ++_scenario.sneakKillCount;
                        if (_scenario.sneakKillCount == 1)
                        {
                            GameManager.Instance.DialogUI.SetDialog(firstSneakKillDialog);
                        }
                    }
                    if (!_scenario.didFirstKill)
                    {
                        _scenario.didFirstKill = true;
                        EventDropItem walkie = Instantiate(_firstKillDropWalkiePrefab, transform);
                        walkie.eventSignal = _activeRemainEnemyCounter;
                        walkie.transform.position = npcUnit.transform.position + Vector3.up * 1;
                    }
                    if (npcUnit.name.Contains("Shield")) //TODO: unsafe shield man identifier
                    {
                        ++_scenario.shieldManKillCount;
                        if (!_scenario.didFirstKillShieldMan)
                        {
                            _scenario.didFirstKillShieldMan = true;
                            GameManager.Instance.DialogUI.SetDialog(firstKillShieldManDialog);
                        }
                    }
                });
            }
        }
        GameManager.DoAfterInit(() => _scenario.stageStartedTime = Time.timeSinceLevelLoad);
    }
    private void Update()
    {
        if (GameManager.Instance.PlayingCutscene)
        {
            return;
        }
        if (!_scenario.didFirstContactGuide)
        {
            foreach (var unit in GameManager.Instance.Stage.NpcUnits)
            {
                if (unit.GetType() == typeof(CommonGuard) && !((CommonGuard)unit).FoundEnemy
                    &&  !((CommonGuard)unit).IsDead
                    && Vector3.Distance(unit.transform.position, Player.transform.position) < 10)
                {
                    _scenario.didFirstContactGuide = true;
                    GameManager.Instance.DialogUI.SetDialog(firstContactDialog);
                    break;
                }
            }
        }
        if (remainEnemyCounter.activeSelf)
        {
            remainEnemyCounter.GetComponentInChildren<Text>().text = "Enemy: " + GameManager.Instance.Stage.AliveNpcUnits.Count;
        }
    }
    public void ActivateRemainEnemyCounter()
    {
        remainEnemyCounter.SetActive(true);
        GameManager.Instance.DialogUI.SetDialog(remainEnemyCounterDialog);
    }
    public void EnablePlayerRicochetMirrorForCutscene(bool cond)
    {
        _projectRicochetMirror.SetMirror(cond);
        _projectRicochetMirror.Mirror.SetCameraLookingAtThisMirror(cond ? _cutsceneCamera : Player.Camera);
    }
    public override void GameClear()
    {
        base.GameClear();
        Player.CutscenePause = true;
        GameManager.Instance.Stage.NpcUnits.ForEach(unit => unit.CutscenePause = true);
        _resultScreen.SetActive(true);
        int enemyCount = GameManager.Instance.Stage.NpcUnits.Count;
        achievements[0].SetAchivement("achiv1", _scenario.shieldManKillCount >= 2);
        achievements[1].SetAchivement("achiv2", _scenario.killCount == enemyCount);
        achievements[2].SetAchivement("achiv3", (float)_scenario.sneakKillCount / enemyCount >= 0.5F);
        _clearTime.text = string.Format("{0:F1}", Time.timeSinceLevelLoad - _scenario.stageStartedTime) + "s";
        _sneakKillRateText.text = string.Format("{0:P2}", (float)_scenario.sneakKillCount / _scenario.killCount);
        _totalDamageTakeText.text = _scenario.totalDamageTaken.ToString();
    }

    public override string Serialize()
    {
        return JsonUtility.ToJson(_scenario);
    }

    public override void Deserialize(string data)
    {
        _scenario = JsonUtility.FromJson<ScenarioData>(data);
    }
}
