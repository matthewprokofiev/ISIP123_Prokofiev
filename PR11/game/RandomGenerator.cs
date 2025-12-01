using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PR11.game;

public static class RandomGenerator
{
    private static Random _random = new Random();

    public static int Next()
    {
        return _random.Next();
    }

    public static int Next(int maxValue)
    {
        return _random.Next(maxValue);
    }

    public static int Next(int minValue, int maxValue)
    {
        return _random.Next(minValue, maxValue);
    }

    public static double NextDouble()
    {
        return _random.NextDouble();
    }

    public static bool NextBool()
    {
        return _random.Next(2) == 0;
    }

    public static T GetRandomItem<T>(T[] array)
    {
        return array[_random.Next(array.Length)];
    }

    public static T GetRandomItem<T>(List<T> list)
    {
        return list[_random.Next(list.Count)];
    }
}

    


