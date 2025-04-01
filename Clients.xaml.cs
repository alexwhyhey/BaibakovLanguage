using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Логика взаимодействия для Clients.xaml
    /// </summary>
    public partial class Clients : Page
    {
        int CountRecords;
        int CountPage;
        int CurrentPage = 0;

        List<Client> CurrentPageList = new List<Client>();
        List<Client> TableList;

        public Clients()
        {
            InitializeComponent();

            colvoCB.SelectedIndex = 0;
            SexCB.SelectedIndex = 0;
            SortCB.SelectedIndex = 0;
            ClientsView.ItemsSource = Baibakov_languageEntities.GetContext().Client.ToList();
            UpdateClients();
        }

        private void ChangePage(int direction, int? selectedPage, int colvo)
        {
            CurrentPageList.Clear();
            CountRecords = TableList.Count;

            if (CountRecords % colvo > 0)
            {
                CountPage = CountRecords / colvo + 1;
            }
            else
            {
                CountPage = CountRecords / colvo;
            }

            Boolean ifUpdate = true;

            int min;

            if (selectedPage.HasValue)
            {
                if (selectedPage >= 0 && selectedPage <= CountPage)
                {
                    CurrentPage = (int)selectedPage;
                    min = CurrentPage * colvo + colvo < CountRecords ? CurrentPage * colvo + colvo : CountRecords;
                    for (int i = CurrentPage * colvo; i < min; i++)
                    {
                        CurrentPageList.Add(TableList[i]);
                    }
                }
            }
            else
            {
                switch (direction)
                {
                    case 1:
                        if (CurrentPage > 0)
                        {
                            CurrentPage--;
                            min = CurrentPage * colvo + colvo < CountRecords ? CurrentPage * colvo + colvo : CountRecords;
                            for (int i = CurrentPage * colvo; i < min; i++)
                            {
                                CurrentPageList.Add(TableList[i]);
                            }
                        }
                        else
                        {
                            ifUpdate = false;
                        }
                        break;
                    case 2:
                        if (CurrentPage < CountPage - 1)
                        {
                            CurrentPage++;
                            min = CurrentPage * colvo + colvo < CountRecords ? CurrentPage * colvo + colvo : CountRecords;
                            for (int i = CurrentPage * colvo; i < min; i++)
                            {
                                CurrentPageList.Add(TableList[i]);
                            }
                        }
                        else
                        {
                            ifUpdate = false;
                        }
                        break;
                }
            }

            if (ifUpdate)
            {
                PageListBox.Items.Clear();

                for (int i = 1; i <= CountPage; i++)
                {
                    PageListBox.Items.Add(i);
                }
                PageListBox.SelectedIndex = CurrentPage;

                ClientsView.ItemsSource = CurrentPageList;
                ClientsView.Items.Refresh();

                min = CurrentPage * colvo + 10 < CountRecords ? CurrentPage * colvo + colvo : CountRecords;

            }
        }

        private int selectedCountPage()
        {
            switch (colvoCB.SelectedIndex)
            {
                case 0: return 10;
                case 1: return 50;
                case 2: return 200;
                default:
                    return Baibakov_languageEntities.GetContext().Client.Count();

            }
        }

        private void UpdateClients()
        {
            var currentClients = Baibakov_languageEntities.GetContext().Client.ToList();

            switch (SexCB.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    currentClients = currentClients.Where(p => p.GenderCode == "ж").ToList();
                    break;
                case 2:
                    currentClients = currentClients.Where(p => p.GenderCode == "м").ToList();
                    break;
            }

            switch (SortCB.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    currentClients = currentClients.OrderBy(p => p.LastName).ToList();
                    break;
                case 2:
                    currentClients = currentClients.OrderBy(p => p.lastJoin).ToList();
                    break;
                case 3:
                    currentClients = currentClients.OrderByDescending(p => p.countOfJoins).ToList();
                    break;
            }

            if (!string.IsNullOrWhiteSpace(SearcTB.Text))
            {
                currentClients = currentClients.Where(p =>
                p.ClientNamesMerge.ToLower().Contains(SearcTB.Text.ToLower()) ||
                p.Email.ToLower().Contains(SearcTB.Text.ToLower()) ||
                p.Phone.Replace("-", "").Replace("(", "").Replace(")", "").ToLower().Contains(SearcTB.Text.ToLower())
                ).ToList();
            }

            TableList = currentClients;

            ClientsView.ItemsSource = currentClients;

            TBCount.Text = currentClients.Count().ToString();
            TBAllRecords.Text = " из " + Baibakov_languageEntities.GetContext().Client.Count().ToString();

            ChangePage(0, 0, selectedCountPage());
        }

        private void ClientDeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            Client client = (sender as Button).DataContext as Client;

            if (client.countOfJoins != 0)
            {
                MessageBox.Show("У клиента есть посещения, удаление невозможно!");
                return;
            }

            if (client.countOfJoins == 0)
            {
                if (MessageBox.Show("Удалить клиента?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        Baibakov_languageEntities.GetContext().Client.Remove(client);
                        Baibakov_languageEntities.GetContext().SaveChanges();

                        ClientsView.ItemsSource = Baibakov_languageEntities.GetContext().Client.ToList();
                        UpdateClients();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Непредвиденная ошибка. Обратитесь к адинистратору.\nПодробнее: " + ex.ToString());
                    }
                } else
                {
                    MessageBox.Show("Удаление клиента отменено.");
                }
            }
        }

        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(1, null, selectedCountPage());
        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(2, null, selectedCountPage());
        }

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ChangePage(0, Convert.ToInt32(PageListBox.SelectedItem.ToString()) - 1, selectedCountPage());
        }

        private void colvoCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClients();
        }

        private void SortCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClients();
        }

        private void SexCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClients();
        }

        private void SearcTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateClients();
        }
    }
}
