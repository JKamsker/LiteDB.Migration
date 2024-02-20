using LiteDB.Migration.Helpers;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDB.Migration.Tests;

public class DictCloner
{
    [Fact]
    public void CloneIfDictionary_CanCloneDictionary()
    {
        // Arrange
        var originalDictionary = new Dictionary<int, string>
        {
            { 1, "One" },
            { 2, "Two" },
            { 3, "Three" }
        };

        // Act
        var clonedDictionary = DictionaryCloner.CloneIfDictionary(originalDictionary) as Dictionary<int, string>;

        // Assert
        Assert.NotNull(clonedDictionary);
        Assert.NotSame(originalDictionary, clonedDictionary);
        Assert.Equal(originalDictionary, clonedDictionary);
    }

    [Fact]
    public void CloneIfDictionary_CanCloneConcurrentDictionary()
    {
        // Arrange
        var originalConcurrentDictionary = new ConcurrentDictionary<int, string>();
        originalConcurrentDictionary.TryAdd(4, "Four");
        originalConcurrentDictionary.TryAdd(5, "Five");
        originalConcurrentDictionary.TryAdd(6, "Six");

        // Act
        var clonedConcurrentDictionary = DictionaryCloner.CloneIfDictionary(originalConcurrentDictionary) as ConcurrentDictionary<int, string>;

        // Assert
        Assert.NotNull(clonedConcurrentDictionary);
        Assert.NotSame(originalConcurrentDictionary, clonedConcurrentDictionary);
        Assert.Equal(originalConcurrentDictionary, clonedConcurrentDictionary);
    }
}