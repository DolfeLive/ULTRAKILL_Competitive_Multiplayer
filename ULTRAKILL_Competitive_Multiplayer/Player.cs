using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ULTRAKILL_Competitive_Multiplayer;

public interface IPlayer { }

public class Player : MonoBehaviour, IPlayer
{
    public static GameObject DodgeEffect;
    public static GameObject SlideEffect;
    public static GameObject JumpSound;
    public static GameObject DashJumpSound;
    public static GameObject Shockwave;
    public static GameObject Coin;
    public static List<Texture> WingTextures = new();
    public static SkinnedMeshRenderer Smr;

    public static List<Transform> aimAtTarget = new();

    public byte Health = 100;
    public bool Dodging = false;

    public Animator anim;

    public CapsuleCollider collider;

    public Vector3 position
    {
        get => transform.position;
        set => transform.position = value;
    }
    public Vector3 rotation
    {
        get => transform.rotation.eulerAngles;
        set => transform.rotation = Quaternion.Euler(value);
    }

    public Vector3 velocity
    {
        get => rb.velocity;
        set => rb.velocity = value;
    }

    public Rigidbody rb;

    public void Start()
    {
        V2 v = GetComponent<V2>();
        Player.DodgeEffect = v.dodgeEffect;
        Player.SlideEffect = v.slideEffect;
        Player.JumpSound = v.jumpSound;
        Player.DashJumpSound = v.dashJumpSound;
        Player.Shockwave = v.shockwave;
        Player.Coin = v.coin;
        Player.WingTextures = v.wingTextures.ToList();
        Player.aimAtTarget = v.aimAtTarget.ToList();
        Player.Smr = v.smr;

        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        
        anim.SetBool("InAir", true);
        anim.SetBool("Sliding", false);
        if (anim.layerCount > 1)
        {
            anim.SetLayerWeight(0, 1f);
            anim.SetLayerWeight(1, 0f);
            anim.SetLayerWeight(2, 0f);
        }

        rb.isKinematic = true;
        v.enabled = false;
    }

    public void Update()
    {

    }

    public void FixedUpdate()
    {

    }

    public void Move(Vector3 pos, Vector3 Vel, byte properties)
    {
        position = pos;
        velocity = Vel;

        // { jumping, dashing, SSJing, Sliding, Slamming }
        
    }

    public void ReliableStateInfo(byte properties)
    {
        // { jumping, dashing, SSJing, Sliding, Slamming }

    }

    public void Aim(Vector3 Rot)
    {
        if (aimAtTarget != null && aimAtTarget.Count > 0 && aimAtTarget[0] != null)
        {
            aimAtTarget[0].localRotation = Quaternion.Euler(Rot.x, Rot.y, 0f); // Head
            aimAtTarget[1].localRotation = Quaternion.Euler(Rot.x, Rot.y, 0f); // Arm
        }
    }



}
public class PlayerIdentifierIdentifier : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        return;

        this.gameObject.SendMessageUpwards("DamageTaken", SendMessageOptions.DontRequireReceiver);
    }
}