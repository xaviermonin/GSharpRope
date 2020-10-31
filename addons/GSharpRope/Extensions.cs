﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xydion.Plugins
{
    static class Extensions
    {
        public static void AddAfterEach<T>(this List<T> list, Func<T, bool> condition, T objectToAdd)
        {
            foreach (var item in list.Select((o, i) => new { Value = o, Index = i }).Where(p => condition(p.Value)).OrderByDescending(p => p.Index))
            {
                if (item.Index + 1 == list.Count) list.Add(objectToAdd);
                else list.Insert(item.Index + 1, objectToAdd);
            }
        }
    }
}
