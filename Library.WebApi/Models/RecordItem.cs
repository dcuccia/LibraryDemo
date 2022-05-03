// Copyright (c) IEvangelist. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Azure.CosmosRepository.Attributes;
using Newtonsoft.Json;
using Microsoft.Azure.CosmosRepository;

namespace Library.WebApi.Models;
    
/// <summary>
/// Mirror of Microsoft.Azure.CosmosRepository.Item class, but a struct instead
/// </summary>
public abstract record RecordItem : IItem
{
    /// <summary>
    /// Gets or sets the item's globally unique identifier.
    /// </summary>
    /// <remarks>
    /// Initialized by <see cref="Guid.NewGuid"/>.
    /// </remarks>
    [JsonProperty("id")]
    public virtual string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the item's type name. This is used as a discriminator.
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// Gets the PartitionKey based on <see cref="GetPartitionKeyValue"/>.
    /// Implemented explicitly to keep out of Item API
    /// </summary>
    string IItem.PartitionKey => GetPartitionKeyValue();

    /// <summary>
    /// Default constructor, assigns type name to <see cref="Type"/> property.
    /// </summary>
    public RecordItem() => Type = GetType().Name;

    /// <summary>
    /// Gets the partition key value for the given <see cref="Item"/> type.
    /// When overridden, be sure that the <see cref="PartitionKeyPathAttribute.Path"/> value corresponds
    /// to the <see cref="JsonPropertyAttribute.PropertyName"/> value, i.e.; "/partition" and "partition"
    /// respectively. If these two values do not correspond an error will occur.
    /// </summary>
    /// <returns>The <see cref="Item.Id"/> unless overridden by the subclass.</returns>
    protected virtual string GetPartitionKeyValue() => Id;
}