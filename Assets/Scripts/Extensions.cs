using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;
using Random = System.Random;

public static class Extensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static string ToJson<T>(this List<T> l)
    {
        Type baseType = typeof(T);

        List<Type> needTypes = new List<Type>();

        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.ValidateBaseType(baseType))
            {
                needTypes.Add(type);
            }
        }

        string result = string.Empty;

        foreach (T element in l)
        {
            foreach (Type type in needTypes)
            {
                if (type == element.GetType())
                {
                    object changeType = Convert.ChangeType(element, type);

                    string json = JsonUtility.ToJson(changeType);

                    result += json + '|' + type.FullName + '\n';
                }
            }
        }

        return result;
    }

    public static List<T> FromJson<T>(this string json)
    {
        List<T> result = new List<T>();

        string[] elements = json.Split('\n');

        for (int i = 0; i < elements.Length; ++i)
        {
            int split = elements[i].LastIndexOf('|');

            string type = elements[i].Substring(split + 1);

            result.Add((T)JsonUtility.FromJson(elements[i].Substring(0, split), Type.GetType(type.Trim())));
        }

        return result;
    }

    public static bool ValidateBaseType(this Type me, Type baseType)
    {
        if (me != null)
        {
            if (me == baseType)
            {
                return true;
            }
            else if (me.BaseType != baseType)
            {
                return me.BaseType.ValidateBaseType(baseType);
            }
            else
                return true;
        }
        else
            return false;
    }
}

public static class ThreadSafeRandom
{
    [ThreadStatic] private static Random Local;

    public static Random ThisThreadsRandom
    {
        get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
    }
}