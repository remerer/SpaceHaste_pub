using Jane.Unity;
using TMPro;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out NetworkPlayer player) 
            && player.IsFinished is false)
        {
            Debug.Log("finished");
            player.IsFinished = true;
            RankManager.instance.finishCount++;
        }

        if (other.gameObject.CompareTag("Player"))
        {
            gameManager.RaceFinish();
        }
    }
}
