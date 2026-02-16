using UnityEngine;
using Project.Core;

namespace Project.Systems
{
    public sealed class DebugStateControls : MonoBehaviour
    {
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private int debugGoldAmount = 10;

        private void Update()
        {
            if (gameStateManager == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                if (gameStateManager.CurrentState == GameState.Combat)
                {
                    gameStateManager.ResolveCombatVictory();
                }
                else if (gameStateManager.CurrentState == GameState.Boss)
                {
                    gameStateManager.ResolveBossVictory();
                }
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                gameStateManager.AddGold(debugGoldAmount);
            }
        }
    }
}
