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
    Text _killCountText;
    [SerializeField]
    Text _sneakKillCountText;
    [SerializeField]
    Text _totalDamageTakeText;

    [Header("Others")]
    [SerializeField]
    TeamWalkie firstKillDropWalkie;
    [SerializeField]
    GameObject remainEnemyCounter;

    ProjectRicochetMirror _projectRicochetMirror;
    struct Scenario
    {
        public bool didFirstContactGuide;
        public bool didFirstKill;
        public bool didFirstKillShieldMan;
        public int shieldManKillCount;
        public int killCount;
        public int sneakKillCount;
        public float totalDamageTake;
    }
    Scenario _scenario;

    protected override void Start()
    {
        base.Start();
        _projectRicochetMirror = Player.AbilityContainer.GetComponentInChildren<ProjectRicochetMirror>();
        GameManager.Instance.DialogUI.SetDialog(initialDialog);
        Player.onDamage.AddListener(damageSouce =>
        {
            if (damageSouce.damage > 0)
            {
                _scenario.totalDamageTake += damageSouce.damage;
            }
        });
        foreach (Unit unit in GameManager.Instance.CurrentStage.NpcUnits)
        {
            if (unit.TryGetComponent(out CommonGuard guard))
            {
                guard.onDead.AddListener(() =>
                {
                    if (!GameManager.Instance.PlayingCutscene)
                        ++_scenario.killCount;
                    if (guard.NeverFoundEnemy)
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
                        TeamWalkie walkie = Instantiate(firstKillDropWalkie, transform);
                        walkie.eventSignal = _activeRemainEnemyCounter;
                        walkie.transform.position = guard.transform.position + Vector3.up * 1;
                    }
                    if (guard.name.Contains("Shield")) //TODO: unsafe shield man identifier
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
    }
    private void Update()
    {
        if (GameManager.Instance.PlayingCutscene)
        {
            return;
        }
        if (!_scenario.didFirstContactGuide)
        {
            foreach (var unit in GameManager.Instance.CurrentStage.NpcUnits)
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
            remainEnemyCounter.GetComponentInChildren<Text>().text = "Enemy: " + GameManager.Instance.CurrentStage.AliveNpcUnits.Count;
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
        GameManager.Instance.CurrentStage.NpcUnits.ForEach(unit => unit.CutscenePause = true);
        _resultScreen.SetActive(true);
        float enemyCount = GameManager.Instance.CurrentStage.NpcUnits.Count;
        achievements[0].SetAchivement("achiv1", _scenario.shieldManKillCount >= 2);
        achievements[1].SetAchivement("achiv2", _scenario.killCount == enemyCount);
        achievements[2].SetAchivement("achiv3", _scenario.sneakKillCount / enemyCount >= 0.5F);
        _killCountText.text = _scenario.killCount + " (" + string.Format("{0:F1}", _scenario.killCount / enemyCount * 100) + "%)";
        _sneakKillCountText.text = _scenario.sneakKillCount + " (" + string.Format("{0:F1}", _scenario.sneakKillCount / enemyCount * 100) + "%)";
        _totalDamageTakeText.text = _scenario.totalDamageTake.ToString();
    }

    public override string Serialize()
    {
        return JsonUtility.ToJson(_scenario);
    }

    public override void Deserialize(string data)
    {
        _scenario = JsonUtility.FromJson<Scenario>(data);
    }
}
