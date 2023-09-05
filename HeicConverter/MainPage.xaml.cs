using ImageMagick;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HeicConverter
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
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
                    var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                    savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
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
                    savePicker.SuggestedFileName = sampleFile.DisplayName;
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
    }
}
