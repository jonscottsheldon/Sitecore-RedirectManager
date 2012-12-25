// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UrlValidator.cs">
//   Copyright (C) 2012 by Alexander Davyduk. All rights reserved.
// </copyright>
// <summary>
//   Defines the UrlValidator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.SharedSource.RedirectManager
{
  using System;
  using System.Runtime.Serialization;
  using System.Text.RegularExpressions;

  using Sitecore.Data.Validators;

  /// <summary>
  /// UrlValidator class
  /// </summary>
  [Serializable]
  public class UrlValidator : StandardValidator
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="UrlValidator"/> class.
    /// </summary>
    public UrlValidator()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlValidator"/> class.
    /// </summary>
    /// <param name="info">The Serialization info.</param>
    /// <param name="context">The context.</param>
    public UrlValidator(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>
    /// The validator name.
    /// </value>
    public override string Name
    {
      get
      {
        return "UrlValidator";
      }
    }

    /// <summary>
    /// Gets the max validator result.
    /// </summary>
    /// <returns>
    /// The max validator result.
    /// </returns>
    protected override ValidatorResult GetMaxValidatorResult()
    {
      return GetFailedResult(ValidatorResult.Error);
    }

    /// <summary>
    /// When overridden in a derived class, this method contains the code
    /// to determine whether the value in the input control is valid.
    /// </summary>
    /// <returns>
    /// The result of the evaluation.
    /// </returns>
    protected override ValidatorResult Evaluate()
    {
      var controlValidationValue = ControlValidationValue;

      if (string.IsNullOrEmpty(Configuration.UrlValidation))
      {
        return ValidatorResult.Valid;
      }

      if (Regex.IsMatch(controlValidationValue, Configuration.UrlValidation))
      {
        return ValidatorResult.Valid;
      }

      Text = Configuration.ValidationErrorMessage;
      return GetFailedResult(ValidatorResult.Error);
    }
  }
}