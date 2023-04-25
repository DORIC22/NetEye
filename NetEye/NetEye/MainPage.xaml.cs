using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NetEye
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Предупреждение","Перейти по ссылке?", "Перейти", "Отмена");
            if (answer)
                await Launcher.OpenAsync(new Uri("http://admin.net-eye.ru"));
        }

        public bool checkNetworkState()
        {
            bool state= false;
            var current = Connectivity.NetworkAccess;

            if (current == NetworkAccess.Internet)
                state = true;            
            return state;
        }

        private async void btn_Auth_Clicked(object sender, EventArgs e)
        {
            bool state = checkNetworkState();
            if (!state) {
                await DisplayAlert("Упс", "Ошибка интернет соединения", "Ок");
                return;
            }
            aiMain.IsVisible = true;
            await Task.Run(() =>
            {
                Thread.Sleep(5000);
            });
            aiMain.IsVisible = false;



        }
    }
}
