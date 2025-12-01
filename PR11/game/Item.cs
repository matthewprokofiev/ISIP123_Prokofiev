using PR11.game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR11.game
{
    
        public abstract class Item
        {
            public string Name { get; protected set; }
            public int Value { get; protected set; }

            protected Item(string name, int value)
            {
                Name = name;
                Value = value;
            }

            public abstract void ApplyEffect(Player player);
        }

        public class Weapon : Item
        {
            public int Attack { get; private set; }

            public Weapon(string name, int value, int attack) : base(name, value)
            {
                Attack = attack;
            }

            public override void ApplyEffect(Player player)
            {
                player.EquipWeapon(this);
            }

            public override string ToString()
            {
                return $"{Name} (Атака: {Attack}, Ценность: {Value})";
            }
        }

        public class Armor : Item
        {
            public int Defense { get; private set; }

            public Armor(string name, int value, int defense) : base(name, value)
            {
                Defense = defense;
            }

            public override void ApplyEffect(Player player)
            {
                player.EquipArmor(this);
            }

            public override string ToString()
            {
                return $"{Name} (Защита: {Defense}, Ценность: {Value})";
            }
        }

        public class HealthPotion : Item
        {
            public HealthPotion(string name, int value) : base(name, value) { }

            public override void ApplyEffect(Player player)
            {
                player.Heal(player.MaxHP);
                Console.WriteLine("Вы восстановили HP!");
            }

            public override string ToString()
            {
                return $"{Name} (восстановление здоровья, Ценность: {Value})";
            }
        }
    
}
