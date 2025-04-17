using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using ShadowSession.Data;
using ShadowSession.Helpers;
using ShadowSession.Messages;
using ShadowSession.Views;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Documents;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace ShadowSession.ViewModels
{
    public class SettingsPageViewModel : ObservableRecipient
    {
        private string? _userSettingsSearchText;

        private UserSetting? _selectedUserSetting;

        public SettingsPageViewModel()
        {
            UserSettings = [];
            UserSettings.CollectionChanged += delegate
            {
                OnPropertyChanged(nameof(UserSettings));
            };

            RefreshUserSettingsCommand = new RelayCommand(RefreshUserSettings);

            SearchUserSettingsCommand = new RelayCommand(SearchUserSettings);

            ResetUserSettingValueCommand = new RelayCommand(ResetUserSettingValue);

            ModifyUserSettingValueCommand = new RelayCommand<string?>(ModifyUserSettingValue);

            ResetUserSettingsSearchTextCommand = new RelayCommand(() =>
            {
                UserSettingsSearchText = null;
                SearchUserSettings();
            });

            Messenger.Register<UserSettingsChangedMessage>(this, (recipient, message) =>
            {
                DispatchHelper.InvokeDispatch(RefreshUserSettings);
            });
        }

        public ObservableCollection<UserSetting> UserSettings { get; }

        public IRelayCommand RefreshUserSettingsCommand { get; }

        public IRelayCommand SearchUserSettingsCommand { get; }

        public IRelayCommand ResetUserSettingsSearchTextCommand { get; }

        public IRelayCommand ResetUserSettingValueCommand { get; }

        public IRelayCommand<string?> ModifyUserSettingValueCommand { get; }

        public UserSetting? SelectedUserSetting
        {
            get => _selectedUserSetting;
            set
            {
                if (_selectedUserSetting != value) 
                {
                    _selectedUserSetting = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? UserSettingsSearchText
        {
            get => _userSettingsSearchText;
            set
            {
                if (_userSettingsSearchText != value)
                {
                    _userSettingsSearchText = value;
                    OnPropertyChanged();
                }
            }
        }

        private void RefreshUserSettings()
        {
            UserSettingsSearchText = null;

            UserSettings.Clear();

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            foreach (var userSetting in context.UserSettings.Where(x => x.Visible).OrderBy(x => x.SortOrder))
            {
                UserSettings.Add(userSetting);
            }
        }

        private void SearchUserSettings()
        {
            UserSettings.Clear();

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons. REASON: Not a recognized LINQ to SQL method
            var userSettings = context.UserSettings.AsNoTracking()
                .Where(x => x.Visible)
                .OrderBy(x => x.SortOrder)
                .Where(x => string.IsNullOrWhiteSpace(UserSettingsSearchText) || 
                            x.DisplayName.ToLower().Contains(UserSettingsSearchText.Trim().ToLower()) ||
                            (x.Description != null && x.Description.ToLower().Contains(UserSettingsSearchText.Trim().ToLower())));
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

            foreach (var userSetting in userSettings)
            {
                UserSettings.Add(userSetting);
            }
        }

        private void ResetUserSettingValue()
        {
            if (SelectedUserSetting == null)
            {
                return;
            }

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            context.TryAttach(SelectedUserSetting);

            SelectedUserSetting.Value = SelectedUserSetting.DefaultValue;

            context.SaveChanges();

            Messenger.Send(new UserSettingsChangedMessage(this));
        }

        private void ModifyUserSettingValue(string? newValue)
        {
            if (SelectedUserSetting == null)
            {
                return;
            }

            if (SelectedUserSetting.ValueRequired && string.IsNullOrWhiteSpace(newValue))
            {
                NotificationHelper.Notify("User Setting Modification", "This user setting does not allow empty values.", NotificationSeverity.Error);
                return;
            }

            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            context.TryAttach(SelectedUserSetting);

            switch (SelectedUserSetting.Kind)
            {
                case UserSettingKind.Boolean:
                    SelectedUserSetting.Value = string.IsNullOrWhiteSpace(newValue) ? null : bool.Parse(newValue).ToString();
                    break;

                case UserSettingKind.String:
                    SelectedUserSetting.Value = newValue;
                    break;

                case UserSettingKind.Integer:
                    SelectedUserSetting.Value = string.IsNullOrWhiteSpace(newValue) ? null : int.Parse(newValue).ToString();
                    break;
                
                case UserSettingKind.Directory:
                    if (!string.IsNullOrWhiteSpace(newValue) && !DirectoryHelper.CanWriteInDirectory(newValue))
                    {
                        NotificationHelper.Notify("Directory Access", "The selected directory does not allow writing. Reverting.", NotificationSeverity.Error);
                        break;
                    }
                    SelectedUserSetting.Value = newValue;
                    break;
                
                default:
                    throw new IndexOutOfRangeException(nameof(SelectedUserSetting.Kind));
            }

            context.SaveChanges();
            Messenger.Send(new UserSettingsChangedMessage(this));
        }

    }
}
