// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SectionToSection.cs">
//   Copyright (C) 2012 by Alexander Davyduk. All rights reserved.
// </copyright>
// <summary>
//   Defines the SectionToSection type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.SharedSource.RedirectManager.Templates
{
  using Sitecore.Data.Fields;
  using Sitecore.Data.Items;

  /// <summary>
  ///  SectionToSection class
  /// </summary>
  public sealed class SectionToSection : CustomItem
  {
    // Fields

    /// <summary>
    /// SectionToSectionTemplate template ID
    /// </summary>
    public const string TemplateId = "{77A922AE-C49E-468D-BA7E-91746A34E7E0}";

    /// <summary>
    ///  Base Section url field
    /// </summary>
    private TextField baseSection;

    /// <summary>
    ///  Target Section path field
    /// </summary>
    private InternalLinkField targetSection;

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
    /// Initializes a new instance of the <see cref="SectionToSection"/> class.
    /// </summary>
    /// <param name="innerItem">Inner item.</param>
    public SectionToSection(Item innerItem)
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
    /// Gets the target section path.
    /// </summary>
    public InternalLinkField TargetSection
    {
      get
      {
        return this.targetSection ?? (this.targetSection = this.InnerItem.Fields["Target Section"]);
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