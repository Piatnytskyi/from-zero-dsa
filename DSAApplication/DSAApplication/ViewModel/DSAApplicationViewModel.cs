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
        private readonly DSACryptoServiceProvider _dsaCryptoServiceProvider;
        private readonly SHA1 _sha1;
        private byte[] _currentSignature = new byte[40];

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
        public RelayCommand VerifyTextCommand { get; set; }
        public AsyncCommand SignFileCommand { get; set; }
        public AsyncCommand VefiryFileCommand { get; set; }

        public DSAApplicationViewModel()
        {
            ChooseFileCommand = new RelayCommand(o => ChooseFile(), c => CanChooseFile());
            ImportSignatureCommand = new RelayCommand(o => ImportSignature(), c => CanImportSignature());
            ExportSignatureCommand = new RelayCommand(o => ExportSignature(), c => CanExportSignature());
            SignTextCommand = new RelayCommand(o => SignText(), c => CanSignText());
            VerifyTextCommand = new RelayCommand(o => VerifyText(), c => CanVerifyText());
            SignFileCommand = new AsyncCommand(o => SignFile(), c => CanSignFile());
            VefiryFileCommand = new AsyncCommand(o => VefiryFile(), c => CanVefiryFile());

            DSACryptoServiceProvider.UseMachineKeyStore = true;
            _dsaCryptoServiceProvider = new DSACryptoServiceProvider();
            _sha1 = SHA1.Create();
        }

        private bool CanVerifyText()
        {
            return !(IsInProgress
                || string.IsNullOrEmpty(TextInput)
                || string.IsNullOrEmpty(Signature));
        }

        private void VerifyText()
        {
            IsInProgress = true;
            Status = "Verifing...:";

            try
            {
                Output = _dsaCryptoServiceProvider.VerifySignature(
                        _sha1.ComputeHash(Encoding.ASCII.GetBytes(TextInput)),
                        _currentSignature)
                    ? "Verified."
                    : "Not verified.";

                Status = "Verification result:";
                IsInProgress = false;
            }
            catch (Exception ex)
            {
                Status = "Verification failed!";
                IsInProgress = false;
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanVefiryFile()
        {
            return !(IsInProgress
                || string.IsNullOrEmpty(FilenameInput)
                || string.IsNullOrEmpty(Signature));
        }

        private async Task VefiryFile()
        {
            await Task.Run(() =>
            {
                IsInProgress = true;
                Status = "Verifing...:";

                try
                {
                    using (var fileToVerify = File.OpenRead(FilenameInput))
                        Output = _dsaCryptoServiceProvider.VerifySignature(_sha1.ComputeHash(fileToVerify), _currentSignature)
                            ? "Verified."
                            : "Not verified.";

                    Status = "Verification result:";
                    IsInProgress = false;
                }
                catch (Exception ex)
                {
                    Status = "Verification failed!";
                    IsInProgress = false;
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private bool CanSignFile()
        {
            return !(IsInProgress
                 || string.IsNullOrEmpty(FilenameInput));
        }

        private async Task SignFile()
        {
            await Task.Run(() =>
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Save File...";
                saveFileDialog.FileName = Path.GetFileName(FilenameInput + ".bin");
                saveFileDialog.Filter = "BIN files (*.bin)|*.bin";

                if (saveFileDialog.ShowDialog() == false)
                    return;

                IsInProgress = true;
                Status = "Signing...:";

                var temporaryFileName = FilenameInput == saveFileDialog.FileName
                    ? saveFileDialog.FileName + ".signature"
                    : saveFileDialog.FileName;

                try
                {
                    using (var fileToSign = File.OpenRead(FilenameInput))
                    using (var signatureFile = File.OpenWrite(temporaryFileName))
                    {
                        _currentSignature = _dsaCryptoServiceProvider.CreateSignature(
                            _sha1.ComputeHash(fileToSign));
                        signatureFile.Write(_currentSignature, 0, _currentSignature.Length);
                        Signature = Convert.ToBase64String(_currentSignature);
                    }

                    Status = "File was signied!";
                    IsInProgress = false;
                }
                catch (Exception ex)
                {
                    Status = "Signing failed!";
                    IsInProgress = false;
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    if (temporaryFileName != saveFileDialog.FileName)
                        File.Move(temporaryFileName, saveFileDialog.FileName, true);
                }
            });
        }

        private bool CanSignText()
        {
            return !(IsInProgress
                || string.IsNullOrEmpty(TextInput));
        }

        private void SignText()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save File...";
            saveFileDialog.FileName = Path.GetFileName("signature.bin");
            saveFileDialog.Filter = "BIN files (*.bin)|*.bin";

            if (saveFileDialog.ShowDialog() == false)
                return;

            IsInProgress = true;
            Status = "Signing...:";

            var temporaryFileName = FilenameInput == saveFileDialog.FileName
                    ? saveFileDialog.FileName + ".signature"
                    : saveFileDialog.FileName;

            try
            {
                using (var signatureFile = File.OpenWrite(temporaryFileName))
                {
                    _currentSignature = _dsaCryptoServiceProvider.CreateSignature(
                        _sha1.ComputeHash(Encoding.ASCII.GetBytes(TextInput)));
                    signatureFile.Write(_currentSignature, 0, _currentSignature.Length);
                    Signature = Convert.ToBase64String(_currentSignature);
                    Output = Signature;
                }

                Status = "Text was signied!";
                IsInProgress = false;
            }
            catch (Exception ex)
            {
                Status = "Signing failed!";
                IsInProgress = false;
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (temporaryFileName != saveFileDialog.FileName)
                    File.Move(temporaryFileName, saveFileDialog.FileName, true);
            }
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
                    using (BinaryReader reader = new BinaryReader(new FileStream(openFileDialog.FileName, FileMode.Open)))
                        reader.Read(_currentSignature, 0, 40);

                    Signature = Convert.ToBase64String(_currentSignature);

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
                File.WriteAllBytes(saveFileDialog.FileName, _currentSignature);

                Status = "Signature exported:";
                Output = saveFileDialog.FileName;
            }
        }
    }
}
