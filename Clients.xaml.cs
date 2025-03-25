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

            ClientsView.ItemsSource = Baibakov_languageEntities.GetContext().Client.ToList();
            UpdateClients();
        }

        private void ChangePage(int direction, int? selectedPage)
        {
            CurrentPageList.Clear();
            CountRecords = TableList.Count;

            if (CountRecords % 10 > 0)
            {
                CountPage = CountRecords / 10 + 1;
            }
            else
            {
                CountPage = CountRecords / 10;
            }

            Boolean ifUpdate = true;

            int min;

            if (selectedPage.HasValue)
            {
                if (selectedPage >= 0 && selectedPage <= CountPage)
                {
                    CurrentPage = (int)selectedPage;
                    min = CurrentPage * 10 + 10 < CountRecords ? CurrentPage * 10 + 10 : CountRecords;
                    for (int i = CurrentPage * 10; i < min; i++)
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
                            min = CurrentPage * 10 + 10 < CountRecords ? CurrentPage * 10 + 10 : CountRecords;
                            for (int i = CurrentPage * 10; i < min; i++)
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
                            min = CurrentPage * 10 + 10 < CountRecords ? CurrentPage * 10 + 10 : CountRecords;
                            for (int i = CurrentPage * 10; i < min; i++)
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

                min = CurrentPage * 10 + 10 < CountRecords ? CurrentPage * 10 + 10 : CountRecords;
                TBCount.Text = min.ToString();
                TBAllRecords.Text = " из " + CountRecords.ToString();
            }
        }

        private void UpdateClients()
        {
            var currentServices = Baibakov_languageEntities.GetContext().Client.ToList();

            ClientsView.ItemsSource = currentServices;
            TableList = currentServices;

            ChangePage(0, 0);
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
            ChangePage(1, null);
        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(2, null);
        }

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ChangePage(0, Convert.ToInt32(PageListBox.SelectedItem.ToString()) - 1);
        }
    }
}
