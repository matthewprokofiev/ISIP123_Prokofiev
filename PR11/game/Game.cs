using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PR11.game;

namespace PR11.game
{
        public class Game
        {
            private Player player;
            private int turnCount;
            private Fabrica Fabrica;

            // Списки предметов для генерации
            private List<Weapon> weapons = new List<Weapon>
        {
            new Weapon("Деревянный меч", 10, 5),
            new Weapon("Железный меч", 20, 8),
            new Weapon("зачарованный лук", 25, 10),
            new Weapon("аганим скипетр", 30, 12),
            new Weapon("алмазный меч", 50, 15)
        };

            private List<Armor> armors = new List<Armor>
        {
            new Armor("Кожаная броня", 10, 3),
            new Armor("Кольчуга", 20, 5),
            new Armor("железная броня", 30, 8),
            new Armor("алмазная броня", 25, 6),
            new Armor("незеритовая броня", 50, 12)
        };

            public Game()
            {
                player = new Player(100);
                turnCount = 0;
                Fabrica = new Fabrica();
            }

            public void Start()
            {
                Console.WriteLine("начало игры\n");

                while (player.HP > 0)
                {
                    turnCount++;
                    Console.WriteLine($"Ход {turnCount}");
                    Console.WriteLine(player.GetStatus());

                    if (player.Frozen)
                    {
                        Console.WriteLine("Вы заморожены");
                        player.Frozen = false;
                        continue;
                    }

                    // Случайное событие: 50% сундук, 50% враг
                    if (RandomGenerator.NextBool())
                    {
                        EncounterChest();
                    }
                    else
                    {
                        EncounterEnemy();
                    }

                    // Каждые 10 ходов - босс
                    if (turnCount % 10 == 0)
                    {
                        Console.WriteLine("\nвнимаени босс!");
                        EncounterBoss();
                    }

                    if (player.HP > 0)
                    {
                        Console.WriteLine("\nНажмите любую клавишу для продолжения");
                        Console.ReadKey();
                    }
                }

                Console.WriteLine("\nигра окончена");
            }

            private void EncounterChest()
            {
                Console.WriteLine("сундук");

                // Случайный предмет: 33% зелье, 33% оружие, 33% доспехи
                int itemType = RandomGenerator.Next(3);
                Item item = null;

                switch (itemType)
                {
                    case 0:
                        item = new HealthPotion("зелье хп", 15);
                        break;
                    case 1:
                        item = RandomGenerator.GetRandomItem(weapons);
                        break;
                    case 2:
                        item = RandomGenerator.GetRandomItem(armors);
                        break;
                }

                Console.WriteLine($"В сундуке: {item}");

                if (item is HealthPotion)
                {
                    item.ApplyEffect(player);
                }
                else
                {
                    Console.WriteLine("\nтекущая экипировка:");
                    Console.WriteLine(player.GetEquipment());
                    Console.WriteLine("\nвзять этот предмет? (д/н)");

                    string choice = Console.ReadLine().ToLower();
                    if (choice == "д" || choice == "y")
                    {
                        item.ApplyEffect(player);
                        Console.WriteLine("Предмет получен");
                    }
                    else
                    {
                        Console.WriteLine("Вы выбросили предмет.");
                    }
                }
            }

            private void EncounterEnemy()
            {
                Enemy enemy = Fabrica.CreateRandomEnemy();
                Console.WriteLine($"Вы встретили {enemy.Name}!");
                StartCombat(enemy);
            }

            private void EncounterBoss()
            {
                Enemy boss = Fabrica.CreateRandomBoss();
                Console.WriteLine($"Перед вами {boss.Name}!"); StartCombat(boss);
            }

            private void StartCombat(Enemy enemy)
            {
                Console.WriteLine($"\n=== Бой с {enemy.Name} ===");

                while (enemy.IsAlive && player.HP > 0)
                {
                    // Ход игрока
                    if (!player.Frozen)
                    {
                        PlayerTurn(enemy);
                    }
                    else
                    {
                        Console.WriteLine("Вы заморожены и пропускаете ход!");
                        player.Frozen = false;
                    }

                    if (!enemy.IsAlive) break;

                    // Ход врага
                    EnemyTurn(enemy);
                }

                if (!enemy.IsAlive)
                {
                    Console.WriteLine($"\nПобеда! Вы победили {enemy.Name}!");
                }
            }

            private void PlayerTurn(Enemy enemy)
            {
                Console.WriteLine("\nВаш ход:");
                Console.WriteLine("1 - Атаковать");
                Console.WriteLine("2 - Защищаться");
                Console.Write("Выберите действие: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        int damage = player.CalculateDamage();
                        enemy.TakeDamage(damage);
                        Console.WriteLine($"Вы нанесли {damage} урона {enemy.Name}!");
                        Console.WriteLine($"{enemy.GetStatus()}");
                        break;

                    case "2":
                        player.TryDefend(); // Результат будет использован при атаке врага
                        break;

                    default:
                        Console.WriteLine("Неверный выбор, вы пропускаете ход!");
                        break;
                }
            }

            private void EnemyTurn(Enemy enemy)
            {
                Console.WriteLine($"\nХод {enemy.Name}:");

                int damage = enemy.CalculateDamage(player);

                // Проверка защиты игрока
                if (!player.TryDefend())
                {
                    int finalDamage = player.CalculateBlockedDamage(damage);
                    player.TakeDamage(finalDamage);
                    Console.WriteLine($"{enemy.Name} наносит {finalDamage} урона! (Исходный урон: {damage})");
                }

                // Применение спецэффектов
                enemy.ApplyEffectDamage(player);

                Console.WriteLine($"{player.GetStatus()}");
            }
        }
    }

