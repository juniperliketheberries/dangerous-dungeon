using UnityEngine;

namespace DangerousDungeon.Character
{
    public class Player : MonoBehaviour
    {
        public PlayerInputActionMapping InputActions;
        public CharacterController Controller;
        public CharacterStats Stats;

        // char controller
        // char stats
        // movement input

        private void OnValidate()
        {
            if (!InputActions) { InputActions = GetComponent<PlayerInputActionMapping>(); }
            if (!Controller) { Controller = GetComponent<CharacterController>(); }
        }


        public void Update()
        {
            ReadStats();
        }

        public void OnTriggerEnter(Collider other)
        {

            // Handle collision with Collectible items
            if (!other.TryGetComponent<ICollectible>(out var collectible)) { return; }

            collectible.Collect();
        }


        /// <summary>
        ///     Maps player stats to their appropriate impact on 
        ///     the player's movement and abilities
        /// </summary>
        public void ReadStats()
        {
            InputActions.MovementSpeed = CharacterStatsHandler.Convert(Stats.Endurance);
            InputActions.ProjectileRate = CharacterStatsHandler.Convert(Stats.Dexterity);
            InputActions.ProjectileSpeed = CharacterStatsHandler.Convert(Stats.Strength);
            InputActions.ProjectileRange = CharacterStatsHandler.Convert(Stats.Intelligence);
        }
    }
}
