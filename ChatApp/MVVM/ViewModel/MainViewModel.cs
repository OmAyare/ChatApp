using ChatApp.MVVM.Core;
using ChatApp.MVVM.Model;
using ChatApp.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatApp.MVVM.ViewModel
{
    class MainViewModel
    {
        public ObservableCollection<UserModel> Users { get; set; }
        public RelayCommand ConnectToServerCommand {  get; set; }

        public string Username { get; set; }

        private Server _server;
        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>();
            _server = new Server();
            _server.connectedEvent += UseeConnected;
                
            ConnectToServerCommand = new RelayCommand(o =>_server.ConnectToServer(Username), o => !String.IsNullOrEmpty(Username));
        }

        private void UseeConnected()
        {
            var user = new UserModel
            {
                Username = _server.PacketReader.ReadMessage(),
                UID = _server.PacketReader.ReadMessage(),
            };

            if (!Users.Any(x => x.UID == user.UID)) 
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }
        }
    }
}
