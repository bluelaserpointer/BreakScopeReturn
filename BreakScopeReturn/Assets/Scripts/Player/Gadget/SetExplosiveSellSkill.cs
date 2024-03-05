using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetExplosiveSellSkill : MonoBehaviour
{
    [SerializeField]
    GameObject _cellPrefab;


    private Player Player => GameManager.Instance.Player;
    void Update()
    {
        if (!Player.AIEnable)
            return;
        if (Input.GetKeyDown(KeyCode.T))
        {
            Instantiate(_cellPrefab, GameManager.Instance.Stage.transform).transform.position = Player.transform.position;
        }
    }
}
