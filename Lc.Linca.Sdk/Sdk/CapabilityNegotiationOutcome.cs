namespace Lc.Linca.Sdk;

/// <summary>
/// Result of an attempt to negotiate capabilities
/// with the Linked Care FHIR Server
/// </summary>
public enum CapabilityNegotiationOutcome
{
    /// <summary>
    /// The negotiation has succeeded, a capability statement
    /// declaring a standards compliant Linked Care implementation
    /// was returned
    /// </summary>
    Succeeded = 0,

    /// <summary>
    /// The capability statement could not be obtained
    /// because there is no active server connection
    /// </summary>
    NotConnected = 1,

    /// <summary>
    /// The capability statement could not be obtained
    /// because there is no authenticated session with the FHIR server
    /// </summary>
    Unauthorized = 2,

    /// <summary>
    /// A capability statement was returned,
    /// but was found to be malformed or of a wrong version
    /// </summary>
    CouldNotParse = 3
}
