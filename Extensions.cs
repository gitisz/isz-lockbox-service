using isz.lockbox.service.Models;
using isz.lockbox.service.Providers;

namespace isz.lockbox.service;

public static class ExtensionMethods
{
  public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
  {
    if (action == null)
    {
      throw new ArgumentNullException(nameof(action));
    }
    foreach (T item in sequence)
    {
      action(item);
    }
  }
}