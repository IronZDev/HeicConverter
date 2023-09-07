using HeicConverter.Data;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.WebUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HeicConverter
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ObservableCollection<FileListElement> files = new ObservableCollection<FileListElement>();
        public MainPage()
        {
            this.InitializeComponent();
        }

        private string CapitalizeFirstLetter(string s)
        {
            if (s.Length == 0)
                return s;
            else if (s.Length == 1)
                return s.ToUpper();
            else
                return char.ToUpper(s[0]) + s.Substring(1);
        }

        private async void ConvertBtn_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker()
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".heic");
            picker.FileTypeFilter.Add(".heif");

            StorageFile sampleFile = await picker.PickSingleFileAsync();
            using (var inputStream = await sampleFile.OpenSequentialReadAsync())
            {
                var readStream = inputStream.AsStreamForRead();

                var byteArray = new byte[readStream.Length];
                await readStream.ReadAsync(byteArray, 0, byteArray.Length);
                using (var imageFromStream = new MagickImage(byteArray))
                {
                    Debug.WriteLine(imageFromStream.FormatInfo);
                    var savePicker = new Windows.Storage.Pickers.FileSavePicker
                    {
                        SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary,
                        SuggestedFileName = sampleFile.DisplayName
                    };
                    // Dropdown of file types the user can save the file as
                    savePicker.FileTypeChoices.Add("Joint Photographic Experts Group JFIF format", new List<string>() { ".jpg", ".jpeg" });
                    savePicker.FileTypeChoices.Add("Portable Network Graphics", new List<string>() { ".png" });
                    savePicker.FileTypeChoices.Add("CompuServe Graphics Interchange Format", new List<string>() { ".gif" });
                    savePicker.FileTypeChoices.Add("Tagged image file multispectral format", new List<string>() { ".tiff" });
                    savePicker.FileTypeChoices.Add("Microsoft Windows bitmap", new List<string>() { ".bmp" });
                    savePicker.FileTypeChoices.Add("Portable Document Format", new List<string>() { ".pdf" });
                    savePicker.FileTypeChoices.Add("Scalable Vector Graphics", new List<string>() { ".svg" });
                    savePicker.FileTypeChoices.Add("Weppy image format", new List<string>() { ".webp" });


                    // Default file name if the user does not type one in or select a file to replace
                    StorageFile targetFile = await savePicker.PickSaveFileAsync();
                    if (targetFile != null)
                    {
                        string chosenExtension = targetFile.FileType.TrimStart('.');
                        string formattedExtension = CapitalizeFirstLetter(chosenExtension);
                        imageFromStream.Format = (MagickFormat)Enum.Parse(typeof(MagickFormat), formattedExtension);
                        // Prevent updates to the remote version of the file until
                        // we finish making changes and call CompleteUpdatesAsync.
                        CachedFileManager.DeferUpdates(targetFile);
                        // write to file
                        await FileIO.WriteBytesAsync(targetFile, imageFromStream.ToByteArray());

                        //imageFromStream.Write(targetFile.Path);
                        // Let Windows know that we're finished changing the file so
                        // the other app can update the remote version of the file.
                        // Completing updates may require Windows to ask for user input.
                        Windows.Storage.Provider.FileUpdateStatus status =
                            await CachedFileManager.CompleteUpdatesAsync(targetFile);
                        if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                        {
                            Debug.WriteLine("File " + targetFile.Name + " was saved.");
                        }
                        else
                        {
                            Debug.WriteLine("File " + targetFile.Name + " couldn't be saved.");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Operation cancelled.");

                    }
                }
            }
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            Grid_DragLeave(sender, e);
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                AddCollectionToFiles(items);
/*                if (items.Any())
                {
                   StorageFolder folder = ApplicationData.Current.LocalFolder;
                    if (contentType == "image/jpg" || contentType == "image/png" || contentType == "image/jpeg")
                    {
                        StorageFile newFile = await storageFile.CopyAsync(folder, storageFile.Name, NameCollisionOption.GenerateUniqueName);
                        var bitmapImg = new BitmapImage();
                        bitmapImg.SetSource(await storageFile.OpenAsync(FileAccessMode.Read));
                        imgMain.Source = bitmapImg;
                    }
                }*/
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
            files?.Remove(i);
        }

        private void ClearAllBtn_Click(object sender, RoutedEventArgs e)
        {
            files.Clear();
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
            AddCollectionToFiles(filesToAdd);
        }

        private void AddCollectionToFiles(IEnumerable<IStorageItem> items)
        {
            if (!items.Any()) return;
            foreach (var item in items)
            {
                if (item is StorageFile)
                {
                    StorageFile file = (StorageFile)item;
                    bool isValid = file.Name.ToLower().EndsWith("heic") || file.Name.ToLower().EndsWith("heif");
                    if (!files.Any(x => x.Path == file.Path))
                    {
                        files.Add(new FileListElement(file.Name, file.Path, isValid ? FileStatus.PENDING : FileStatus.INVALID));
                    }
                }
            }
        }
    }
}
