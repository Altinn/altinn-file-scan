using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using Altinn.FileScan.Models;

using Xunit;

namespace Altinn.FileScan.Tests.TestingModels
{
    public class MuecheliResponseTests
    {
        [Fact]
        public void Deserialize()
        {
            string jsonContent = "[{ \"Filename\":\"Attachment.pdf\",\"Result\":\"OK\"}]";
            MuescheliResponse r = JsonSerializer.Deserialize<List<MuescheliResponse>>(
                jsonContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                })
             .FirstOrDefault();
        }
    }
}
