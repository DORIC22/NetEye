using NetEye.pages;
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
            NavigationPage.SetHasNavigationBar(this, false);
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
            if (string.IsNullOrEmpty(entry_Email.Text) || string.IsNullOrEmpty(entry_Password.Text))
            {
                await DisplayAlert("Не верно заполнены поля", "Пожалуйста, введите почту и пароль", "Ок");
                return;
            }


            bool state = checkNetworkState();
            if (!state) {
                await DisplayAlert("Упс", "Ошибка интернет соединения", "Ок");
                return;
            }
            #region при попытке авторизации
            entry_Email.IsEnabled = false;
            entry_Password.IsEnabled = false;
            aiMain.IsVisible = true;
            btn_Auth.IsVisible= false;
            #endregion
            await Task.Run(() =>
            {
                //Процесс обращения к серверу
                Thread.Sleep(1000);
            });
            #region После попытки авторизации
            aiMain.IsVisible = false;
            btn_Auth.IsVisible = true;
            entry_Email.IsEnabled = true;
            entry_Password.IsEnabled = true;
            #endregion

            await Navigation.PushAsync(new userPage());
            

        }

        
    }
}
