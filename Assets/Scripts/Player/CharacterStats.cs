using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


namespace DangerousDungeon.Character
{
    public enum Stat
    {
        Level = 0,
        Vitality = 1,       // HitPoints
        Attunement = 2,     // Ability
        Endurance = 3,      // Movement Speed
        Strength = 4,       // Damage / Projectile Speed?
        Dexterity = 5,      // Fire Rate
        Resistance = 6,     // Defense
        Intelligence = 7,   // Range
        Faith = 8,          // ?
        Humanity = 9        // ?
    }


    [Serializable]
    public class CharacterStats
    {
        public Action<Stat> ValueChanged;

        
        // Accessor, Mutator Functions

        public readonly List<Stat> ActiveFields = new()
        { 
            Stat.Level, Stat.Endurance, Stat.Strength, Stat.Dexterity, Stat.Intelligence 
        };

        public int Get(Stat stat)
        {
            switch (stat)
            {
                case Stat.Level: return level;
                case Stat.Endurance: return endurance;
                case Stat.Strength: return strength;
                case Stat.Dexterity: return dexterity;
                case Stat.Intelligence: return intelligence;
                default: 
                    Debug.Log($"Stat {stat} currently unused");
                    return 0;
            }
        }

        public void Set(Stat stat, int value)
        {
            SetWithoutNotify(stat, value);
            ValueChanged?.Invoke(stat);
        }

        public void SetWithoutNotify(Stat stat, int value)
        {
            switch (stat)
            {
                case Stat.Level: level = value; break;
                case Stat.Endurance: endurance = value; break;
                case Stat.Strength: strength = value; break;
                case Stat.Dexterity: dexterity = value; break;
                case Stat.Intelligence: intelligence = value; break;
                default: Debug.Log($"Stat {stat} currently unused"); break;
            }
        }


        // Stat Fields

        [SerializeField, Tooltip("Total Level"), FormerlySerializedAs("Level")]
        protected int level = 0;
        public int Level
        {
            get => level;
            set
            {
                level = value;
                ValueChanged?.Invoke(Stat.Level);
            }
        }

        [SerializeField, Tooltip("Movement Speed"), FormerlySerializedAs("Endurance")]
        [Range(0, 200)]
        protected int endurance = 50;

        public int Endurance
        {
            get => endurance;
            set
            {
                endurance = value;
                ValueChanged?.Invoke(Stat.Endurance);
            }
        }

        [SerializeField, Tooltip("Projectile Speed"), FormerlySerializedAs("Strength")]
        [Range(0, 200)]
        protected int strength = 50;

        public int Strength
        {
            get => strength;
            set
            {
                strength = value;
                ValueChanged?.Invoke(Stat.Strength);
            }
        }

        [SerializeField, Tooltip("Projectile Fire Rate"), FormerlySerializedAs("Dexterity")]
        [Range(0, 200)]
        protected int dexterity = 60;

        public int Dexterity
        {
            get => dexterity;
            set
            {
                dexterity = value;
                ValueChanged?.Invoke(Stat.Dexterity);
            }
        }

        [SerializeField, Tooltip("Projectile Range"), FormerlySerializedAs("Intelligence")]
        [Range(0, 200)]
        protected int intelligence;

        public int Intelligence
        {
            get => intelligence;
            set
            {
                intelligence = value;
                ValueChanged?.Invoke(Stat.Intelligence);
            }
        }
    }
}
