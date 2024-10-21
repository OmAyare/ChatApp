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
        public ObservableCollection<string> Messages { get; set; }
        public RelayCommand ConnectToServerCommand {  get; set; }
        public RelayCommand SendMessageCommand {  get; set; }

        public string Username { get; set; }
        public string Message { get; set; }

        private Server _server;
        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<string>();
            _server = new Server();
            _server.connectedEvent += UseeConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.userDisconectedEvent += RemoveUser;
                
            ConnectToServerCommand = new RelayCommand(o =>_server.ConnectToServer(Username), o => !String.IsNullOrEmpty(Username));
            SendMessageCommand = new RelayCommand(o =>_server.SendMessageToSeerver(Message), o => !String.IsNullOrEmpty(Message));
        }

        private void RemoveUser()
        {
            var uid = _server.PacketReader.ReadMessage();
            var user = Users.Where(z => z.UID == uid).FirstOrDefault();
            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }

        private void MessageReceived()
        {
            var msg = _server.PacketReader.ReadMessage();
            Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));
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
