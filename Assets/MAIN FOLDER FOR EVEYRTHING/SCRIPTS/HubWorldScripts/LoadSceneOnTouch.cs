using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class LoadSceneOnTouch : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "EndingCutScene";

    private bool loading;

    private void OnTriggerEnter(Collider other)
    {
        if (loading || !other.CompareTag("Player")) return;

        loading = true;
        SceneManager.LoadScene(sceneToLoad);
    }
}