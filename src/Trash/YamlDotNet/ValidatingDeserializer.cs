using System;
using System.ComponentModel.DataAnnotations;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Trash.YamlDotNet
{
    public class ValidatingDeserializer : INodeDeserializer
    {
        private readonly INodeDeserializer _nodeDeserializer;

        public ValidatingDeserializer(INodeDeserializer nodeDeserializer)
        {
            _nodeDeserializer = nodeDeserializer;
        }

        public bool Deserialize(IParser parser, Type expectedType,
            Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
        {
            if (!_nodeDeserializer.Deserialize(parser, expectedType, nestedObjectDeserializer, out value) ||
                value == null)
            {
                return false;
            }

            var context = new ValidationContext(value, null, null);

            try
            {
                Validator.ValidateObject(value, context, true);
            }
            catch (ValidationException e)
            {
                if (parser.Current == null)
                {
                    throw;
                }

                throw new YamlException(parser.Current.Start, parser.Current.End, e.Message);
            }

            return true;
        }
    }
}
