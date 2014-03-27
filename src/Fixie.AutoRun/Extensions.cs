using System;
using System.Collections.Generic;

namespace Fixie.AutoRun
{
   public static class Extensions
   {
      public static void Do<T>(this IEnumerable<T> source, Action<IEnumerable<T>> action)
      {
         action(source);
      }

      public static void Each<T>(this IEnumerable<T> source, Action<T> action)
      {
         foreach (var item in source) action(item);
      }

      public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item)
      {
         foreach (var x in source) yield return x;
         yield return item;
      }

      public static ISet<T> ToSet<T>(this IEnumerable<T> source, IEqualityComparer<T> equalityComparer = null)
      {
         return new HashSet<T>(source, equalityComparer ?? EqualityComparer<T>.Default);
      } 
   }
}