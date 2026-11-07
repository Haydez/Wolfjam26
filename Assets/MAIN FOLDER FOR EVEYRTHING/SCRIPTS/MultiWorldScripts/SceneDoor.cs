using UnityEngine;

public class SceneDoor : MonoBehaviour
{
    public enum TransitionStyle { BatSpin, Pixelate, SplitReveal }

#if UNITY_EDITOR
    [SerializeField] UnityEditor.SceneAsset sceneAsset;
    void OnValidate() { if (sceneAsset != null) nextScene = sceneAsset.name; }
#endif
    [SerializeField, HideInInspector] string nextScene;

    [SerializeField] TransitionStyle transition = TransitionStyle.BatSpin;
    [SerializeField] string playerTag = "Player";

    public static bool AnyTransitionRunning =>
        BatSpinTransition.IsTransitioning || PixelateTransition.IsTransitioning || SplitRevealTransition.IsTransitioning;
    
    void OnTriggerEnter(Collider other)            => TryEnter(other.transform);
    void OnCollisionEnter(Collision collision)      => TryEnter(collision.transform);
    
    void TryEnter(Transform touched)
    {
        if (IsPlayer(touched)) Interact();
    }
    
    bool IsPlayer(Transform t) 
    {
        for (; t != null; t = t.parent)
            if (t.CompareTag(playerTag)) return true;
        return false;
    }
    
    public void Interact()
    {
        if (AnyTransitionRunning) return;

        /*f (string.IsNullOrEmpty(nextScene))
        {
            return;
        }*/

        switch (transition)
        {
            case TransitionStyle.Pixelate:    PixelateTransition.Play(nextScene);    break;
            case TransitionStyle.SplitReveal: SplitRevealTransition.Play(nextScene); break;
            default:                          BatSpinTransition.Play(nextScene);     break;
        }
    }
}