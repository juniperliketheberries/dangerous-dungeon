using UnityEngine;


namespace DangerousDungeon.Character
{
    // use this to define a decorator for stat boosts to pass data to player
    public class CharacterStatsHandler : MonoBehaviour
    {
        [SerializeField] private Player m_Player;
        [SerializeField] private PlayerInputActionMapping m_InputActionMapping; // this is a god class rn

        // Get base stats
        // Apply modifiers
        // Pass values to other classes, ie PlayerInputActionMapping


        public void RandomStatIncrease()
        {
            switch (Random.Range(0, 4))
            {
                case 0:
                    m_Player.Stats.Endurance++;
                    break;
                case 1:
                    m_Player.Stats.Dexterity++;
                    break;
                case 2:
                    m_Player.Stats.Strength++;
                    break;
                case 3:
                    m_Player.Stats.Intelligence++;
                    break;
            }
        }



        public void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Random Stat Increase")) { RandomStatIncrease(); }
            GUILayout.EndHorizontal();
        }

        public static float Convert(int stat)
        {
            return (float)stat / 10;
        }
    }
}
