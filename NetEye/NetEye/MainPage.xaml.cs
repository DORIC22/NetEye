using NetEye.pages;
using NetEye.res.model;
using NetEye.res.service;
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
        private HttpClientWithJwt _httpClient;

        public MainPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            _httpClient = HttpClientWithJwt.GetInstance();
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

            AuthUser user = null;
            await Task.Run(async () =>
            {
                try
                {
                    user = _httpClient.Authorization(entry_Email.Text, entry_Password.Text);
                    if (user != null)
                    {
                        App.Current.Properties.Remove("UserId");
                        App.Current.Properties.Add("UserId", Convert.ToInt32(user.Id)); //Сохраняем id пользователя
                        string userRole = user.Role.ToString();

                        switch (userRole)
                        {
                            case "User":
                                Device.BeginInvokeOnMainThread(async () =>
                                {
                                    await Navigation.PushAsync(new userPage(user));
                                    DependencyService.Get<IToast>().LongToast("Добро пожаловать, " + user.FullName);
                                });
                                break;
                            case "Tech":
                                Device.BeginInvokeOnMainThread(async () =>
                                {
                                    await Navigation.PushAsync(new techPage(user));
                                    DependencyService.Get<IToast>().LongToast("Добро пожаловать, " + user.FullName);
                                });
                                break;
                            case "Admin":
                                Device.BeginInvokeOnMainThread(async () =>
                                {
                                    await DisplayAlert("Предупреждение", "Функционал администратора в мобильном приложении ограничен," +
                                        "используйте Web версию нашей системы для получения доступа ко всем функциям", "Ок");
                                    await Navigation.PushAsync(new techPage(user));
                                    DependencyService.Get<IToast>().LongToast("Добро пожаловать, " + user.FullName);
                                });
                                break;
                        }
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await DisplayAlert("Что-то пошло не так", "Проверьте Вашу почту и пароль", "Ок");
                        });
                    }
                }
                catch (Exception ex)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("Что-то пошло не так", ex.ToString(), "Ок");
                    });
                }
            });
            #region После попытки авторизации
            aiMain.IsVisible = false;
            btn_Auth.IsVisible = true;
            entry_Email.IsEnabled = true;
            entry_Password.IsEnabled = true;
            #endregion            
        }

        
    }
}
