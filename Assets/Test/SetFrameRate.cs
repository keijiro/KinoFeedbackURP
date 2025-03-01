using UnityEngine;

public sealed class SetFrameRate : MonoBehaviour
{
    [SerializeField] int _targetFrameRate = 60;

    void Start()
      => Application.targetFrameRate = _targetFrameRate;
}
