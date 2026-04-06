using UnityEngine;
[CreateAssetMenu(fileName = "New Environment", menuName = "ScriptableObjects/Environment")]
public class EnvironmentSO : ScriptableObject
{
    public float gravityScale;
    public float airResistance;
}