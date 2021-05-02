using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CG_lab1KM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private ImageSource image_backup { get; set; }
        private List<RadioButton> custom_filters_buttons = new List<RadioButton>();
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Uri fileUri = new Uri(openFileDialog.FileName);
                image_backup = new BitmapImage(fileUri);
                Result_Image.Source = image_backup;
                Raw_Image.Source = image_backup;
            }
            ConvolutionPanel.IsEnabled = true;
            FunctionPanel.IsEnabled = true;
            SaveButton.IsEnabled = true;
            DitherRow.IsEnabled = true;
        }

        private void Revert_Click(object sender, RoutedEventArgs e)
        {
            if (Result_Image.Source != null)
            {
                Result_Image.Source = image_backup;
                Revert_button.IsEnabled = false;
            }
            
        }

        private void UpdateKernelSizePanel(ConvoFilter filter)
        {
            foreach (ComboBoxItem item in KernelSizeX.Items)
            {
                if (Int32.Parse((string)item.Content) == filter.Kernel.GetLength(0))
                {
                    KernelSizeX.SelectedItem = item;
                }
            }
            foreach (ComboBoxItem item in KernelSizeY.Items)
            {
                if (Int32.Parse((string)item.Content) == filter.Kernel.GetLength(1))
                {
                    KernelSizeY.SelectedItem = item;
                }
            }
        }
        private void UpdateKernelEntries(ConvoFilter filter)
        {
            ComboBoxItem rows_item = KernelSizeX.SelectedItem as ComboBoxItem;
            ComboBoxItem cols_item = KernelSizeY.SelectedItem as ComboBoxItem;

            int i = 0;
            int j = 0;
            if (kernel_panel != null)
            {
                var panel = kernel_panel.Children;
                foreach(StackPanel stack in panel)
                {
                    foreach(TextBox box in stack.Children)
                    {
                        box.Text = filter.Kernel[i, j].ToString();
                        j++;
                    }
                    i++;
                    j = 0;
                }
            }
        }
        private void UpdateDivisorOffset(ConvoFilter filter)
        {
            OffsetInput.Text = filter.Offset.ToString();
            DivisorInput.Text = filter.Divisor.ToString();
            AnchorXInput.Text = filter.AnchorX.ToString();
            AnchorYInput.Text = filter.AnchorY.ToString();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Bitmap Image (.bmp)|*.bmp|Gif Image (.gif)|*.gif|JPEG Image (.jpeg)|*.jpeg|Png Image (.png)|*.png";
            dialog.AddExtension = true;
            dialog.Title = "Save your image";
            dialog.DefaultExt = "png";

            if (dialog.ShowDialog() == true)
            {
                string fName = dialog.FileName;
                using (var fileStream = new FileStream(fName, FileMode.Create))
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)Result_Image.Source));
                    encoder.Save(fileStream);
                }
            }
        }

        private float CalculateDivisor(float[,] Kernel)
        {
            float Weight = 0.0f;
            for (int row = 0; row < Kernel.GetLength(0); row++)
            {
                for (int col = 0; col < Kernel.GetLength(1); col++)
                {
                    Weight += Kernel[row, col];
                }
            }
            if (Weight == 0)
                Weight = 1;
            return Weight;
        }

        private void Apply_button_Click(object sender, RoutedEventArgs e)
        {
            if (Inversion_Button.IsChecked == true)
                Result_Image.Source = Filters.Inverse((BitmapSource)Result_Image.Source);
            else if (Brightness_Button.IsChecked == true)
                Result_Image.Source = Filters.BrightnessCorr((BitmapSource)Result_Image.Source);
            else if (Contrast_button.IsChecked == true)
                Result_Image.Source = Filters.ContrastEnh((BitmapSource)Result_Image.Source);
            else if (Gamma_button.IsChecked == true)
                Result_Image.Source = Filters.GammaCorr((BitmapSource)Result_Image.Source);
            else if (Blur_button.IsChecked == true)
                Result_Image.Source = Filters.ConvolutionFilter((BitmapSource)Result_Image.Source, Filters.blurFilter);
            else if (Gblur_button.IsChecked == true)
                Result_Image.Source = Filters.ConvolutionFilter((BitmapSource)Result_Image.Source, Filters.GBlurFilter);
            else if (Sharpen_button.IsChecked == true)
                Result_Image.Source = Filters.ConvolutionFilter((BitmapSource)Result_Image.Source, Filters.SharpenFilter);
            else if (Edge_button.IsChecked == true)
                Result_Image.Source = Filters.ConvolutionFilter((BitmapSource)Result_Image.Source, Filters.EdgeDetectionFilter);
            else if (Emboss_button.IsChecked == true)
                Result_Image.Source = Filters.ConvolutionFilter((BitmapSource)Result_Image.Source, Filters.EmbossingFilter);
            else if (Labfilter_button.IsChecked == true)
            {
                Result_Image.Source = Filters.LabFilter((BitmapSource)Result_Image.Source);
            }
            else if (Dither_button.IsChecked == true)
            {
                Result_Image.Source = Filters.OrderedDithering((BitmapSource)Result_Image.Source,
                    Int32.Parse(RedColorNumber.Text), Int32.Parse(GreenColorNumber.Text), Int32.Parse(BlueColorNumber.Text));
            }
            else if (Pop_alg_button.IsChecked == true)
            {
                Result_Image.Source = Filters.PopularityAlgorithm((BitmapSource)Result_Image.Source, Int32.Parse(PopColorNumber.Text));
            }
            else
            {
                for(int i = 0; i < custom_filters_buttons.Count; ++i)
                {
                    if (custom_filters_buttons[i].IsChecked == true)
                    {
                        Result_Image.Source = Filters.ConvolutionFilter((BitmapSource)Result_Image.Source, Filters.cfilters[i]);
                    }
                }
            }
            Revert_button.IsEnabled = true;
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            


        }
        private void Filter_checked(object sender, RoutedEventArgs e)
        {
            if (Inversion_Button.IsChecked == true || Brightness_Button.IsChecked == true || Contrast_button.IsChecked == true ||
                Gamma_button.IsChecked == true || Labfilter_button.IsChecked == true || Dither_button.IsChecked == true ||
                Pop_alg_button.IsChecked == true)
                task2panel.IsEnabled = false;
            else
            {
                Applyto_newfilter_button.IsEnabled = true;
                task2panel.IsEnabled = true;
                Save_newfilter_button.IsEnabled = true;
            }
                
            Apply_button.IsEnabled = true;
        }


        //autodiv checkbox checked
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DivisorInput.IsEnabled = false;

            ComboBoxItem rows_item = KernelSizeX.SelectedItem as ComboBoxItem;
            ComboBoxItem cols_item = KernelSizeY.SelectedItem as ComboBoxItem;

            float[,] new_kernel = new float[Int32.Parse((string)rows_item.Content), Int32.Parse((string)cols_item.Content)];
            int i = 0;
            int j = 0;
            if (kernel_panel != null)
            {
                var panel = kernel_panel.Children;
                foreach (StackPanel stack in panel)
                {
                    foreach (TextBox box in stack.Children)
                    {
                        float number; 
                        new_kernel[i, j] = Single.TryParse(box.Text, out number) ? number : 0;
                        if (number == 0)
                            box.Text = "0";
                        j++;
                    }
                    i++;
                    j = 0;
                }
            }
            DivisorInput.Text = CalculateDivisor(new_kernel).ToString();

        }

        private void auto_div_Unchecked(object sender, RoutedEventArgs e)
        {
            DivisorInput.IsEnabled = true;
        }
       

        
        

        private void Blur_button_Click(object sender, RoutedEventArgs e)
        {
            UpdateKernelSizePanel(Filters.blurFilter);
            UpdateKernelEntries(Filters.blurFilter);
            UpdateDivisorOffset(Filters.blurFilter);
        }
        private void Gauss_button_Click(object sender, RoutedEventArgs e)
        {
            UpdateKernelSizePanel(Filters.GBlurFilter);
            UpdateDivisorOffset(Filters.GBlurFilter);
            UpdateKernelEntries(Filters.GBlurFilter);

        }
        private void Sharp_button_Click(object sender, RoutedEventArgs e)
        {
            UpdateKernelSizePanel(Filters.SharpenFilter);
            UpdateDivisorOffset(Filters.SharpenFilter);
            UpdateKernelEntries(Filters.SharpenFilter);
        }
        private void Edge_button_Click(object sender, RoutedEventArgs e)
        {
            UpdateKernelSizePanel(Filters.EdgeDetectionFilter);
            UpdateDivisorOffset(Filters.EdgeDetectionFilter);
            UpdateKernelEntries(Filters.EdgeDetectionFilter);
        }
        private void Emboss_button_Click(object sender, RoutedEventArgs e)
        {
            UpdateKernelSizePanel(Filters.EmbossingFilter);
            UpdateDivisorOffset(Filters.EmbossingFilter);
            UpdateKernelEntries(Filters.EmbossingFilter);
        }

        

        private void KernelSizeX_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PopulateKernel();
        }

        private void KernelSizeY_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PopulateKernel();
        }

        private void ApplyToExistingFilter(ConvoFilter filter)
        {
            ComboBoxItem rows_item = KernelSizeX.SelectedItem as ComboBoxItem;
            ComboBoxItem cols_item = KernelSizeY.SelectedItem as ComboBoxItem;
            float[,] new_kernel = new float[Int32.Parse((string)rows_item.Content), Int32.Parse((string)cols_item.Content)];
            int i = 0;
            int j = 0;
            if (kernel_panel != null)
            {
                var panel = kernel_panel.Children;
                foreach (StackPanel stack in panel)
                {
                    foreach (TextBox box in stack.Children)
                    {
                        float number1;
                        new_kernel[i, j] = Single.TryParse(box.Text, out number1) ? number1 : 0;
                        j++;
                    }
                    i++;
                    j = 0;
                }
            }
            float number;
            filter.Kernel = new_kernel;
            filter.Divisor = Single.TryParse(DivisorInput.Text, out number) ? number : 0;
            filter.Offset = Single.TryParse(OffsetInput.Text, out number) ? number : 0;
            filter.AnchorX = Int32.Parse((string)AnchorXInput.Text);
            filter.AnchorY = Int32.Parse((string)AnchorYInput.Text);
        }

        private void Applyto_newfilter_button_Click(object sender, RoutedEventArgs e)
        {
            if (Blur_button.IsChecked == true)
                ApplyToExistingFilter(Filters.blurFilter);
            else if (Gblur_button.IsChecked == true)
                ApplyToExistingFilter(Filters.GBlurFilter);
            else if (Sharpen_button.IsChecked == true)
                ApplyToExistingFilter(Filters.SharpenFilter);
            else if (Edge_button.IsChecked == true)
                ApplyToExistingFilter(Filters.EdgeDetectionFilter);
            else if (Emboss_button.IsChecked == true)
                ApplyToExistingFilter(Filters.EmbossingFilter);
            else
            {
                for (int i = 0; i < custom_filters_buttons.Count; ++i)
                {
                    if (custom_filters_buttons[i].IsChecked == true)
                    {
                        ApplyToExistingFilter(Filters.cfilters[i]);
                    }
                }
            }
        }

        private void Custom_button_Click(object sender, RoutedEventArgs e)
        {
            RadioButton customButton = e.Source as RadioButton;
            int which = Int32.Parse(((string)customButton.Content).Split(' ')[1]);
            UpdateKernelSizePanel(Filters.cfilters[which-1]);
            UpdateKernelEntries(Filters.cfilters[which-1]);
            UpdateDivisorOffset(Filters.cfilters[which-1]);
        }

        private void Save_newfilter_button_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem rows_item = KernelSizeX.SelectedItem as ComboBoxItem;
            ComboBoxItem cols_item = KernelSizeY.SelectedItem as ComboBoxItem;

            float[,] new_kernel = new float[Int32.Parse((string)rows_item.Content), Int32.Parse((string)cols_item.Content)];
            int i = 0;
            int j = 0;
            if (kernel_panel != null)
            {
                var panel = kernel_panel.Children;
                foreach (StackPanel stack in panel)
                {
                    foreach (TextBox box in stack.Children)
                    {
                        float number1;
                        new_kernel[i,j] = Single.TryParse(box.Text, out number1) ? number1 : 0;
                        j++;
                    }
                    i++;
                    j = 0;
                }
            }
            float number;
            CustomFilter cf = new CustomFilter()
            {
                Divisor = Single.TryParse(DivisorInput.Text, out number) ? number : 1,
                Offset = Single.TryParse(OffsetInput.Text, out number) ? number : 0,
                Kernel = new_kernel,
                AnchorX = Int32.Parse((string)AnchorXInput.Text),
                AnchorY = Int32.Parse((string)AnchorYInput.Text)
            };
            Filters.cfilters.Add(cf);
            RadioButton rb = new RadioButton();
            rb.Content = "Custom " + Filters.cfilters.Count;
            rb.Checked += Filter_checked; 
            rb.Click += Custom_button_Click;
            rb.GroupName = "FiltersGroup";
            
            rb.Margin = new Thickness(10, 0, 10, 10);
            ConvolutionPanel.Children.Add(rb);
            custom_filters_buttons.Add(rb);
        }

        private void Labfilter_button_Click(object sender, RoutedEventArgs e)
        {

        }

        //private void a00_TextChanged(object sender, TextChangedEventArgs e)
        //{

            
        //    float[,] new_kernel = new float[3, 3];
        //    int i = 0;
        //    int j = 0;
        //    if (labpart_panel != null)
        //    {
        //        var panel = labpart_panel.Children;
        //        foreach (StackPanel stack in panel)
        //        {
        //            foreach (TextBox box in stack.Children)
        //            {

        //                float number1;
        //                new_kernel[i, j] = Single.TryParse(box.Text, out number1) ? number1 : 1.0f;
        //                //if (number1 == 0)
        //                //    box.Text = "1";
        //                j++;
        //            }
        //            i++;
        //            j = 0;
        //        }
        //    }
        //    Filters.lb.Matrix = new_kernel;
        //}

        private void a20_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox temp = e.Source as TextBox;
            int x = Int32.Parse(temp.Name[1].ToString());
            int y = Int32.Parse(temp.Name[2].ToString());

            temp.Text = Filters.lb.Matrix[x, y].ToString();
        }
        private void PopulateKernel()
        {
            if (kernel_panel != null)
            {
                kernel_panel.Children.Clear();
                ComboBoxItem rows_item = KernelSizeX.SelectedItem as ComboBoxItem;
                ComboBoxItem cols_item = KernelSizeY.SelectedItem as ComboBoxItem;
                for (int i = 0; i < Int32.Parse((string)rows_item.Content); i++)
                {
                    StackPanel panel = new StackPanel();
                    panel.Name = "row" + i;
                    panel.HorizontalAlignment = HorizontalAlignment.Center;
                    panel.Orientation = Orientation.Horizontal;
                    for (int j = 0; j < Int32.Parse((string)cols_item.Content); j++)
                    {
                        TextBox box = new TextBox();
                        box.LostFocus += TextChanged;
                        box.Name = "e" + i + "" + j;
                        box.TextAlignment = TextAlignment.Center;
                        box.Height = 20;
                        box.Width = 40;
                        box.Text = "1";
                        panel.Children.Add(box);

                    }

                    kernel_panel.Children.Add(panel);
                }

            }
        }
        private void e00_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextChanged(object sender, RoutedEventArgs e)
        {
            ComboBoxItem rows_item = KernelSizeX.SelectedItem as ComboBoxItem;
            ComboBoxItem cols_item = KernelSizeY.SelectedItem as ComboBoxItem;


            float[,] new_kernel = new float[Int32.Parse((string)rows_item.Content), Int32.Parse((string)cols_item.Content)];
            int i = 0;
            int j = 0;
            if (kernel_panel != null)
            {
                var panel = kernel_panel.Children;
                foreach (StackPanel stack in panel)
                {
                    foreach (TextBox box in stack.Children)
                    {
                        float number;
                        new_kernel[i, j] = Single.TryParse(box.Text, out number) ? number : 0;
                        if (number == 0)
                            box.Text = "0";
                        else if (box.Text == "")
                            box.Text = "";
                        j++;

                    }
                    i++;
                    j = 0;
                }
            }

            if (auto_div.IsChecked == true)
            {
                DivisorInput.IsEnabled = true;
                DivisorInput.Text = CalculateDivisor(new_kernel).ToString();
                DivisorInput.IsEnabled = false;

            }
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Dither_button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Pop_alg_button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TmapSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorPanel != null)
            {
                ComboBoxItem threshsize = KernelSizeX.SelectedItem as ComboBoxItem;
                switch (Int32.Parse((string)threshsize.Content))
                {
                    case (2):
                        Filters.ThresholdMap = Filters.DitherMap2x2;
                        break;
                    case (3):
                        Filters.ThresholdMap = Filters.DitherMap3x3;
                        break;
                    case (4):
                        Filters.ThresholdMap = Filters.DitherMap4x4;
                        break;
                    case (6):
                        Filters.ThresholdMap = Filters.DitherMap6x6;
                        break;
                }
            }
        }

        private void RedColorNumber_KeyUp(object sender, KeyEventArgs e)
        {
           
        }

        private void size_two_Selected(object sender, RoutedEventArgs e)
        {
            Filters.ThresholdMap = Filters.DitherMap2x2;
        }

        private void size_three_Selected(object sender, RoutedEventArgs e)
        {
            Filters.ThresholdMap = Filters.DitherMap3x3;
        }

        private void size_four_Selected(object sender, RoutedEventArgs e)
        {
            Filters.ThresholdMap = Filters.DitherMap4x4;
        }

        private void size_six_Selected(object sender, RoutedEventArgs e)
        {
            Filters.ThresholdMap = Filters.DitherMap6x6;
        }
    }
}
