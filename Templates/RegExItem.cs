// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RegExtItem.cs">
//   Copyright (C) 2012 by Alexander Davyduk. All rights reserved.
// </copyright>
// <summary>
//   Defines the RegExtItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.SharedSource.RedirectManager.Templates
{
  using System.Text.RegularExpressions;
  using Sitecore.Data;

  /// <summary>
  /// Defines the regex item class.
  /// </summary>
  public class RegExItem
  {
    /// <summary>
    /// Gets or sets the item ID.
    /// </summary>
    /// <value>
    /// The item ID.
    /// </value>
    public ID ItemId { get; set; }

    /// <summary>
    /// Gets or sets the base url.
    /// </summary>
    /// <value>
    /// The base url.
    /// </value>
    public Regex Expression { get; set; }

    /// <summary>
    /// Gets or sets the target url.
    /// </summary>
    /// <value>
    /// The target url.
    /// </value>
    public Regex Value { get; set; }

    /// <summary>
    /// Gets or sets the redirect code.
    /// </summary>
    /// <value>
    /// The redirect code.
    /// </value>
    public int RedirectCode { get; set; }
  }
}