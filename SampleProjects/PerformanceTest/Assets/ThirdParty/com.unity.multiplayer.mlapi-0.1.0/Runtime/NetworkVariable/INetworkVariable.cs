using System.IO;
using MLAPI.Transports;

namespace MLAPI.NetworkVariable
{
    /// <summary>
    /// Interface for network value containers
    /// </summary>
    public interface INetworkVariable
    {
        /// <summary>
        /// Returns the name of the channel to be used for syncing
        /// </summary>
        /// <returns>The name of the channel to be used for syncing</returns>
        NetworkChannel GetChannel();

        /// <summary>
        /// Resets the dirty state and marks the variable as synced / clean
        /// </summary>
        void ResetDirty();

        /// <summary>
        /// Gets Whether or not the container is dirty
        /// </summary>
        /// <returns>Whether or not the container is dirty</returns>
        bool IsDirty();

        /// <summary>
        /// Gets Whether or not a specific client can write to the varaible
        /// </summary>
        /// <param name="clientId">The clientId of the remote client</param>
        /// <returns>Whether or not the client can write to the variable</returns>
        bool CanClientWrite(ulong clientId);

        /// <summary>
        /// Gets Whether or not a specific client can read to the varaible
        /// </summary>
        /// <param name="clientId">The clientId of the remote client</param>
        /// <returns>Whether or not the client can read to the variable</returns>
        bool CanClientRead(ulong clientId);

        /// <summary>
        /// Writes the dirty changes, that is, the changes since the variable was last dirty, to the writer
        /// </summary>
        /// <param name="stream">The stream to write the dirty changes to</param>
        void WriteDelta(Stream stream);

        /// <summary>
        /// Writes the complete state of the variable to the writer
        /// </summary>
        /// <param name="stream">The stream to write the state to</param>
        void WriteField(Stream stream);

        /// <summary>
        /// Reads the complete state from the reader and applies it
        /// </summary>
        /// <param name="stream">The stream to read the state from</param>
        /// <param name="localTick">The local network tick at which this var was written, on the machine it was written </param>
        /// <param name="remoteTick">The remote network tick at which this var was sent by the host </param>
        void ReadField(Stream stream, ushort localTick, ushort remoteTick);

        /// <summary>
        /// Reads delta from the reader and applies them to the internal value
        /// </summary>
        /// <param name="stream">The stream to read the delta from</param>
        /// <param name="keepDirtyDelta">Whether or not the delta should be kept as dirty or consumed</param>
        /// <param name="localTick">The local network tick at which this var was written, on the machine it was written </param>
        /// <param name="remoteTick">The remote network tick at which this var was sent by the host </param>
        void ReadDelta(Stream stream, bool keepDirtyDelta, ushort localTick, ushort remoteTick);

        /// <summary>
        /// Sets NetworkBehaviour the container belongs to.
        /// </summary>
        /// <param name="behaviour">The behaviour the container behaves to</param>
        void SetNetworkBehaviour(NetworkBehaviour behaviour);

        /// <summary>
        /// Accessor for the RemoteTick stored in the networkVariable, list, set or dictionary
        /// </summary>
        ushort RemoteTick { get; }
    }
}
