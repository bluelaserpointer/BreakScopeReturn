using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IzumiTools;

public enum MenuStyle{
	horizontal,vertical
}
public class GunInventory : MonoBehaviour
{
    [Header("Hands")]
    [SerializeField]
    List<PlayerHands> _hands;
    [SerializeField]
    Cooldown _switchWeaponCD = new Cooldown(1.2F);

    [Header("Equipment")]
    [SerializeField]
    HandEquipment[] _initialEquipments;
	[SerializeField]
    int _equipmentInventorySize = 2;

    [Header("Sounds")]
    [Tooltip("Sound of weapon changing.")]
    public AudioSource weaponChanging;

    public readonly List<HandEquipment> equipments = new();
    public int EquipmentInventorySize => _equipmentInventorySize;
    public int HoldingEquipmentIndex { get; private set; }
	public HandEquipment HoldingEquipment => (HoldingEquipmentIndex < equipments.Count) ? equipments[HoldingEquipmentIndex] : null;
    public PlayerHands Hands { get; private set; } //TODO: make sure non null (make empty hands)
	//readonly Dictionary<HandEquipment, PlayerHands> _equipmentToHands = new();

    private Player Player => GameManager.Instance.Player;

	public void InitialInit(){
        foreach (var equipmentPrefab in _initialEquipments)
        {
            AddEquipment(Instantiate(equipmentPrefab));
        }
        LoadInit();
	}
    public void LoadInit()
    {
        if (Hands != null)
        {
            Hands.WithdrawItemAndDisable();
            Hands = null;
        }
        _switchWeaponCD.IsReady = true;
    }
    public void InitHoldingWeaponIndex(int index)
    {
        HoldingEquipmentIndex = index;
    }
    public void AddEquipment(HandEquipment equipment)
    {
        //TODO: size check
        if (equipments.Contains(equipment))
        {
            print("<!>" + nameof(AddEquipment) + " received " + equipments + " which is already exist in inventory.");
        }
        equipment.transform.SetParent(transform, false);
        equipment.saveProperty.prefabRoot.excludeFromSave = true;
        equipment.gameObject.SetActive(false);
        equipments.Add(equipment);
        if (typeof(Gun).IsAssignableFrom(equipment.GetType()))
            Player.EquipmentSidePreview.SetGun(equipments.Count - 1, equipment as Gun);
        else
            print("<!>Unsupported " + nameof(HandEquipment) + " type: " + equipment.GetType().Name);
    }
    private void Update()
    {
        ListenSwitchWeaponInput();
	}
    /// <summary>
    /// If used scroll mousewheel or arrows up and down the player will change weapon.
    /// </summary>
    private void ListenSwitchWeaponInput() {
        _switchWeaponCD.AddDeltaTime();
        int newEquipmentIndex = HoldingEquipmentIndex;
        if (_switchWeaponCD.IsReady)
        {
            //TODO: bind action button for weapon scrolling
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetAxis("Mouse ScrollWheel") > 0)
                newEquipmentIndex = (newEquipmentIndex + 1) % EquipmentInventorySize;
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                if (--newEquipmentIndex < 0)
                {
                    newEquipmentIndex += EquipmentInventorySize;
                }
            }
            else
            {
                for (int i = 0; i < EquipmentInventorySize; i++)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    {
                        newEquipmentIndex = i;
                        break;
                    }
                }
            }
        }
        SwitchWeaponUpdate(newEquipmentIndex);
    }
    public void SwitchWeaponUpdate(int newEquipmentIndex)
    {
        StartCoroutine(nameof(HandsChange), newEquipmentIndex);
    }

    /// <summary>
    /// check if we carry a gun and destroy it, and load prefab from the Resources Folder
    /// </summary>
    /// <param name="newEquipmentIndex"></param>
    /// <returns></returns>
    IEnumerator HandsChange(int newEquipmentIndex)
    {
        newEquipmentIndex = Mathf.Clamp(newEquipmentIndex, 0, equipments.Count - 1);
        if (HoldingEquipmentIndex == newEquipmentIndex && (Hands != null || HoldingEquipment == null))
			yield break;
        _switchWeaponCD.Reset();
        if (weaponChanging != null)
			weaponChanging.Play();
		if(Hands != null)
        {
            Hands.Animator.SetTrigger("takeDown");
            yield return new WaitForSeconds(0.8f);//0.8 time to change waepon, but since there is no change weapon animation there is no need to wait fo weapon taken down
            Hands.WithdrawItemAndDisable();
            Hands = null;
        }
        HoldingEquipmentIndex = newEquipmentIndex;
        if (HoldingEquipment != null)
        {
            Hands = _hands.Find(hands => hands.HandsType.Equals(HoldingEquipment.HandsType));
            Hands.transform.SetPositionAndRotation(transform.position, transform.rotation);
            Hands.Init(HoldingEquipment);
            Hands.gameObject.SetActive(true);
        }
        else
        {
            //add empty hands (fists?)
        }
    }
}
