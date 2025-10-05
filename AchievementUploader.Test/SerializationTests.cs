using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using AchievementUploader.Models;
using Shouldly;

namespace AchievementUploader.Test;

public class SerializationTests
{
    record Nested(string Name);
    record Foo(string Name, NullAsFalse<Nested> Test);

    [Test]
    public void ShouldRoundTripNullAsFalse()
    {
        var opt = new JsonSerializerOptions();
        opt.AddNullFalseSupport();

        var t = new Foo("Foo", new Nested("Bar"));

        var j = JsonSerializer.Serialize(t, opt);
        Console.WriteLine(j);
        JsonSerializer.Deserialize<Foo>(j, opt).ShouldBeEquivalentTo(t);

        var t2 = t with { Test = null };
        j = JsonSerializer.Serialize(t2, opt);
        j.ShouldContain("false");
        Console.WriteLine(j);
        JsonSerializer.Deserialize<Foo>(j, opt).ShouldBeEquivalentTo(t2);
    }
}
