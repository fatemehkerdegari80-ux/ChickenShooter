using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private LevelDataSO firstLevel;
    private bool loading;

    public void NewGame()
    {
        if (loading)
            return;

        if (GameManager.Instance == null || firstLevel == null)
        {
            Debug.LogError("Connect GameManager and First Level.", this);
            return;
        }

        if (string.IsNullOrWhiteSpace(firstLevel.sceneName) ||
            !Application.CanStreamedLevelBeLoaded(firstLevel.sceneName))
        {
            Debug.LogError("Check First Level Scene Name and the build scene list.", this);
            return;
        }

        loading = true;
        GameManager.Instance.ResetRun();
        Time.timeScale = 1f;
        SceneManager.LoadScene(firstLevel.sceneName);
    }
}