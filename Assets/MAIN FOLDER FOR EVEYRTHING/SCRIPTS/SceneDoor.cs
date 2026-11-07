using UnityEngine;

public class SceneDoor : MonoBehaviour
{
    public enum TransitionStyle { BatSpin, Pixelate }


    [SerializeField] UnityEditor.SceneAsset sceneAsset;
    void OnValidate() { if (sceneAsset != null) nextScene = sceneAsset.name; }

    [SerializeField, HideInInspector] string nextScene;

    [SerializeField] TransitionStyle transition = TransitionStyle.BatSpin;
    [SerializeField] string playerTag = "Player";

    public static bool AnyTransitionRunning =>
        BatSpinTransition.IsTransitioning || PixelateTransition.IsTransitioning;
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Touched by {other.name}");
        TryEnter(other.transform);
    }

    void OnCollisionEnter(Collision collision)
    {
        TryEnter(collision.transform);
    }

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
            case TransitionStyle.Pixelate: PixelateTransition.Play(nextScene); break;
            default:                        BatSpinTransition.Play(nextScene);  break;
        }
    }
}