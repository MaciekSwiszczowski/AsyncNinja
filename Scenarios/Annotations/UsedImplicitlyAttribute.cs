using System;

// ReSharper disable once CheckNamespace
namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.All, Inherited = false)]
internal sealed class UsedImplicitlyAttribute : Attribute
{
}
