using JOSYN.Foundation.ResultPattern;

namespace JOSYN.System.Shared.Contract;

/// <summary>
/// Contract definition for the JOSYN Application Protocol (JAP).
/// Describes the communication between the <b>JobHost</b> (frontend) and
/// the <b>JAPServer</b> (backend) at the application layer — independent of
/// the transport mechanism.
/// <para>
/// The caller (<c>JobHost</c>) retrieves <see cref="GetRawArguments"/>
/// to receive the job assignment, executes the job, and submits
/// the result via <see cref="PutRawResult"/> — or any error that occurred
/// via <see cref="PutError"/>.
/// </para>
/// </summary>
public interface IJosynApplicationProtocol
{
    /// <summary>
    /// Retrieves the serialized job arguments as a raw string.
    /// The caller is responsible for deserialization.
    /// </summary>
    /// <returns>
    /// Serialized argument string on success;
    /// failure if no arguments are available or the transport fails.
    /// </returns>
    Task<Result<string>> GetRawArguments();

    /// <summary>
    /// Submits the job result as a serialized string.
    /// The caller is responsible for serialization.
    /// </summary>
    /// <param name="result">Serialized job result.</param>
    /// <returns>
    /// Successful when the result has been submitted;
    /// failure if the transport fails.
    /// </returns>
    Task<Result> PutRawResult(string result);

    /// <summary>
    /// Submits an error that occurred as a serialized <see cref="ErrorReport"/>.
    /// Serves for error logging on the server side.
    /// If the transport itself fails, the error remains in the caller's local log.
    /// </summary>
    /// <param name="serializedError">A <see cref="ErrorReport"/> serialized via PropertyBag.</param>
    Task<Result> PutError(string serializedError);
}
