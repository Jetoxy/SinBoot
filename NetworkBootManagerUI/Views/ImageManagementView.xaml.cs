using System;
using System.Windows.Controls; // Moved this using statement to the top

// It's generally better practice to put data classes like ImageInfo
// in their own file, perhaps in a 'Models' folder/namespace.
// However, keeping it here is also valid as long as it's within the *same* namespace block.

namespace NetworkBootManagerUI.Views
{
    // Definition for ImageInfo (can stay here or move to its own file/namespace)
    public class ImageInfo
    {
        public string? FileName { get; set; }
        public long SizeMB { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsReadOnly { get; set; }
    }

    /// <summary>
    /// Interaction logic for ImageManagementView.xaml
    /// </summary>
    public partial class ImageManagementView : UserControl // UserControl should now be recognized
    {
        public ImageManagementView()
        {
            InitializeComponent();
            // Логика загрузки списка образов и управления ими будет здесь
        }
    }
} // Only one namespace block needed here