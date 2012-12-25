// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CyclingProtectionManager.cs" >
//   Copyright (C) 2012 by Alexander Davyduk. All rights reserved.
// </copyright>
// <summary>
//   Defines the SessionManager type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.SharedSource.RedirectManager
{
  using System.Globalization;
  using System.Web;

  /// <summary>
  /// Session manager class
  /// </summary>
  public class CyclingProtectionManager
  {
    /// <summary>
    /// The cookies key for current cycle
    /// </summary>
    private const string CurrentCycleKey = Items.ItemIDs.RedirectsFolderItem + "-CurrentCycle";

    /// <summary>
    /// The cookies value key for current cycle
    /// </summary>
    private const string ValueKey = "Cycle";

    /// <summary>
    /// Checks the cycle.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <param name="request">The request.</param>
    /// <returns>
    ///   <c>true</c> if current cycle allowed; otherwise, <c>false</c>.
    /// </returns>
    public static bool CheckCycle([NotNull]HttpResponse response, [NotNull]HttpRequest request)
    {
      if (!Configuration.EnableCyclingProtection)
      {
        return true;
      }

      var currentCycle = GetCurrentCycle(request);
      if (currentCycle == Configuration.RecursiveAttempts)
      {
        ClearCurrentCycle(response, request);
        return false;
      }

      SetCurrentCycle(response, ++currentCycle);
      return true;
    }

    /// <summary>
    /// Increments the current cycle.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <param name="request">The request.</param>
    public static void ClearCurrentCycle([NotNull]HttpResponse response, [NotNull]HttpRequest request)
    {
      if (!Configuration.EnableCyclingProtection)
      {
        return;
      }

      if (GetCurrentCycle(request) != 0)
      {
        SetCurrentCycle(response, 0);
      }
    }

    /// <summary>
    /// Gets the current cycle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>
    /// current cycle
    /// </returns>
    private static int GetCurrentCycle([NotNull]HttpRequest request)
    {
      var redirectCookie = request.Cookies[CurrentCycleKey];
      var cycle = 0;
      if (redirectCookie != null)
      {
        int.TryParse(redirectCookie[ValueKey], out cycle);
      }

      return cycle;
    }

    /// <summary>
    /// Sets the current cycle.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <param name="value">The value.</param>
    private static void SetCurrentCycle([NotNull]HttpResponse response, int value)
    {
      var redirectCookie = response.Cookies[CurrentCycleKey];
      if (redirectCookie != null)
      {
        redirectCookie[ValueKey] = value.ToString(CultureInfo.InvariantCulture);
      }
      else
      {
        var newRedirectCookie = new HttpCookie(CurrentCycleKey);
        newRedirectCookie[ValueKey] = value.ToString(CultureInfo.InvariantCulture);
        response.Cookies.Add(newRedirectCookie);
      }
    }
  }
}