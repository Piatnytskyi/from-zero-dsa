using Microsoft.Win32;
using DSAApplication.Commands;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DSAApplication.ViewModel
{
    class DSAApplicationViewModel : AbstractViewModel
    {
        private readonly RSACryptoServiceProvider _rsaCryptoServiceProvider;

        //TODO: Use some kind of settings.
        private const int _rsaEncryptionBlockSize = 64;

        private string _rc5PasswordInput = string.Empty;

        public string RC5PasswordInput
        {
            get => _rc5PasswordInput;
            set
            {
                if (_rc5PasswordInput.Equals(value))
                {
                    return;
                }
                _rc5PasswordInput = value;
                RaisePropertyChanged(nameof(RC5PasswordInput));
            }
        }

        private string _filenameInput = string.Empty;

        public string FilenameInput
        {
            get => _filenameInput;
            set
            {
                if (_filenameInput.Equals(value))
                {
                    return;
                }
                _filenameInput = value;
                RaisePropertyChanged(nameof(FilenameInput));
            }
        }
        
        //TODO: Make this and next property into state pattern.
        private string _status = string.Empty;

        public string Status
        {
            get => _status;
            set
            {
                if (_status.Equals(value))
                {
                    return;
                }
                _status = value;
                RaisePropertyChanged(nameof(Status));
            }
        }

        private string _output = string.Empty;

        public string Output
        {
            get => _output;
            set
            {
                if (_output.Equals(value))
                {
                    return;
                }
                _output = value;
                RaisePropertyChanged(nameof(Output));
            }
        }

        private bool _isInProgress;

        public bool IsInProgress
        {
            get => _isInProgress;
            set
            {
                if (_isInProgress.Equals(value))
                {
                    return;
                }
                _isInProgress = value;
                RaisePropertyChanged(nameof(IsInProgress));
            }
        }

        public RelayCommand ChooseFileCommand { get; set; }
        public RelayCommand ImportRSAKeysCommand { get; set; }
        public RelayCommand ExportRSAKeysCommand { get; set; }
        public AsyncCommand RSAEncryptFileCommand { get; set; }
        public AsyncCommand RSADencryptFileCommand { get; set; }
        public AsyncCommand RC5EncryptFileCommand { get; set; }
        public AsyncCommand RC5DencryptFileCommand { get; set; }

        public DSAApplicationViewModel()
        {
            //TODO: Create decorators for state change.
            //TODO: Make encrypt/decrypt and RSA keys related commands accept filenames.
            //TODO: Make proper exception handling.
            ChooseFileCommand = new RelayCommand(o => ChooseFile(), c => CanChooseFile());
            ImportRSAKeysCommand = new RelayCommand(o => ImportRSAKeys(), c => CanImportRSAKeys());
            ExportRSAKeysCommand = new RelayCommand(o => ExportRSAKeys(), c => CanExportRSAKeys());
            RSAEncryptFileCommand = new AsyncCommand(o => RSAEncryptFile(), c => CanRSAEncryptFile());
            RSADencryptFileCommand = new AsyncCommand(o => RSADecryptFile(), c => CanRSADecryptFile());
            RC5EncryptFileCommand = new AsyncCommand(o => RC5EncryptFile(), c => CanRC5EncryptFile());
            RC5DencryptFileCommand = new AsyncCommand(o => RC5DecryptFile(), c => CanRC5DecryptFile());

            _rsaCryptoServiceProvider = new RSACryptoServiceProvider();
        }

        private bool CanChooseFile()
        {
            return !IsInProgress;
        }

        private void ChooseFile()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Choose File...";
            openFileDialog.Filter = "All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                FilenameInput = openFileDialog.FileName;

                Status = "Chosen file:";
                Output = FilenameInput;
            }
        }

        private bool CanImportRSAKeys()
        {
            return !IsInProgress;
        }

        private void ImportRSAKeys()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Choose File...";
            openFileDialog.Filter = "All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _rsaCryptoServiceProvider.FromXmlString(File.ReadAllText(openFileDialog.FileName));

                    Status = "RSA Keys Imported:";
                    Output = openFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanExportRSAKeys()
        {
            return !IsInProgress;
        }

        private void ExportRSAKeys()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save File...";
            saveFileDialog.FileName = "RSAKeys.xml";
            saveFileDialog.Filter = "XML files (*.xml)|*.xml";

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(
                    saveFileDialog.FileName,
                    _rsaCryptoServiceProvider.ToXmlString(includePrivateParameters: true));

                Status = "RSA Keys Exported:";
                Output = saveFileDialog.FileName;
            }
        }

        private bool CanRSAEncryptFile()
        {
            return !(IsInProgress
                || !File.Exists(FilenameInput)
                || string.IsNullOrEmpty(FilenameInput));
        }

        private async Task RSAEncryptFile()
        {
            
        }

        private bool CanRSADecryptFile()
        {
            return !(IsInProgress
                || !File.Exists(FilenameInput)
                || string.IsNullOrEmpty(FilenameInput));
        }

        private async Task RSADecryptFile()
        {
            
        }

        private bool CanRC5DecryptFile()
        {
            return !(IsInProgress
                || !File.Exists(FilenameInput)
                || string.IsNullOrEmpty(FilenameInput));
        }

        private async Task RC5DecryptFile()
        {
            
        }

        private bool CanRC5EncryptFile()
        {
            return !(IsInProgress
                || !File.Exists(FilenameInput)
                || string.IsNullOrEmpty(FilenameInput)
                || string.IsNullOrEmpty(RC5PasswordInput));
        }

        private async Task RC5EncryptFile()
        {
            
        }
    }
}
