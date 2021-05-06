using System.Collections.Generic;

namespace Trash.Radarr.Api.Objects
{
    public class SpecificationField
    {
        public int Order { get; set; }
        public string Name { get; set; } = "";
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
        public string Type { get; set; } = "";
        public bool Advanced { get; set; }
    }

    public class Specification
    {
        public string Name { get; set; } = "";
        public string Implementation { get; set; } = "";
        public string ImplementationName { get; set; } = "";
        public string InfoLink { get; set; } = "";
        public bool Negate { get; set; }
        public bool Required { get; set; }
        public List<SpecificationField> Fields { get; set; } = new();
    }

    public class CustomFormatItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IncludeCustomFormatWhenRenaming { get; set; }
        public List<Specification> Specifications { get; set; } = new();
    }
}
