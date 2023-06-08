using NetEye.pages;
using NetEye.res.model;
using NetEye.res.service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NetEye
{
    public partial class MainPage : ContentPage
    {
        private HttpClientWithJwt _httpClient;
        AuthUser user = null;

        public MainPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            _httpClient = HttpClientWithJwt.GetInstance();

            AutomaticAuth();
        }

        public void AutomaticAuth()
        {
            string autAuth = "false";

            try
            {
                autAuth = App.Current.Properties["rememberMe"].ToString();
            } catch(Exception ex) { }

            if ( autAuth == "true" )
            {
                entry_Email.Text = App.Current.Properties["email"].ToString();
                entry_Password.Text = App.Current.Properties["password"].ToString();
                btn_Auth_Clicked(null, null);
            }
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

        bool isValidEmail(string email)
        {
            string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
            Match isMatch = Regex.Match(email, pattern, RegexOptions.IgnoreCase);
            return isMatch.Success;
        }

        private async void changeUserPassword()
        {
            if (entry_Password.Text == "Passw0rd")
            {
                string newPassword = await DisplayPromptAsync("Сброс пароля", "Придумайте новый пароль, и введите его в поле ниже:", "Ок", "Отмена", "...");

                if (!string.IsNullOrEmpty(newPassword))
                {
                    if (CheckPasswordComplexity(newPassword))
                    {
                        string newPasswordAgain = await DisplayPromptAsync("Сброс пароля", "Введите пароль еще раз", "Ок", "Отмена", "...");

                        if (newPassword == newPasswordAgain)
                        {
                            // запрос к бд на смену пароля.
                            _httpClient.PathPassword(user.Id, newPassword);

                            await DisplayAlert("Успешно", "Ваш пароль был успешно изменён.", "Ок");
                        }
                        else
                        {
                            DependencyService.Get<IToast>().LongToast("Пароли не совпадают");
                            changeUserPassword();
                        }
                    }
                    else
                    {
                        DependencyService.Get<IToast>().LongToast("Придумайте более сложный пароль");
                        changeUserPassword();
                    }
                }
                else
                {
                    DependencyService.Get<IToast>().LongToast("Поле пароля не может быть пустым");
                    changeUserPassword();
                }
            }
        }

        public bool CheckPasswordComplexity(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }

            if (password == "Passw0rd")
            {
                return false;
            }

            if (password.Length < 8)
            {
                // Пароль слишком короткий
                return false;
            }

            bool hasDigit = false;
            bool hasLetter = false;

            foreach (char c in password)
            {
                if (Char.IsDigit(c))
                {
                    hasDigit = true;
                }
                else if (Char.IsLetter(c))
                {
                    hasLetter = true;
                }

                if (hasDigit && hasLetter)
                {
                    // Пароль удовлетворяет всем требованиям
                    return true;
                }
            }

            // Пароль не содержит как минимум одну цифру и одну букву
            return false;
        }


        private async void btn_Auth_Clicked(object sender, EventArgs e)
        {
            bool emailAnswer = false;
            if (!string.IsNullOrEmpty(entry_Email.Text))
                emailAnswer = isValidEmail(entry_Email.Text);

            if (!emailAnswer)
            {
                await DisplayAlert("Упс", "Введена некорректная почта!", "Ок");
                return;
            }

            if (string.IsNullOrEmpty(entry_Password.Text))
            {
                await DisplayAlert("Упс", "Введен некорректный пароль!", "Ок");
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

            bool auth = false;
            
            await Task.Run(async () =>
            {
                try
                {
                    user = _httpClient.Authorization(entry_Email.Text, entry_Password.Text);
                    if (user != null)
                    {
                        auth = true;

                        if (rememberMeCheckBox.IsChecked == true)
                        {
                            App.Current.Properties.Add("rememberMe", "true"); 
                            App.Current.Properties.Add("email", entry_Email.Text);
                            App.Current.Properties.Add("password", entry_Password.Text);
                        }                        

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

            #region проверка на Passw0rd (Сброс пароля)
            if (auth)
                changeUserPassword();
            #endregion
        }


    }
}
