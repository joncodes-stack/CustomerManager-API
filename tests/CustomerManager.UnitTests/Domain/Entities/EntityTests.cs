using CustomerManager.Domain.Entities;
using FluentAssertions;

namespace CustomerManager.UnitTests.Domain.Entities;

// Concrete subclass to allow instantiation of the abstract Entity
public sealed class ConcreteEntity : Entity
{
    public ConcreteEntity() : base() { }
}

public class EntityTests
{
    [Fact]
    public void Constructor_ShouldGenerateNonEmptyGuid_WhenEntityIsCreated()
    {
        // Arrange & Act
        var entity = new ConcreteEntity();

        // Assert
        entity.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueIds_ForDifferentInstances()
    {
        // Arrange & Act
        var entity1 = new ConcreteEntity();
        var entity2 = new ConcreteEntity();

        // Assert
        entity1.Id.Should().NotBe(entity2.Id);
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingWithSameReference()
    {
        // Arrange
        var entity = new ConcreteEntity();

        // Act & Assert
        entity.Equals(entity).Should().BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenEntitiesHaveDifferentIds()
    {
        // Arrange
        var entity1 = new ConcreteEntity();
        var entity2 = new ConcreteEntity();

        // Act & Assert
        entity1.Equals(entity2).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingWithNull()
    {
        // Arrange
        var entity = new ConcreteEntity();

        // Act & Assert
        entity.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingWithNonEntityObject()
    {
        // Arrange
        var entity = new ConcreteEntity();
        var other = new object();

        // Act & Assert
        entity.Equals(other).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_ShouldReturnSameValue_WhenCalledMultipleTimesOnSameEntity()
    {
        // Arrange
        var entity = new ConcreteEntity();

        // Act
        var hash1 = entity.GetHashCode();
        var hash2 = entity.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_ShouldBeDifferent_ForEntitiesWithDifferentIds()
    {
        // Arrange
        var entity1 = new ConcreteEntity();
        var entity2 = new ConcreteEntity();

        // Act & Assert
        entity1.GetHashCode().Should().NotBe(entity2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ShouldMatchIdHashCode_ForAnyEntity()
    {
        // Arrange
        var entity = new ConcreteEntity();

        // Act & Assert
        entity.GetHashCode().Should().Be(entity.Id.GetHashCode());
    }
}
