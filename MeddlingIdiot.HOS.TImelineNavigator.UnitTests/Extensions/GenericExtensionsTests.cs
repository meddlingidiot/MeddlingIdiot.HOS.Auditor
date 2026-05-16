using MeddlingIdiot.HOS.TimelineNavigator.Extensions;

namespace MeddlingIdiot.HOS.TimelineNavigator.UnitTests.Extensions;

public class GenericExtensionsTests
{
    // ── helper types ──────────────────────────────────────────────────────────

    private class SimplePoco
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private class PocoWithList
    {
        public List<int> Items { get; set; } = new();
    }

    private class PocoWithComplexList
    {
        public List<SimplePoco> Items { get; set; } = new();
    }

    private class PocoWithNested
    {
        public SimplePoco? Inner { get; set; }
    }

    // Interface property: not IComparable, not IEnumerable, not a class → hits "Cannot compare" branch
    private interface IOpaque { }
    private class PocoWithOpaque
    {
        public IOpaque? Value { get; set; }
    }

    // Struct without IComparable → hits object.Equals path in AreValuesEqual
    private struct NonComparableStruct { public int X; }
    private class PocoWithStruct
    {
        public NonComparableStruct Value { get; set; }
    }

    private enum Color { Red, Green, Blue }

    // ── CanSerialize ──────────────────────────────────────────────────────────

    [Test]
    public async Task CanSerialize_RoundTrips_PrimitiveValue()
    {
        var result = 42.CanSerialize();
        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async Task CanSerialize_RoundTrips_StringValue()
    {
        var result = "hello".CanSerialize();
        await Assert.That(result).IsEqualTo("hello");
    }

    // ── DeepEquals ── both-null / one-null ───────────────────────────────────

    [Test]
    public async Task DeepEquals_BothNull_ReturnsTrue()
    {
        var result = ((object?)null).DeepEquals(null);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task DeepEquals_OneNull_ReturnsFalse()
    {
        var nullResult = ((object?)null).DeepEquals(new SimplePoco());
        var otherResult = new SimplePoco().DeepEquals(null);
        await Assert.That(nullResult).IsFalse();
        await Assert.That(otherResult).IsFalse();
    }

    // ── DeepEquals ── primitive / directly-comparable properties ─────────────

    [Test]
    public async Task DeepEquals_SamePrimitiveProperties_ReturnsTrue()
    {
        var a = new SimplePoco { Id = 1, Name = "A" };
        var b = new SimplePoco { Id = 1, Name = "A" };
        await Assert.That(a.DeepEquals(b)).IsTrue();
    }

    [Test]
    public async Task DeepEquals_DifferentPrimitiveProperty_ReturnsFalse()
    {
        var a = new SimplePoco { Id = 1, Name = "A" };
        var b = new SimplePoco { Id = 2, Name = "A" };
        await Assert.That(a.DeepEquals(b)).IsFalse();
    }

    [Test]
    public async Task DeepEquals_IgnoreList_SkipsNamedProperty()
    {
        var a = new SimplePoco { Id = 1, Name = "A" };
        var b = new SimplePoco { Id = 1, Name = "DIFFERENT" };
        await Assert.That(a.DeepEquals(b, nameof(SimplePoco.Name))).IsTrue();
    }

    // ── DeepEquals ── non-IComparable struct → object.Equals path ────────────

    [Test]
    public async Task DeepEquals_NonComparableStructProperties_Equal_ReturnsTrue()
    {
        var a = new PocoWithStruct { Value = new NonComparableStruct { X = 5 } };
        var b = new PocoWithStruct { Value = new NonComparableStruct { X = 5 } };
        await Assert.That(a.DeepEquals(b)).IsTrue();
    }

    [Test]
    public async Task DeepEquals_NonComparableStructProperties_Different_ReturnsFalse()
    {
        var a = new PocoWithStruct { Value = new NonComparableStruct { X = 5 } };
        var b = new PocoWithStruct { Value = new NonComparableStruct { X = 9 } };
        await Assert.That(a.DeepEquals(b)).IsFalse();
    }

    // ── DeepEquals ── IEnumerable properties ─────────────────────────────────

    [Test]
    public async Task DeepEquals_OneCollectionNullOtherNot_ReturnsFalse()
    {
        var a = new PocoWithList { Items = null! };
        var b = new PocoWithList { Items = new List<int> { 1 } };
        await Assert.That(a.DeepEquals(b)).IsFalse();
    }

    [Test]
    public async Task DeepEquals_CollectionsDifferentCount_ReturnsFalse()
    {
        var a = new PocoWithList { Items = new List<int> { 1 } };
        var b = new PocoWithList { Items = new List<int> { 1, 2 } };
        await Assert.That(a.DeepEquals(b)).IsFalse();
    }

    [Test]
    public async Task DeepEquals_CollectionsSameItems_ReturnsTrue()
    {
        var a = new PocoWithList { Items = new List<int> { 1, 2, 3 } };
        var b = new PocoWithList { Items = new List<int> { 1, 2, 3 } };
        await Assert.That(a.DeepEquals(b)).IsTrue();
    }

    [Test]
    public async Task DeepEquals_CollectionsDifferentItems_ReturnsFalse()
    {
        var a = new PocoWithList { Items = new List<int> { 1, 2, 3 } };
        var b = new PocoWithList { Items = new List<int> { 1, 2, 9 } };
        await Assert.That(a.DeepEquals(b)).IsFalse();
    }

    [Test]
    public async Task DeepEquals_CollectionsOfComplexItems_Equal_ReturnsTrue()
    {
        var a = new PocoWithComplexList { Items = new List<SimplePoco> { new() { Id = 1, Name = "X" } } };
        var b = new PocoWithComplexList { Items = new List<SimplePoco> { new() { Id = 1, Name = "X" } } };
        await Assert.That(a.DeepEquals(b)).IsTrue();
    }

    [Test]
    public async Task DeepEquals_CollectionsOfComplexItems_Different_ReturnsFalse()
    {
        var a = new PocoWithComplexList { Items = new List<SimplePoco> { new() { Id = 1, Name = "X" } } };
        var b = new PocoWithComplexList { Items = new List<SimplePoco> { new() { Id = 2, Name = "Y" } } };
        await Assert.That(a.DeepEquals(b)).IsFalse();
    }

    // ── DeepEquals ── nested class property ──────────────────────────────────

    [Test]
    public async Task DeepEquals_NestedClassProperties_Equal_ReturnsTrue()
    {
        var a = new PocoWithNested { Inner = new SimplePoco { Id = 7, Name = "N" } };
        var b = new PocoWithNested { Inner = new SimplePoco { Id = 7, Name = "N" } };
        await Assert.That(a.DeepEquals(b)).IsTrue();
    }

    [Test]
    public async Task DeepEquals_NestedClassProperties_Different_ReturnsFalse()
    {
        var a = new PocoWithNested { Inner = new SimplePoco { Id = 7, Name = "N" } };
        var b = new PocoWithNested { Inner = new SimplePoco { Id = 8, Name = "N" } };
        await Assert.That(a.DeepEquals(b)).IsFalse();
    }

    // ── DeepEquals ── interface property → "Cannot compare" branch ───────────

    [Test]
    public async Task DeepEquals_InterfaceProperty_ReturnsFalse()
    {
        var a = new PocoWithOpaque();
        var b = new PocoWithOpaque();
        await Assert.That(a.DeepEquals(b)).IsFalse();
    }

    // ── DeepClone ─────────────────────────────────────────────────────────────

    [Test]
    public async Task DeepClone_Null_ThrowsArgumentNullException()
    {
        SimplePoco? obj = null;
        await Assert.That(() => obj!.DeepClone()).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task DeepClone_Primitive_ReturnsEqualValue()
    {
        var result = 99.DeepClone();
        await Assert.That(result).IsEqualTo(99);
    }

    [Test]
    public async Task DeepClone_Enum_ReturnsEqualValue()
    {
        var result = Color.Green.DeepClone();
        await Assert.That(result).IsEqualTo(Color.Green);
    }

    [Test]
    public async Task DeepClone_String_ReturnsEqualValue()
    {
        var result = "clone me".DeepClone();
        await Assert.That(result).IsEqualTo("clone me");
    }

    [Test]
    public async Task DeepClone_Array_ReturnsDeepCopy()
    {
        var original = new int[] { 1, 2, 3 };
        var clone = original.DeepClone();

        using (Assert.Multiple())
        {
            await Assert.That(clone).IsNotSameReferenceAs(original);
            await Assert.That(clone).IsEquivalentTo(original);
        }
    }

    [Test]
    public async Task DeepClone_Class_ReturnsDeepCopy()
    {
        var original = new SimplePoco { Id = 10, Name = "orig" };
        var clone = original.DeepClone();

        using (Assert.Multiple())
        {
            await Assert.That(clone).IsNotSameReferenceAs(original);
            await Assert.That(clone!.Id).IsEqualTo(10);
            await Assert.That(clone.Name).IsEqualTo("orig");
        }
    }

    [Test]
    public async Task DeepClone_Class_MutatingCloneDoesNotAffectOriginal()
    {
        var original = new SimplePoco { Id = 10, Name = "orig" };
        var clone = original.DeepClone()!;
        clone.Id = 99;

        await Assert.That(original.Id).IsEqualTo(10);
    }
}
