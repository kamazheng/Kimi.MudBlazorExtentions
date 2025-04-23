// ***********************************************************************
// <copyright file="FluentValidationService.cs" company="Molex(Chengdu)">
//     Copyright © Molex(Chengdu) 2025
// </copyright>
// ***********************************************************************
// Author           : MOLEX\kzheng
// Created          : 04/01/2025
// ***********************************************************************

namespace Kimi.MudBlazorExtentions.Extensions;

using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

public static class FluentValidationService
{
    // Cache for validators to improve performance
    private static readonly ConcurrentDictionary<Type, object> ValidatorCache = new();

    // Logger for capturing errors or warnings
    private static ILogger? _logger;

    /// <summary>
    /// Sets the logger for this service.
    /// </summary>
    public static void SetLogger(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates a specific property of a model asynchronously.
    /// </summary>
    public static Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));

        try
        {
            Type modelType = model.GetType();

            // Resolve the validator for the model type
            var validator = GetValidatorClass(modelType);
            if (validator == null)
            {
                _logger?.LogWarning("No validator found for model type: {ModelType}", modelType.FullName);
                return Array.Empty<string>();
            }

            // Dynamically resolve types for validation
            Type validationContextType = typeof(ValidationContext<>).MakeGenericType(modelType);
            Type validationStrategyType = typeof(ValidationStrategy<>).MakeGenericType(modelType);

            // Resolve the CreateWithOptions method of ValidationContext<T>
            MethodInfo? createWithOptionsMethod = validationContextType.GetMethod(
                "CreateWithOptions",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { modelType, typeof(Action<>).MakeGenericType(validationStrategyType) },
                null
            );

            if (createWithOptionsMethod == null)
            {
                _logger?.LogError("Failed to resolve 'CreateWithOptions' method for type: {ValidationContextType}", validationContextType.FullName);
                return Array.Empty<string>();
            }

            // Build the IncludeProperties call dynamically
            MethodInfo? includePropertiesMethod = validationStrategyType.GetMethod(
                "IncludeProperties",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(string[]) },
                null
            );

            if (includePropertiesMethod == null)
            {
                _logger?.LogError("Failed to resolve 'IncludeProperties' method for type: {ValidationStrategyType}", validationStrategyType.FullName);
                return Array.Empty<string>();
            }

            var strategyParameter = Expression.Parameter(validationStrategyType, "x");
            var includePropertiesCall = Expression.Call(
                strategyParameter,
                includePropertiesMethod,
                Expression.Constant(new[] { propertyName })
            );
            var options = Expression.Lambda(
                typeof(Action<>).MakeGenericType(validationStrategyType),
                includePropertiesCall,
                strategyParameter
            ).Compile();

            // Create the validation context
            object context = createWithOptionsMethod.Invoke(null, new object[] { model, options })!;

            // Resolve the ValidateAsync method of AbstractValidator<T>
            Type abstractValidatorType = typeof(AbstractValidator<>).MakeGenericType(modelType);
            MethodInfo? validateAsyncMethod = abstractValidatorType.GetMethod(
                "ValidateAsync",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { validationContextType, typeof(CancellationToken) },
                null
            );

            if (validateAsyncMethod == null)
            {
                _logger?.LogError("Failed to resolve 'ValidateAsync' method for type: {AbstractValidatorType}", abstractValidatorType.FullName);
                return Array.Empty<string>();
            }

            // Perform asynchronous validation
            Task<ValidationResult> task = (Task<ValidationResult>)validateAsyncMethod.Invoke(validator, new object[] { context, CancellationToken.None })!;
            ValidationResult result = await task;

            // Return validation errors if invalid, otherwise return an empty array
            return result.IsValid
                ? Array.Empty<string>()
                : result.Errors.Select(e => e.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while validating property '{PropertyName}' for model", propertyName);
            return new[] { "An unexpected error occurred during validation." };
        }
    };

    /// <summary>
    /// Retrieves the validator class for the given model type from the cache or resolves it dynamically.
    /// </summary>
    private static object? GetValidatorClass(Type modelType)
    {
        if (ValidatorCache.TryGetValue(modelType, out var cachedValidator))
        {
            return cachedValidator;
        }

        try
        {
            var validatorType = typeof(AbstractValidator<>).MakeGenericType(modelType);

            // Find the first concrete validator class in the loaded assemblies
            var validatorClass = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.IsSubclassOf(validatorType) && !t.IsAbstract);

            if (validatorClass != null)
            {
                var validatorInstance = Activator.CreateInstance(validatorClass);

                // Ensure we only cache non-null validators
                if (validatorInstance != null)
                {
                    ValidatorCache.TryAdd(modelType, validatorInstance);
                    return validatorInstance;
                }
                else
                {
                    _logger?.LogWarning("Failed to create an instance of validator for model type: {ModelType}", modelType.FullName);
                }
            }
            else
            {
                _logger?.LogWarning("No validator class found for model type: {ModelType}", modelType.FullName);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while resolving validator for model type: {ModelType}", modelType.FullName);
        }

        return null;
    }
}


public static class FluentValidationService<T, V> where V : AbstractValidator<T>, new()
{
    // Cache for validators to improve performance
    private static readonly ConcurrentDictionary<Type, AbstractValidator<T>> ValidatorCache = new();

    // Static property to hold the validator instance
    private static AbstractValidator<T> Validator
    {
        get
        {
            return ValidatorCache.GetOrAdd(typeof(T), _ => new V());
        }
    }

    /// <summary>
    /// Validates a specific property of a model asynchronously.
    /// </summary>
    public static Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));

        // Ensure the model is of type T
        if (model is not T typedModel)
        {
            throw new ArgumentException($"Model must be of type {typeof(T).FullName}.", nameof(model));
        }

        // Perform validation asynchronously with options to include only the specified property
        var result = await Validator.ValidateAsync(
            ValidationContext<T>.CreateWithOptions(typedModel, options => options.IncludeProperties(propertyName))
        );

        // Return an empty collection if valid; otherwise, extract error messages
        return result.IsValid
            ? Array.Empty<string>()
            : result.Errors.Select(e => e.ErrorMessage);
    };
}
