// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SectionToItem.cs">
//   Copyright (C) 2012 by Alexander Davyduk. All rights reserved.
// </copyright>
// <summary>
//   Defines the SectionToItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.SharedSource.RedirectManager.Templates
{
  using Sitecore.Data.Fields;
  using Sitecore.Data.Items;

  /// <summary>
  /// SectionToItem class
  /// </summary>
  public sealed class SectionToItem : CustomItem
  {
    // Fields

    /// <summary>
    ///  SectionToItemTemplate template ID
    /// </summary>
    public const string TemplateId = "{A7C5E6AB-48FF-4846-9A2C-255F8387F18E}";

    /// <summary>
    /// Base Section url field
    /// </summary>
    private TextField baseSection;

    /// <summary>
    /// Target Item field
    /// </summary>
    private LinkField targetItem;

    /// <summary>
    /// The redirect code
    /// </summary>
    private int redirectCode;

    /// <summary>
    /// The date of last use
    /// </summary>
    private DateField lastUse;

    // Methods

    /// <summary>
    /// Initializes a new instance of the <see cref="SectionToItem"/> class.
    /// </summary>
    /// <param name="innerItem">Inner item.</param>
    public SectionToItem(Item innerItem)
      : base(innerItem)
    {
    }

    // Properties

    /// <summary>
    /// Gets the base section url.
    /// </summary>
    public TextField BaseSection
    {
      get
      {
        return this.baseSection ?? (this.baseSection = this.InnerItem.Fields["Base Section"]);
      }
    }

    /// <summary>
    /// Gets the target item.
    /// </summary>
    public LinkField TargetItem
    {
      get
      {
        return this.targetItem ?? (this.targetItem = this.InnerItem.Fields["Target Item"]);
      }
    }

    /// <summary>
    /// Gets the redirect code.
    /// </summary>
    /// <value>
    /// The redirect code.
    /// </value>
    public int RedirectCode
    {
      get
      {
        return this.redirectCode == 0 ? (this.redirectCode = MainUtil.GetInt(this.InnerItem.Fields["Redirect Code"], Configuration.RedirectStatusCode)) : this.redirectCode;
      }
    }

    /// <summary>
    /// Gets the last use.
    /// </summary>
    /// <value>
    /// The last use.
    /// </value>
    public DateField LastUse
    {
      get
      {
        return this.lastUse ?? (this.lastUse = this.InnerItem.Fields["Last Use"]);
      }
    }
  }
}