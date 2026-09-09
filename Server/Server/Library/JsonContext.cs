using System.Text.Json;
using System.Text.Json.Serialization;

using Fido2NetLib;
using Fido2NetLib.Objects;

[JsonSerializable(typeof(AuthenticatorAttestationRawResponse))]
[JsonSerializable(typeof(AuthenticatorAssertionRawResponse))]
[JsonSerializable(typeof(JsonElement))]
internal partial class AppJsonContext : JsonSerializerContext
{ }