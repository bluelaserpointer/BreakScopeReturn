using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Stage1Scenario : Stage
{
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
    GameObject firstKillDrop;
    [SerializeField]
    GameObject remainEnemyCounter;


    [HideInInspector]
    [SerializeField]
    bool _didFirstContactGuide;
    [HideInInspector]
    [SerializeField]
    bool _didFirstKill;
    [HideInInspector]
    [SerializeField]
    bool _didFirstKillShieldMan;
    [HideInInspector]
    [SerializeField]
    int _shieldManKillCount;
    [HideInInspector]
    [SerializeField]
    int _killCount;
    [HideInInspector]
    [SerializeField]
    int _sneakKillCount;
    [HideInInspector]
    [SerializeField]
    float _totalDamageTake;

    protected override void Start()
    {
        base.Start();
        GameManager.Instance.DialogUI.SetDialog(initialDialog);
        GameManager.Instance.Player.onDamage.AddListener(damageSouce =>
        {
            if (damageSouce.damage > 0)
            {
                _totalDamageTake += damageSouce.damage;
            }
        });
        foreach (Unit unit in GameManager.Instance.CurrentStage.units)
        {
            if (unit.TryGetComponent(out CommonGuard guard))
            {
                guard.onDead.AddListener(() =>
                {
                    ++_killCount;
                    if (guard.NeverFoundEnemy)
                    {
                        ++_sneakKillCount;
                        if (_sneakKillCount == 1)
                        {
                            GameManager.Instance.DialogUI.SetDialog(firstSneakKillDialog);
                        }
                    }
                    if (!_didFirstKill)
                    {
                        _didFirstKill = true;
                        Instantiate(firstKillDrop, transform).transform.position = guard.transform.position;
                    }
                    if (guard.name.Contains("Shield")) //TOOD: unsafe shield man identifier
                    {
                        ++_shieldManKillCount;
                        if (!_didFirstKillShieldMan)
                        {
                            _didFirstKillShieldMan = true;
                            GameManager.Instance.DialogUI.SetDialog(firstKillShieldManDialog);
                        }
                    }
                });
            }
        }
    }
    private void Update()
    {
        if (!_didFirstContactGuide)
        {
            foreach (var unit in GameManager.Instance.CurrentStage.units)
            {
                if (unit.GetType() == typeof(CommonGuard) && !((CommonGuard)unit).FoundEnemy
                    && Vector3.Distance(unit.transform.position, GameManager.Instance.Player.transform.position) < 10)
                {
                    _didFirstContactGuide = true;
                    GameManager.Instance.DialogUI.SetDialog(firstContactDialog);
                    break;
                }
            }
        }
        if (remainEnemyCounter.gameObject.activeSelf)
        {
            remainEnemyCounter.GetComponentInChildren<Text>().text = "Enemy: " + GameManager.Instance.CurrentStage.aliveUnits.Count;
        }
    }
    public void ActivateRaminEnemyCounter()
    {
        remainEnemyCounter.SetActive(true);
        GameManager.Instance.DialogUI.SetDialog(remainEnemyCounterDialog);
    }
    public override void GameClear()
    {
        base.GameClear();
        GameManager.Instance.Player.enabled = false;
        GameManager.Instance.Player.MouseLook.enabled = false;
        GameManager.Instance.Player.Movement.enabled = false;
        GameManager.Instance.Player.Movement.Rigidbody.isKinematic = true;
        GameManager.Instance.Player.GunInventory.enabled = false;
        GameManager.Instance.Player.GunInventory.HandsAnimator.enabled = false;
        GameManager.Instance.Player.GunInventory.currentGun.enabled = false;
        GameManager.Instance.Player.GetComponent<ProjectRicochetMirror>().enabled = false;
        GameManager.Instance.CurrentStage.units.ForEach(unit => unit.enabled = false);
        Cursor.lockState = CursorLockMode.None;
        _resultScreen.gameObject.SetActive(true);
        float enemyCount = GameManager.Instance.CurrentStage.units.Count;
        achievements[0].SetAchivement("シールドマンを二体倒す", _shieldManKillCount >= 2);
        achievements[1].SetAchivement("全ての敵を倒す", _killCount == enemyCount);
        achievements[2].SetAchivement("半数以上の敵を気付かれずに倒す", _sneakKillCount / enemyCount >= 0.5F);
        _killCountText.text = _killCount + " (" + string.Format("{0:F1}", _killCount / enemyCount * 100) + "%)";
        _sneakKillCountText.text = _sneakKillCount + " (" + string.Format("{0:F1}", _sneakKillCount / enemyCount * 100) + "%)";
        _totalDamageTakeText.text = _totalDamageTake.ToString();
    }
}
