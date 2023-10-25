using HeicConverter.Data;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HeicConverter
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string SAVE_FOLDER_ACCESS_TOKEN = "SAVE_FOLDER_ACCESS_TOKEN";
        private const int CHUNK_SIZE = 5; // We don't want to convert big number of files at once TODO: Improve so we do not have to wait for the entire chunk to finish before we start a new one
        private const bool OPTIMIZE_IMG = true;
        private MainPageViewModel ViewModel;
        public MainPage()
        {
            this.InitializeComponent();
            ViewModel = new MainPageViewModel();
        }

        private async void ConvertBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.IsConversionInProgress = true;
            ViewModel.ConvertedFilesCounter = 0;
            var saveFolderPicker = new Windows.Storage.Pickers.FolderPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };
            saveFolderPicker.FileTypeFilter.Add("*");

            var _fileAccess = await saveFolderPicker.PickSingleFolderAsync();
            if (_fileAccess == null)
            {
                ViewModel.IsConversionInProgress = false;
                return;
            }

            try
            {
                Utils.RememberStorageItem(_fileAccess, SAVE_FOLDER_ACCESS_TOKEN);
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(SAVE_FOLDER_ACCESS_TOKEN, _fileAccess);
            } catch (Exception ex)
            {
                if (ex is SystemException)
                {
                    Utils.ClearFutureAccessList();
                }
            }

            Task.Run(() => processAllFiles());
        }

        private async Task processAllFiles()
        {
            List<Task> tasks = new List<Task>();
            var chunks = Utils.Chunk(ViewModel.files, CHUNK_SIZE);

            foreach (var chunk in chunks)
            {
                foreach (FileListElement file in chunk)
                {
                    if (file.Status != FileStatus.INVALID && file.Status != FileStatus.COMPLETED)
                    {
                        tasks.Add(ProcessFile(file));
                    }
                }
                await Task.WhenAll(tasks);
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ViewModel.IsConversionInProgress = false;
            });
        }

        private async Task ProcessFile(FileListElement fileElm)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                fileElm.Status = FileStatus.IN_PROGRESS;
            });
            MagickImage img = null;
            try
            {
                img = await ReadFile(fileElm);
                ConvertFile(img);
                bool result = await SaveFile(img, Path.GetFileNameWithoutExtension(fileElm.Path));
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    fileElm.Status = result ? FileStatus.COMPLETED : FileStatus.ERROR;
                    fileElm.TooltipMsg = "";
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}");
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    fileElm.Status = FileStatus.ERROR;
                    fileElm.TooltipMsg = ex.Message;
                });
            }
            finally
            {
                if (img != null)
                {
                    img.Dispose();
                }
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ViewModel.ConvertedFilesCounter++;
                });
            }
        }

        private async Task<MagickImage> ReadFile(FileListElement fileElm)
        {

            StorageFile file = await Utils.GetFileForToken(fileElm.Token);
            using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                return new MagickImage(fileStream.AsStream());
            }
        }

        private void ConvertFile(MagickImage img)
        {
            img.Format = (MagickFormat)Enum.Parse(typeof(MagickFormat), ViewModel.SelectedItem.Extension);
        }

        private async Task<bool> SaveFile(MagickImage img, string fileName)
        {
            StorageFolder targetFolder = await Utils.GetFolderForToken(SAVE_FOLDER_ACCESS_TOKEN);
            StorageFile targetFile = await targetFolder.CreateFileAsync($"{fileName}.{ViewModel.SelectedItem.Extension.ToLower()}", CreationCollisionOption.GenerateUniqueName);
            await FileIO.WriteBytesAsync(targetFile, img.ToByteArray());
            Windows.Storage.Provider.FileUpdateStatus status =
                await CachedFileManager.CompleteUpdatesAsync(targetFile);
            if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
            {
                Debug.WriteLine("File " + targetFile.Name + " was saved.");
                await OptimizeImage(targetFile);
                return true;
            }
            else
            {
                Debug.WriteLine("File " + targetFile.Name + " couldn't be saved.");
                return false;
            }
        }

        private async Task OptimizeImage(StorageFile img)
        {
            if (img == null) return;
            if (!OPTIMIZE_IMG) return;

            var optimizer = new ImageOptimizer
            {
                IgnoreUnsupportedFormats = true,
                OptimalCompression = ViewModel.IsAdvancedOptimizationEnabled,
            };
            using (IRandomAccessStream fileStream = await img.OpenAsync(FileAccessMode.ReadWrite))
            {
                if (ViewModel.IsLosslessConvertionEnabled)
                {
                    optimizer.LosslessCompress(fileStream.AsStream());
                } else
                {
                    optimizer.Compress(fileStream.AsStream());
                }
            }
        }


        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            Grid_DragLeave(sender, e);
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var itemsToAdd = await e.DataView.GetStorageItemsAsync();
                AddFilesToList(itemsToAdd);
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "Drop here to add to the list";
            e.DragUIOverride.IsGlyphVisible = true;
            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsCaptionVisible = true;
        }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            MainApp_Grid.Visibility = Visibility.Collapsed;
            DragDrop_Grid.Visibility = Visibility.Visible;
        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            MainApp_Grid.Visibility = Visibility.Visible;
            DragDrop_Grid.Visibility = Visibility.Collapsed;
        }

        private void RemoveFileBtn_Click(object sender, RoutedEventArgs e)
        {
            FileListElement i = (FileListElement)((FrameworkElement)sender).DataContext;
            Utils.ForgetFileToken(i.Token);
            ViewModel.files?.Remove(i);
            // Fix for wrong icons bug (Remove all images and add a new one)
            if (ViewModel.files?.Count == 0)
            {
                ViewModel.files.Clear();
            }
            Debug.WriteLine("Removed");
        }

        private void ClearAllBtn_Click(object sender, RoutedEventArgs e)
        {
            Utils.ClearFutureAccessList();
            ViewModel.files.Clear();
        }

        private async void AddToListBtn_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker()
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".heic");
            picker.FileTypeFilter.Add(".heif");

            IReadOnlyList<StorageFile> filesToAdd = await picker.PickMultipleFilesAsync();
            AddFilesToList(filesToAdd);
        }

        private void AddFilesToList(IEnumerable<IStorageItem> filesToAdd)
        {
            ListLoadingOverlay.Visibility = Visibility.Visible;
            Task.Run(() => AddFilesToListAsync(filesToAdd, ViewModel.files));
        }

        private async Task AddFilesToListAsync(IEnumerable<IStorageItem> items, IEnumerable<FileListElement> currentFilesList)
        {
            if (!items.Any()) return;
            List<FileListElement> filesToAddList = new List<FileListElement>();
            List<string> notHandledFiles = new List<string>();

            long maxImagesNum = StorageApplicationPermissions.FutureAccessList.MaximumItemsAllowed - StorageApplicationPermissions.FutureAccessList.Entries.Count() - 1;
            if (items.Count() > maxImagesNum)
            {
                await ShowInfoDialog($"The number of images cannot exceed {StorageApplicationPermissions.FutureAccessList.MaximumItemsAllowed - 1}", "Max files number limit exceeded");
                return;
            }

            int restartCounter = 0;
            do
            {
                if (restartCounter > 1)
                {
                    await ShowInfoDialog("Could not parse the files! Try restarting the app.", "Fatal error");
                    return;
                }

                foreach (var item in items)
                {
                    try
                    {
                        if (item is StorageFile)
                        {
                            StorageFile file = (StorageFile)item;
                            string token = Utils.RememberStorageItem(file);
                            bool isValid = file.Name.ToLower().EndsWith("heic") || file.Name.ToLower().EndsWith("heif");
                            if (!currentFilesList.Any(x => x.Path == file.Path) && isValid)
                            {
                                filesToAddList.Add(new FileListElement(file.Name, file.Path, FileStatus.PENDING, token));
                            }
                            else if (!isValid)
                            {
                                notHandledFiles.Add(file.Name);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is SystemException)
                        {
                            Utils.ClearFutureAccessList();
                            restartCounter++;
                        }
                    }
                }
            } while (restartCounter > 0);

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => {
                ListLoadingOverlay.Visibility = Visibility.Collapsed;
                ViewModel.files.AddRange(filesToAddList);

                if (notHandledFiles.Any())
                {
                    await ShowUnhandledFilesDialog(notHandledFiles);
                }
            });
        }

        private void FormatOptionsCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in ViewModel.files)
            {
                item.Status = FileStatus.PENDING;
            }
            ViewModel.ConvertedFilesCounter = 0;
        }

        private async Task<ContentDialogResult> ShowUnhandledFilesDialog(List<string> notHandledFiles)
        {
            ContentDialog notHandledFilesDialog = new ContentDialog()
            {
                Title = "Unhandled file types!",
                PrimaryButtonText = "Ok",
                PrimaryButtonStyle = this.Resources["UnhandledFilesDialogButtonStyle"] as Style
            };

            StackPanel stackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Spacing = 10,
            };

            TextBlock dialogText = new TextBlock()
            {
                Text = "Only .heic, .heif images can be converted. The following files are not supported: "
            };

            ListView unhandledFilesList = new ListView()
            {
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromArgb(102, 255, 255, 255)),
                ItemTemplate = this.Resources["UnhandledFilesTemplate"] as DataTemplate,
                ItemsSource = notHandledFiles,
                MaxHeight = this.ActualHeight * 0.5,
                SelectionMode = ListViewSelectionMode.None
            };

            stackPanel.Children.Add(dialogText);
            stackPanel.Children.Add(unhandledFilesList);

            notHandledFilesDialog.Content = stackPanel;
            return await notHandledFilesDialog.ShowAsync();
        }

        private async Task<IUICommand> ShowInfoDialog(string text, string title)
        {
            MessageDialog fatalErrorDialog = new MessageDialog(text, title);
            return await fatalErrorDialog.ShowAsync();
        }
    }
}
