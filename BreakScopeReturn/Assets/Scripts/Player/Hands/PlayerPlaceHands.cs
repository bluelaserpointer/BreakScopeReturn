using IzumiTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlaceHands : PlayerHands
{
    [SerializeField]
    float _defaultFov = 60;
    public override HandsType HandsType => HandsType.Place;
    public PlacableGadget Gadget { get; private set; }
    public Cooldown ReloadCD { get; private set; }
    public bool IsReloading { get; private set; }

    private Player Player => GameManager.Instance.Player;
    private Camera MainCamera => Player.Camera;
    private PlayerMovement PlayerMovement => Player.Movement;
    private MouseLook PlayerLook => Player.MouseLook;

    Transform _rightHandBone;
    bool _rendered;
    bool _rightHandIKRotationOffsetAssigned;
    Quaternion _rightHandIKRotationOffset;

    public override void Init(HandEquipment equipment)
    {
        Gadget = (PlacableGadget)equipment;
        PrepareTakeDown = false;
        IsReloading = false;
        ReloadCD = new Cooldown(Gadget.ReloadTime);
        InitAnimatorParameters();
        _rightHandBone = Animator.GetBoneTransform(HumanBodyBones.RightHand);
        Gadget.gameObject.SetActive(true);
        Player.IKEventExposure.onAnimatorIK.AddListener(AnimatorIK);
        Player.Camera.fieldOfView = _defaultFov;
        PlayerLook.recoilRotation.x = PlayerLook.recoilRotation.y = 0;
    }
    private void OnEnable()
    {
        if (Gadget == null)
            return;
        //animator parameters will be lost after gameobject disable.
        InitAnimatorParameters();
    }
    private void InitAnimatorParameters()
    {
        //Animator.SetFloat("reloadSpeedMultiplier", Gadget.ReloadAnimationClipLength / Mathf.Max(ReloadCD.Capacity, 0.1F));
        //Animator.SetFloat("reloadAnimationType", Gadget.ReloadAnimationID);
    }
    private void AnimatorIK(int layerIndex)
    {
        if (layerIndex == Animator.GetLayerIndex("LegsLayer")) //must done before heads layer
        {
            //camera defines head rotation
            //hands ik depends on head bone ik result, thus must do look at ik in prior layer 
            Animator.SetLookAtPosition(Player.GunEyeNoZRotAnchor.position + MainCamera.transform.forward);
            return;
        }
        if (layerIndex == Animator.GetLayerIndex("HandsLayer"))
        {
            //compute skeleton rotation offset from mechanim (Gun hands ik goal uses mechanim form)
            if (!_rightHandIKRotationOffsetAssigned)
            {
                _rightHandIKRotationOffsetAssigned = true;
                _rightHandIKRotationOffset = Quaternion.Inverse(_rightHandBone.rotation) * Animator.GetIKRotation(AvatarIKGoal.RightHand);
            }
            Quaternion rightHandIKRotationGoal = _rightHandBone.rotation * _rightHandIKRotationOffset;

            //animation defines position of gadget
            Gadget.transform.SetPositionAndRotation(
                Gadget.CentreRelRightHand.GetChildPosition(_rightHandBone.position, rightHandIKRotationGoal),
                Gadget.CentreRelRightHand.GetChildRotation(rightHandIKRotationGoal)
                );
            //gun defines position of hands
            Animator.SetIKPosition(AvatarIKGoal.LeftHand, Gadget.LeftHandGoal.position);
            Animator.SetIKRotation(AvatarIKGoal.LeftHand, Gadget.LeftHandGoal.rotation);
            Animator.SetIKPosition(AvatarIKGoal.RightHand, Gadget.RightHandGoal.position);
            Animator.SetIKRotation(AvatarIKGoal.RightHand, Gadget.RightHandGoal.rotation);
            //(above ik weights are exposed in IKEventExposure for direct animation control)
            //Ensure first gun positioning is done before render
            if (!_rendered)
            {
                _rendered = true;
                Gadget.gameObject.SetActive(true);
            }
        }
    }
    private void Update()
    {
        PlaceUpdate();
        ReloadUpdate();
    }
    public override void TakeDown()
    {
        base.TakeDown();
        Animator.SetTrigger("takeDown");
        Gadget.ReloadSESource.Stop();
        IsReloading = false;
    }
    public override void Disable()
    {
        Gadget.gameObject.SetActive(false);
        GameManager.Instance.Player.IKEventExposure.onAnimatorIK.RemoveListener(AnimatorIK);
        base.Disable();
    }
    void PlaceUpdate()
    {
        if (PrepareTakeDown || !Player.AIEnable)
            return;
        if (Input.GetButtonDown("Fire1"))
        {
            Place();
        }
    }
    public void Place()
    {
        if (Gadget.magazine.Value > 0 && !IsReloading)
        {
            Instantiate(Gadget.SpawningPrefab, GameManager.Instance.Stage.transform).transform.position = Player.transform.position;
            if (Gadget.PlaceSESource != null)
                Gadget.PlaceSESource.Play();
            --Gadget.magazine.Value;
            TryReload();
        }
    }
    bool TryReload()
    {
        if (IsReloading || Gadget.spareAmmo <= 0 || Gadget.magazine.Full)
            return false;
        if (Gadget.ReloadSESource != null)
            Gadget.ReloadSESource.Play();
        IsReloading = true;
        Animator.SetTrigger("reload");
        ReloadCD.Clear();
        return true;
    }
    void ReloadUpdate()
    {
        if (PrepareTakeDown)
            return;
        if (PlayerMovement.HasInputXZ && PlayerMovement.MovingState == MovingStateEnum.Run)
        {
            IsReloading = false;
        }
        if (ReloadCD.AddDeltaTimeAndEat())
        {
            IsReloading = false;
            Gadget.spareAmmo = (int)Gadget.magazine.AddAndGetOverflow(Gadget.spareAmmo);
        }
    }
}
