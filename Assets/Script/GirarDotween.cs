 using UnityEngine;
using DG.Tweening;

public class GirarDotween : MonoBehaviour
{
    public float duracao = 2f;  

    void Start()
    {
        transform
            .DORotate(new Vector3(0, 0, 360), duracao, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);
    }
}