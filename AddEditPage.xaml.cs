using BaibakovLanguage;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BaibakovLanguage
{
    /// <summary>
    /// Логика взаимодействия для AddEditPage.xaml
    /// </summary>
    public partial class AddEditPage : Page
    {
        private Client _currentClient = new Client();

        public AddEditPage(Client SelectedClient)
        {
            InitializeComponent();
            if (SelectedClient != null)
            {
                _currentClient = SelectedClient;

                if (_currentClient.GenderCode == "м")
                    RBtnMal.IsChecked = true;
                else
                    RBtnFem.IsChecked = true;

            }
            else
            {
                TBID.Text = (Baibakov_languageEntities.GetContext().Client.OrderByDescending(x => x.ID).First().ID + 1).ToString();
                _currentClient.PhotoPath = null;
                TBID.Visibility = Visibility.Hidden;
                TBID1.Visibility = Visibility.Hidden;

            }

            DataContext = _currentClient;
        }

        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return false;

            // Разрешенные символы: цифры, '+', '-', '(', ')', пробел
            var allowedChars = new[] { '+', '-', '(', ')', ' ' };

            foreach (char c in phoneNumber)
            {
                if (!char.IsDigit(c) && !allowedChars.Contains(c))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Проверка на английские буквы, цифры и разрешенные символы
            if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                return false;

            // Дополнительные проверки
            int atIndex = email.IndexOf('@');

            // Проверка что @ присутствует и не первый символ
            if (atIndex <= 0)
                return false;

            string domainPart = email.Substring(atIndex + 1);

            // Проверка что после @ есть точка
            if (!domainPart.Contains('.'))
                return false;

            // Проверка что точка не сразу после @
            if (domainPart.StartsWith("."))
                return false;

            // Проверка что после последней точки минимум 2 символа
            int lastDotIndex = domainPart.LastIndexOf('.');
            if (lastDotIndex == domainPart.Length - 1 ||
                domainPart.Substring(lastDotIndex + 1).Length < 2)
                return false;

            return true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(TBLastName.Text) || !TBLastName.Text.All(c => char.IsLetter(c) || c == ' ' || c == '-') || TBLastName.Text.Length > 50)
                errors.AppendLine("Фамилия может содержать только буквы, пробелы и дефисы, и не может быть длиннее 50 символов.");
            if (string.IsNullOrWhiteSpace(TBFirstName.Text) || !TBFirstName.Text.All(c => char.IsLetter(c) || c == ' ' || c == '-') || TBFirstName.Text.Length > 50)
                errors.AppendLine("Имя может содержать только буквы, пробелы и дефисы, и не может быть длиннее 50 символов.");
            if (string.IsNullOrWhiteSpace(TBPatron.Text) || !TBPatron.Text.All(c => char.IsLetter(c) || c == ' ' || c == '-') || TBPatron.Text.Length > 50)
                errors.AppendLine("Отчество может содержать только буквы, пробелы и дефисы, и не может быть длиннее 50 символов.");

            if (string.IsNullOrWhiteSpace(TBEmail.Text))
                errors.AppendLine("Email не может быть пустым.");
            else
            {
                if (!IsValidEmail(TBEmail.Text))
                {
                    errors.AppendLine("Укажите правильный email агента.");
                }
            }

            if (!IsValidPhoneNumber(TBNumber.Text))
               errors.AppendLine("Телефон может содержать только цифры, плюс, минус, скобки, пробел. Не может быть пустым");


            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            _currentClient.GenderCode = RBtnMal.IsChecked == true ? "м" : "ж";

            if (_currentClient.ID == 0)
            {
                _currentClient.ID = Convert.ToInt32(TBID.Text);
                _currentClient.LastName = TBLastName.Text;
                _currentClient.FirstName = TBLastName.Text;
                _currentClient.Patronymic = TBPatron.Text;
                _currentClient.Email = TBEmail.Text;
                _currentClient.Phone = TBNumber.Text;
                DateTime var = DateTime.Now;
                DateTime.TryParse(TBBirthday.Text, out var);
                _currentClient.Birthday = var;
                _currentClient.RegistrationDate = DateTime.Now;
                if (RBtnFem.IsChecked == true)
                {
                    _currentClient.GenderCode = "ж";
                }
                else
                {
                    _currentClient.GenderCode = "м";
                }
                Baibakov_languageEntities.GetContext().Client.Add(_currentClient);
            }
            try
            {
                Baibakov_languageEntities.GetContext().SaveChanges();
                MessageBox.Show("Информация сохранена");
                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

        }

        private string GetProjectRootDirectory()
        {
            // Путь к исполняемому файлу (bin/Debug)
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            // Поднимаемся на 3 уровня: bin/Debug → bin → Корень проекта
            return System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(exePath)));
        }

        private void BtnEditPhoto_Click(object sender, RoutedEventArgs e)
        {
            // Путь к корню проекта (без имени проекта)
            string projectDirectory = GetProjectRootDirectory();
            string clientsFolderPath = System.IO.Path.Combine(projectDirectory, "Resources\\Clients");
            // Создаем папку, если её нет
            if (!Directory.Exists(clientsFolderPath))
            {
                Directory.CreateDirectory(clientsFolderPath);
            }
            OpenFileDialog myOpenFileDialog = new OpenFileDialog
            {
                InitialDirectory = clientsFolderPath
            };
            if (myOpenFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = myOpenFileDialog.FileName;
                // Сохраняем относительный путь ОТНОСИТЕЛЬНО КОРНЯ ПРОЕКТА
                _currentClient.PhotoPath = System.IO.Path.Combine("Resources\\Clients", System.IO.Path.GetFileName(selectedFilePath)).Split('\\').Reverse().First();
                // Загружаем изображение по полному пути
                Photo.Source = new BitmapImage(new Uri(selectedFilePath));
            }
        }
    }
}
