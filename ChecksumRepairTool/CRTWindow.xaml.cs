using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using FFX.ChecksumUtility;
using Microsoft.Win32;

namespace ChecksumRepairTool
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class CRTWindow : Window
  {
    private string SaveFileLocation;
    private byte[] SaveFileContent;
    private byte[] Checksum;

    private readonly OpenFileDialog ofd = new OpenFileDialog()
    {
      Title = "Select a save file.."
    };

    public CRTWindow()
    {
      InitializeComponent();
    }

    private void BtnLoadFile_Click(object sender, RoutedEventArgs e)
    {
      Reset();
      if (ofd.ShowDialog() == true && OpenFile(ofd.FileName)) CheckFile();
    }

    private bool OpenFile(string path)
    {
      SaveFileLocation = path;
      if (File.Exists(path))
      {
        FileInfo fi = new FileInfo(SaveFileLocation);
        if (fi.Length == 26880 || fi.Length == 25848)
        {
          try
          {
            SaveFileContent = File.ReadAllBytes(SaveFileLocation);
            return true;
          }
          catch (Exception)
          {
            MessageBox.Show("Failed to read the save file", "Error");
            SaveFileContent = null;
            SaveFileLocation = null;
          }
        }
      }
      else
      {
        MessageBox.Show("File not found!", "Error", MessageBoxButton.OK);
        SaveFileContent = null;
        SaveFileLocation = null;
      }

      return false;
    }

    private void CheckFile()
    {
      lblFilePath.Content = SaveFileLocation.Contains("\\") ?
        Regex.Replace(SaveFileLocation, @".+\\([^\\]+)\\([^\\]+)$", ".../$1/$2") :
        SaveFileLocation.Replace("\\", "/");

      lblHeadActualChecksum.IsEnabled = lblHeadCurrentChecksum.IsEnabled = true;
      lblActualChecksum.Content = lblCurrentChecksum.Content = "Calculating..";

      byte[] cs = FFXChecksumUtility.GetCurrentChecksum(SaveFileContent);
      Checksum = FFXChecksumUtility.CRC16CCITT(SaveFileContent);

      lblCurrentChecksum.Content = String.Format("{0:X} {1:X}", cs[0], cs[1]);
      lblActualChecksum.Content = String.Format("{0:X} {1:X}", Checksum[0], Checksum[1]);

      if (cs[0] != Checksum[0] || cs[1] != Checksum[1])
      {
        lblCurrentChecksum.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x30, 0x30));
        btnFixChecksum.IsEnabled = true;
      }
    }

    private void BtnFixChecksum_Click(object sender, RoutedEventArgs e)
    {
      btnFixChecksum.IsEnabled = false;
      SaveFileContent.RepairSaveChecksum();

      try
      {
        File.WriteAllBytes(SaveFileLocation, SaveFileContent);
        lblSaved.Visibility = Visibility.Visible;
        lblCurrentChecksum.Foreground = SystemColors.ControlTextBrush;
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to save file..");

#if DEBUG
        Console.WriteLine(ex);
#endif
      }

      CheckFile();
    }

    private void Reset()
    {
      lblCurrentChecksum.Foreground = SystemColors.ControlTextBrush;
      SaveFileLocation = null;
      SaveFileContent = null;
      Checksum = null;
      lblActualChecksum.Content = lblCurrentChecksum.Content = "?? ??";
      lblHeadActualChecksum.IsEnabled = lblHeadCurrentChecksum.IsEnabled = false;
      btnFixChecksum.IsEnabled = false;
      lblFilePath.Content = "";
      lblSaved.Visibility = Visibility.Hidden;
    }
  }
}
