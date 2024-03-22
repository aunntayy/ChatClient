
namespace Communications {
    public class Networking : INetworking {
        string INetworking.ID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        bool INetworking.IsConnected => throw new NotImplementedException();

        bool INetworking.IsWaitingForClients => throw new NotImplementedException();

        string INetworking.RemoteAddressPort => throw new NotImplementedException();

        string INetworking.LocalAddressPort => throw new NotImplementedException();

        Task INetworking.ConnectAsync(string host, int port) {
            throw new NotImplementedException();
        }

        void INetworking.Disconnect() {
            throw new NotImplementedException();
        }

        Task INetworking.HandleIncomingDataAsync(bool infinite) {
            throw new NotImplementedException();
        }

        Task INetworking.SendAsync(string text) {
            throw new NotImplementedException();
        }

        void INetworking.StopWaitingForClients() {
            throw new NotImplementedException();
        }

        void INetworking.StopWaitingForMessages() {
            throw new NotImplementedException();
        }

        Task INetworking.WaitForClientsAsync(int port, bool infinite) {
            throw new NotImplementedException();
        }
    }
}