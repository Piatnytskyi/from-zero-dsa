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
        private string _textInput = string.Empty;

        public string TextInput
        {
            get => _textInput;
            set
            {
                if (_textInput.Equals(value))
                {
                    return;
                }
                _textInput = value;
                RaisePropertyChanged(nameof(TextInput));
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

        private string _signature = string.Empty;

        public string Signature
        {
            get => _signature;
            set
            {
                if (_signature.Equals(value))
                {
                    return;
                }
                _signature = value;
                RaisePropertyChanged(nameof(Signature));
            }
        }

        public RelayCommand ChooseFileCommand { get; set; }
        public RelayCommand ImportSignatureCommand { get; set; }
        public RelayCommand ExportSignatureCommand { get; set; }
        public RelayCommand SignTextCommand { get; set; }
        public AsyncCommand SignFileCommand { get; set; }
        public AsyncCommand VefiryFileCommand { get; set; }

        public DSAApplicationViewModel()
        {
            ChooseFileCommand = new RelayCommand(o => ChooseFile(), c => CanChooseFile());
            ImportSignatureCommand = new RelayCommand(o => ImportSignature(), c => CanImportSignature());
            ExportSignatureCommand = new RelayCommand(o => ExportSignature(), c => CanExportSignature());
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

        private bool CanImportSignature()
        {
            return !IsInProgress;
        }

        private void ImportSignature()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Choose File...";
            openFileDialog.Filter = "All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    byte[] signature = new byte[20];
                    using (BinaryReader reader = new BinaryReader(new FileStream(openFileDialog.FileName, FileMode.Open)))
                        reader.Read(signature, 0, 20);

                    Signature = Encoding.ASCII.GetString(signature);

                    Status = "Signature imported:";
                    Output = openFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanExportSignature()
        {
            return !(IsInProgress
                || string.IsNullOrEmpty(Signature));
        }

        private void ExportSignature()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save File...";
            saveFileDialog.FileName = "Signature.bin";
            saveFileDialog.Filter = "BIN files (*.bin)|*.bin";

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllBytes(
                    saveFileDialog.FileName,
                    Encoding.ASCII.GetBytes(Signature));

                Status = "Signature exported:";
                Output = saveFileDialog.FileName;
            }
        }
    }
}
