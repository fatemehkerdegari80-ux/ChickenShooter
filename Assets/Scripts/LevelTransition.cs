using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    [SerializeField] private LevelDataSO nextLevel;

    private bool loading;

    [ContextMenu("Test Next Level")]
    public void LoadNextLevel()
    {
        if (!Application.isPlaying || loading)
            return;

        if (nextLevel == null || string.IsNullOrWhiteSpace(nextLevel.sceneName))
        {
            Debug.LogError("LevelTransition: connect Next Level and set its Scene Name.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nextLevel.sceneName))
        {
            Debug.LogError(
                $"Add scene '{nextLevel.sceneName}' to the build scene list.", this);
            return;
        }

        loading = true;

        if (GameManager.Instance != null)
        {
            GameManager state = GameManager.Instance;
            Debug.Log(
                $"Before transition: Lives={state.Lives}, Bombs={state.Bombs}, " +
                $"Power={state.PowerLevel}, Score={state.Score}");
        }

        SceneManager.LoadScene(nextLevel.sceneName);
    }
}