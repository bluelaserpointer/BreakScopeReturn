using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stage2Scenario : Stage
{
    public GameObject remainEnemyCounter;

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

    struct ScenarioData
    {
        public float stageStartedTime;
        public bool didFirstContactGuide;
        public bool didFirstKill;
        public bool didFirstKillShieldMan;
        public bool didFirstKillTurret;
        public int shieldManKillCount;
        public int killCount;
        public int sneakKillCount;
        public float totalDamageTaken;
    }
    ScenarioData _scenario;

    protected override void Start()
    {
        base.Start();
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
        remainEnemyCounter.GetComponentInChildren<Text>().text = "Enemy: " + GameManager.Instance.Stage.AliveNpcUnits.Count;
    }
    public override void GameClear()
    {
        base.GameClear();
        _resultScreen.SetActive(true);
        int enemyCount = GameManager.Instance.Stage.NpcUnits.Count;
        achievements[0].SetAchivement("achiv1", _scenario.shieldManKillCount >= 2);
        achievements[1].SetAchivement("achiv2", _scenario.killCount == enemyCount);
        achievements[2].SetAchivement("achiv3", (float)_scenario.sneakKillCount / enemyCount >= 0.5F);
        _clearTime.text = string.Format("{0:F1}", Time.timeSinceLevelLoad - _scenario.stageStartedTime) + "s";
        _sneakKillRateText.text = _scenario.killCount == 0 ? "0" : string.Format("{0:P2}", (float)_scenario.sneakKillCount / _scenario.killCount);
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
