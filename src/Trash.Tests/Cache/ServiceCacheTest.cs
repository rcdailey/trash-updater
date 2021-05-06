using System;
using System.IO;
using System.IO.Abstractions;
using FluentAssertions;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Trash.Cache;
using Trash.Config;

namespace Trash.Tests.Cache
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ServiceCacheTest
    {
        private class Context
        {
            public Context(IFileSystem? fs = null)
            {
                Filesystem = fs ?? Substitute.For<IFileSystem>();
                StoragePath = Substitute.For<ICacheStoragePath>();
                ServiceConfig = Substitute.For<IServiceConfiguration>();
                Cache = new ServiceCache(Filesystem, StoragePath, ServiceConfig);
            }

            public ServiceCache Cache { get; }
            public IServiceConfiguration ServiceConfig { get; }
            public ICacheStoragePath StoragePath { get; }
            public IFileSystem Filesystem { get; }
        }

        private class ObjectWithoutAttribute
        {
        }

        private const string ValidObjectName = "azAZ_09";

        [CacheObjectName(ValidObjectName)]
        private class ObjectWithAttribute
        {
            public string TestValue { get; init; } = "";
        }

        [CacheObjectName("invalid+name")]
        private class ObjectWithAttributeInvalidChars
        {
        }

        [Test]
        public void Load_NoFileExists_ReturnsNull()
        {
            var ctx = new Context();
            ctx.Filesystem.File.Exists(Arg.Any<string>()).Returns(false);

            var result = ctx.Cache.Load<ObjectWithAttribute>();
            result.Should().BeNull();
        }

        [Test]
        public void Load_WithAttribute_ParsesCorrectly()
        {
            var ctx = new Context();

            ctx.StoragePath.Path.Returns("testpath");

            dynamic testJson = new {TestValue = "Foo"};
            ctx.Filesystem.File.Exists(Arg.Any<string>()).Returns(true);
            ctx.Filesystem.File.ReadAllText(Arg.Any<string>())
                .Returns(_ => JsonConvert.SerializeObject(testJson));

            var obj = ctx.Cache.Load<ObjectWithAttribute>();

            obj.Should().NotBeNull();
            obj!.TestValue.Should().Be("Foo");
            ctx.Filesystem.File.Received().ReadAllText(Path.Join("testpath", "c59d1c81", $"{ValidObjectName}.json"));
        }

        [Test]
        public void Load_WithAttributeInvalidName_ThrowsException()
        {
            var ctx = new Context();

            Action act = () => ctx.Cache.Load<ObjectWithAttributeInvalidChars>();

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*'invalid+name' has unacceptable characters*");
        }

        [Test]
        public void Load_WithoutAttribute_Throws()
        {
            var ctx = new Context();

            Action act = () => ctx.Cache.Load<ObjectWithoutAttribute>();

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("CacheObjectNameAttribute is missing*");
        }

        [Test]
        public void Save_WithAttribute_ParsesCorrectly()
        {
            var ctx = new Context();

            ctx.StoragePath.Path.Returns("testpath");

            ctx.Cache.Save(new ObjectWithAttribute {TestValue = "Foo"});

            var expectedParentDirectory = Path.Join("testpath", "c59d1c81");
            ctx.Filesystem.Directory.Received().CreateDirectory(expectedParentDirectory);

            dynamic expectedJson = new {TestValue = "Foo"};
            var expectedPath = Path.Join(expectedParentDirectory, $"{ValidObjectName}.json");
            ctx.Filesystem.File.Received()
                .WriteAllText(expectedPath, JsonConvert.SerializeObject(expectedJson, Formatting.Indented));
        }

        [Test]
        public void Save_WithAttributeInvalidName_ThrowsException()
        {
            var ctx = new Context();

            Action act = () => ctx.Cache.Save(new ObjectWithAttributeInvalidChars());

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*'invalid+name' has unacceptable characters*");
        }

        [Test]
        public void Save_WithoutAttribute_Throws()
        {
            var ctx = new Context();

            Action act = () => ctx.Cache.Save(new ObjectWithoutAttribute());

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("CacheObjectNameAttribute is missing*");
        }
    }
}
