using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Fixie.AutoRun.VisualStudio;

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

      public static TimeSpan Milliseconds(this int milliseconds)
      {
         return TimeSpan.FromMilliseconds(milliseconds);
      }

      public static TimeSpan Seconds(this double seconds)
      {
         return TimeSpan.FromSeconds(seconds);
      }

      public static TimeSpan Seconds(this int seconds)
      {
         return TimeSpan.FromSeconds(seconds);
      }

      public static bool IsLocalName(this XAttribute xAttribute, string localName)
      {
         return xAttribute.Name.LocalName == localName;
      }

      public static bool IsLocalName(this XElement xElement, string localName)
      {
         return xElement.Name.LocalName == localName;
      }

      public static bool IsTestProject(this IProject project)
      {
         var filename = Path.GetFileNameWithoutExtension(project.Path);
         var patterns = new[] {"Test", "Tests"};
         return patterns.Any(x => filename.EndsWith(x));
      }
   }
}