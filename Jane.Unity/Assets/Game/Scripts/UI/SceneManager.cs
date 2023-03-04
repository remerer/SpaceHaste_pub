using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneManager : MonoBehaviour
{
    [SerializeField] private string titleSceneName = "";
    [SerializeField] private string gameSceneName = "";

    private MatchMakingManager? matchMakingManager = null;
    private GameHubManager? gameHubManager = null;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(this);
    }
    
    public async UniTask LoadGameSceneAsync()
    {
        await UnitySceneManager.LoadSceneAsync(gameSceneName).ToUniTask()
            .ContinueWith(async () =>
            {
                gameHubManager = FindObjectOfType<GameHubManager>();
                Debug.Log(gameHubManager);
                await gameHubManager.InitializeAsync();
            });
        
    }
}
