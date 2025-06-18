using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bird.Framework
{
    /// <summary>
    /// Manages JSON payloads for API tests. Provides functionality to load JSON files,
    /// modify their contents, and remove fields. This class is used to prepare test data
    /// for API requests.
    /// </summary>
    public class JsonPayloadManager
    {
        // Directory containing JSON payload files
        private readonly string _payloadsDirectory;

        /// <summary>
        /// Initializes a new instance of the JsonPayloadManager class.
        /// </summary>
        /// <param name="payloadsDirectory">Directory containing JSON payload files (default: "TestData")</param>
        public JsonPayloadManager(string payloadsDirectory = "TestData")
        {
            _payloadsDirectory = payloadsDirectory;
        }

        /// <summary>
        /// Loads a JSON file and optionally modifies its contents.
        /// </summary>
        /// <param name="payloadFileName">Name of the JSON file to load</param>
        /// <param name="modifications">Dictionary of modifications to apply (path -> value)</param>
        /// <param name="fieldsToRemove">List of fields to remove from the JSON</param>
        /// <returns>Modified JsonNode</returns>
        /// <exception cref="InvalidOperationException">Thrown when JSON parsing fails</exception>
        public async Task<JsonNode> LoadAndModifyPayloadAsync(
            string payloadFileName,
            Dictionary<string, object>? modifications = null,
            List<string>? fieldsToRemove = null)
        {
            var payloadPath = Path.Combine(_payloadsDirectory, payloadFileName);
            var jsonContent = await File.ReadAllTextAsync(payloadPath);
            var jsonNode = JsonNode.Parse(jsonContent) ?? 
                throw new InvalidOperationException($"Failed to parse JSON from {payloadFileName}");

            if (modifications != null)
            {
                foreach (var modification in modifications)
                {
                    ModifyJsonNode(jsonNode, modification.Key, modification.Value);
                }
            }

            if (fieldsToRemove != null)
            {
                foreach (var field in fieldsToRemove)
                {
                    RemoveJsonNode(jsonNode, field);
                }
            }

            return jsonNode;
        }

        /// <summary>
        /// Modifies a value in the JSON node at the specified path.
        /// </summary>
        /// <param name="node">JsonNode to modify</param>
        /// <param name="path">Dot-notation path to the value (e.g., "user.name")</param>
        /// <param name="value">New value to set</param>
        /// <exception cref="ArgumentNullException">Thrown when node is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when path cannot be accessed</exception>
        private void ModifyJsonNode(JsonNode node, string path, object value)
        {
            ArgumentNullException.ThrowIfNull(node);
            var parts = path.Split('.');
            var current = node;

            // Navigate to the parent node
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (current[parts[i]] == null)
                {
                    current[parts[i]] = new JsonObject();
                }
                current = current[parts[i]] ?? 
                    throw new InvalidOperationException($"Failed to access path {path}");
            }

            // Set the value at the final path
            var lastPart = parts[^1];
            if (value == null)
            {
                current[lastPart] = null;
            }
            else if (value is JsonNode jsonNode)
            {
                current[lastPart] = jsonNode;
            }
            else
            {
                current[lastPart] = JsonValue.Create(value);
            }
        }

        /// <summary>
        /// Removes a field from the JSON node at the specified path.
        /// </summary>
        /// <param name="node">JsonNode to modify</param>
        /// <param name="path">Dot-notation path to the field to remove</param>
        /// <exception cref="ArgumentNullException">Thrown when node is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when path cannot be accessed</exception>
        private void RemoveJsonNode(JsonNode node, string path)
        {
            ArgumentNullException.ThrowIfNull(node);
            var parts = path.Split('.');
            var current = node;

            // Navigate to the parent node
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (current[parts[i]] == null)
                {
                    return;
                }
                current = current[parts[i]] ?? 
                    throw new InvalidOperationException($"Failed to access path {path}");
            }

            // Remove the field at the final path
            var lastPart = parts[^1];
            if (current is JsonObject jsonObject)
            {
                jsonObject.Remove(lastPart);
            }
        }
    }
} 