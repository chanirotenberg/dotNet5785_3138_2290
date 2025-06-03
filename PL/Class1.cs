//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Windows;
//using System.Windows.Controls;

//namespace PL.Volunteer
//{
//    public partial class VolunteerListWindow : Window
//    {
//        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
//        public BO.VolunteerInList? SelectedVolunteer { get; set; }


//        public VolunteerListWindow()
//        {
//            InitializeComponent();
//            SortFields = Enum.GetValues(typeof(BO.VolunteerSortField)).Cast<BO.VolunteerSortField>().ToList();
//            RefreshVolunteerList();
//            s_bl?.Volunteer.AddObserver(RefreshVolunteerList);
//        }

//        private void RefreshVolunteerList()
//        {
//            VolunteerList = s_bl.Volunteer.GetVolunteersList(
//                isActive: null,
//                sortBy: SelectedSortField == BO.VolunteerSortField.None ? null : SelectedSortField,
//                filterField: SelectedCallType == BO.CallType.None ? null : SelectedCallType);
//        }


//        public IEnumerable<BO.VolunteerInList> VolunteerList
//        {
//            get => (IEnumerable<BO.VolunteerInList>)GetValue(VolunteerListProperty);
//            set => SetValue(VolunteerListProperty, value);
//        }

//        public static readonly DependencyProperty VolunteerListProperty =
//            DependencyProperty.Register("VolunteerList", typeof(IEnumerable<BO.VolunteerInList>), typeof(VolunteerListWindow), new PropertyMetadata(null));

//        public List<BO.VolunteerSortField> SortFields { get; set; }

//        private BO.VolunteerSortField _selectedSortField = BO.VolunteerSortField.None;
//        public BO.VolunteerSortField SelectedSortField
//        {
//            get => _selectedSortField;
//            set
//            {
//                if (_selectedSortField != value)
//                {
//                    _selectedSortField = value;
//                    RefreshVolunteerList();
//                }
//            }
//        }
//        public IEnumerable<BO.CallType> CallTypeList { get; } = Enum.GetValues(typeof(BO.CallType)).Cast<BO.CallType>().ToList();

//        private BO.CallType? _selectedCallType = null;
//        public BO.CallType? SelectedCallType
//        {
//            get => _selectedCallType;
//            set
//            {
//                if (_selectedCallType != value)
//                {
//                    _selectedCallType = value;
//                    RefreshVolunteerList();
//                }
//            }
//        }


//        private void SortByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
//        {
//            var selectedItem = (ComboBoxItem)((ComboBox)sender).SelectedItem;
//            var content = selectedItem.Content.ToString();
//            SelectedSortField = content switch
//            {
//                "Name" => BO.VolunteerSortField.Name,
//                "Total Responses Handled" => BO.VolunteerSortField.TotalResponsesHandled,
//                "Total Responses Cancelled" => BO.VolunteerSortField.TotalResponsesCancelled,
//                "Total Expired Responses" => BO.VolunteerSortField.TotalExpiredResponses,
//                "Sum of Calls" => BO.VolunteerSortField.SumOfCalls,
//                "Sum of Cancellation" => BO.VolunteerSortField.SumOfCancellation,
//                "Sum of Expired Calls" => BO.VolunteerSortField.SumOfExpiredCalls,
//                _ => BO.VolunteerSortField.None,
//            };
//        }
//        private void DeleteButton_Click(object sender, RoutedEventArgs e)
//        {
//            if (sender is Button button && button.DataContext is BO.VolunteerInList volunteer)
//            {
//                var result = MessageBox.Show(
//                    $"Are you sure you want to delete volunteer '{volunteer.Name}' (ID: {volunteer.Id})?",
//                    "Confirm Deletion",
//                    MessageBoxButton.YesNo,
//                    MessageBoxImage.Warning);

//                if (result == MessageBoxResult.Yes)
//                {
//                    try
//                    {
//                        s_bl.Volunteer.DeleteVolunteer(volunteer.Id);

//                    }
//                    catch (BO.BlDeletionException ex)
//                    {
//                        MessageBox.Show($"Cannot delete volunteer:\n{ex.Message}",
//                            "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Error);
//                    }
//                    catch (Exception ex)
//                    {
//                        MessageBox.Show($"Unexpected error:\n{ex.Message}",
//                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//                    }
//                }
//            }
//        }

//        private void Window_Loaded(object sender, RoutedEventArgs e) =>
//            s_bl?.Volunteer.AddObserver(RefreshVolunteerList);

//        private void Window_Closed(object sender, EventArgs e) =>
//            s_bl?.Volunteer.RemoveObserver(RefreshVolunteerList);

//        private void btnAdd_Click(object sender, RoutedEventArgs e)
//        {
//            new VolunteerWindow().Show();
//        }

//        private void lsvVolunteerList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
//        {
//            if (SelectedVolunteer != null)
//                new VolunteerWindow(SelectedVolunteer.Id).Show();

//        }
//    }
//}