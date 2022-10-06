using System;
using System.Text;
using System.Collections.Specialized;

public static class Util
{
  static System.Random random = new System.Random();

  public static float Aprox(float value, float variance = 0.2f)
  {
    return UnityEngine.Random.Range(value - (value * variance), value + (value * variance));
  }

  public static T Decide<T>(T[] items)
  {
    int index = Util.random.Next(items.Length);
    return items[index];
  }

  public static bool Decide()
  {
    return Decide(50);
  }

  public static bool Decide(int odds)
  {
    return Util.random.Next(100) > (100 - odds);
  }
}

public static class UtilityExtensionMethods
{
  public static string ToQueryString(this NameValueCollection nvc)
  {
    if (nvc == null) return string.Empty;

    StringBuilder sb = new StringBuilder();

    foreach (string key in nvc.Keys)
    {
      if (string.IsNullOrWhiteSpace(key)) continue;

      string[] values = nvc.GetValues(key);
      if (values == null) continue;

      foreach (string value in values)
      {
        sb.Append(sb.Length == 0 ? "?" : "&");
        sb.AppendFormat("{0}={1}", Uri.EscapeDataString(key), Uri.EscapeDataString(value));
      }
    }

    return sb.ToString();
  }
}
