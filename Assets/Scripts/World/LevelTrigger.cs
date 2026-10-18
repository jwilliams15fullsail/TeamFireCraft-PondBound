using UnityEditor.Build.Content;
using UnityEngine;

public class LevelTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Game manager controlls what happens when you win
            GameManager.instance.WinGame();
        }
    }
}
