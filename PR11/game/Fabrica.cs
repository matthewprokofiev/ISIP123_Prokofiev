using PR11.game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR11.game
{
        public class Fabrica
        {
            public Enemy CreateEnemy(EnemyType type)
            {
                switch (type)
                {
                    case EnemyType.Goblin:
                        return new Goblin();
                    case EnemyType.Skeleton:
                        return new Skeleton();
                    case EnemyType.Mage:
                        return new Mage();
                    case EnemyType.Slime:
                        return new Slime();
                    case EnemyType.VVG:
                        return new VVG();
                    case EnemyType.Kovalsky:
                        return new Kovalsky();
                    case EnemyType.ArchimageCPP:
                        return new ArchimageCPP();
                    case EnemyType.PestovCmm:
                        return new PestovCmm();
                    default:
                        throw new System.ArgumentException($"Неизвестный тип врага: {type}");
                }
            }

            public Enemy CreateRandomEnemy()
            {
                var enemyTypes = new[] { EnemyType.Goblin, EnemyType.Skeleton, EnemyType.Mage, EnemyType.Slime };
                var randomType = RandomGenerator.GetRandomItem(enemyTypes);
                return CreateEnemy(randomType);
            }

            public Enemy CreateRandomBoss()
            {
                var bossTypes = new[] { EnemyType.VVG, EnemyType.Kovalsky, EnemyType.ArchimageCPP, EnemyType.PestovCmm };
                var randomType = RandomGenerator.GetRandomItem(bossTypes);
                return CreateEnemy(randomType);
            }
        }
    }
