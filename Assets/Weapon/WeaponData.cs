using UnityEngine;
using TPSRoguelite.InGame.Enum;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
public class WeaponData : ScriptableObject
{
    [field: SerializeField] public string WeaponName { get; private set; }

    [field: SerializeField] public FireType WeaponFireType { get; private set; }

    [field: SerializeField] public int AttackPower { get; private set; }

    [field: SerializeField] public float FireInteval { get; private set; }
    [field: SerializeField] public float FireRate { get; private set; }
    [field: SerializeField] public int MaxAmmo { get; private set; }
    [field: SerializeField] public float ReloadTime { get; private set; }
}
