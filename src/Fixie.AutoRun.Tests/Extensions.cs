using Microsoft.Reactive.Testing;
using System;

namespace Fixie.AutoRun.Tests
{
   public static class Extensions
   {
      public static void AdvanceBy(this TestScheduler scheduler, TimeSpan timespan)
      {
         scheduler.AdvanceBy(timespan.Ticks);
      }
   }
}