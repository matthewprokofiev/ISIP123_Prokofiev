using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PR11.game;

namespace PR11.game
{
        public class Player
        {
            public int HP { get; private set; }
            public int MaxHP { get; private set; }
            public Weapon CurrentWeapon { get; private set; }
            public Armor CurrentArmor { get; private set; }
            public bool Frozen { get; set; }
            public int TotalAttack => (CurrentWeapon?.Attack ?? 0);
            public int TotalDefense => (CurrentArmor?.Defense ?? 0);

            public Player(int maxHP)
            {
                MaxHP = maxHP;
                HP = maxHP;

                // Стартовое снаряжение
                CurrentWeapon = new Weapon("меч", 5, 13);
                CurrentArmor = new Armor("армор", 5, 12);
            }

            public void TakeDamage(int damage)
            {
                HP -= damage;
                if (HP < 0) HP = 0;
            }

            public void Heal(int amount)
            {
                HP += amount;
                if (HP > MaxHP) HP = MaxHP;
            }

            public void EquipWeapon(Weapon weapon)
            {
                CurrentWeapon = weapon;
            }

            public void EquipArmor(Armor armor)
            {
                CurrentArmor = armor;
            }

            public int CalculateDamage()
            {
                return TotalAttack;
            }

            public bool TryDefend()
            {
                if (RandomGenerator.NextDouble() < 0.4)
                {
                    Console.WriteLine("уклонились от атаки");
                    return true;
                }
                return false;
            }

            public int CalculateBlockedDamage(int incomingDamage)
            {
                double blockPercentage = 0.7 + (RandomGenerator.NextDouble() * 0.3);
                int blockedDamage = (int)(TotalDefense * blockPercentage);
                return Math.Max(0, incomingDamage - blockedDamage);
            }

            public string GetStatus()
            {
                return $"Игрок - HP: {HP}/{MaxHP}, Атака: {TotalAttack}, Защита: {TotalDefense}";
            }

            public string GetEquipment()
            {
                return $"Оружие: {CurrentWeapon?.ToString() ?? "Нет"}\nДоспехи: {CurrentArmor?.ToString() ?? "Нет "}";
            }
        }
    }

