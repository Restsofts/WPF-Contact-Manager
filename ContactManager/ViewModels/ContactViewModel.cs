using ContactManager.Helpers;
using ContactManager.Models.ContactManager.Models;
using Microsoft.Win32; // For file dialogs
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ContactManager.ViewModels
{
    public class ContactViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Contact> _contacts;
        public ObservableCollection<Contact> Contacts
        {
            get { return _contacts; }
            set { _contacts = value; OnPropertyChanged(); }
        }

        private Contact _selectedContact;
        public Contact SelectedContact
        {
            get { return _selectedContact; }
            set { _selectedContact = value; OnPropertyChanged(); }
        }

        private ContactDbContext _dbContext;

        public ICommand AddContactCommand { get; }
        public ICommand UpdateContactCommand { get; }
        public ICommand DeleteContactCommand { get; }
        public ICommand ExportContactsCommand { get; }
        public ICommand ImportContactsCommand { get; }

        public ContactViewModel()
        {
            _dbContext = new ContactDbContext();
            Contacts = new ObservableCollection<Contact>(_dbContext.Contacts.ToList());
            SelectedContact = new Contact(); // Initialize SelectedContact

            AddContactCommand = new RelayCommand(AddContact);
            UpdateContactCommand = new RelayCommand(UpdateContact, CanExecuteUpdateOrDelete);
            DeleteContactCommand = new RelayCommand(DeleteContact, CanExecuteUpdateOrDelete);
            ExportContactsCommand = new RelayCommand(ExportContacts);
            ImportContactsCommand = new RelayCommand(ImportContacts);
        }

        private bool CanExecuteUpdateOrDelete(object parameter)
        {
            return SelectedContact != null;
        }

        private void AddContact(object parameter)
        {
            var newContact = new Contact
            {
                Name = SelectedContact.Name,
                Email = SelectedContact.Email,
                Phone = SelectedContact.Phone
            };

            _dbContext.Contacts.Add(newContact);
            _dbContext.SaveChanges();
            Contacts.Add(newContact);

            // Reset SelectedContact for the next entry
            SelectedContact = new Contact();
        }

        private void UpdateContact(object parameter)
        {
            if (SelectedContact == null) return;

            var contactToUpdate = _dbContext.Contacts.FirstOrDefault(c => c.Id == SelectedContact.Id);
            if (contactToUpdate != null)
            {
                contactToUpdate.Name = SelectedContact.Name;
                contactToUpdate.Email = SelectedContact.Email;
                contactToUpdate.Phone = SelectedContact.Phone;
                _dbContext.SaveChanges();
            }
        }

        private void DeleteContact(object parameter)
        {
            if (SelectedContact == null) return;

            _dbContext.Contacts.Remove(SelectedContact);
            _dbContext.SaveChanges();
            Contacts.Remove(SelectedContact);
        }

        // تصدير جهات الاتصال إلى ملف CSV
        private void ExportContacts(object parameter)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = "contacts.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (var writer = new StreamWriter(saveFileDialog.FileName))
                {
                    foreach (var contact in Contacts)
                    {
                        writer.WriteLine($"{contact.Name},{contact.Email},{contact.Phone}");
                    }
                }
            }
        }

        // استيراد جهات الاتصال من ملف CSV
        private void ImportContacts(object parameter)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                using (var reader = new StreamReader(openFileDialog.FileName))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');

                        var contact = new Contact
                        {
                            Name = values[0],
                            Email = values[1],
                            Phone = values[2]
                        };

                        _dbContext.Contacts.Add(contact);
                        _dbContext.SaveChanges();
                        Contacts.Add(contact);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}