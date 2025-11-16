using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaloniaDiagramerControl.Demo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Load sample data by default
        LoadSampleData1_Click(null, null!);
    }

    private void LoadSampleData1_Click(object? sender, RoutedEventArgs e)
    {
        ChartControl.ClearDataSeries();
        ChartControl.ChartTitle = "Sales Performance by Product Line";
        ChartControl.XAxisLabel = "Quarter,Q1,Q2,Q3,Q4,Q5,Q6,Q7,Q8,Q9,Q10,Q11,Q12";

        // Interesting upward trending data
        ChartControl.AddDataSeries("Premium Products,100,150,180,220,280,320,380,420,450,480,520,580");
        ChartControl.AddDataSeries("Standard Products,80,95,110,125,140,155,175,190,210,230,250,275");
        ChartControl.AddDataSeries("Budget Products,50,60,70,75,85,90,100,105,115,120,130,140");
        ChartControl.AddDataSeries("Enterprise Products,200,220,250,280,310,340,380,420,460,500,550,600");

        ChartControl.InvalidateVisual();
    }

    private void LoadSampleData2_Click(object? sender, RoutedEventArgs e)
    {
        ChartControl.ClearDataSeries();
        ChartControl.ChartTitle = "Temperature Variations Throughout the Year";
        ChartControl.XAxisLabel = "Month,Jan,Feb,Mar,Apr,May,Jun,Jul,Aug,Sep,Oct,Nov,Dec";

        // Seasonal data with interesting patterns
        ChartControl.AddDataSeries("City A,-10,-5,5,15,25,35,40,38,28,18,5,-8");
        ChartControl.AddDataSeries("City B,0,3,10,18,28,32,35,33,26,16,8,2");
        ChartControl.AddDataSeries("City C,20,22,25,28,32,35,38,37,33,28,24,21");
        ChartControl.AddDataSeries("City D,-20,-15,-5,8,18,25,30,28,20,10,-2,-15");
        ChartControl.AddDataSeries("City E,15,17,20,24,29,33,36,35,30,25,20,16");

        ChartControl.InvalidateVisual();
    }

    private void LoadSampleData3_Click(object? sender, RoutedEventArgs e)
    {
        ChartControl.ClearDataSeries();
        ChartControl.ChartTitle = "Stock Market Performance Analysis";
        ChartControl.XAxisLabel = "Week,W1,W2,W3,W4,W5,W6,W7,W8,W9,W10,W11,W12,W13";

        // Volatile market data with ups and downs
        ChartControl.AddDataSeries("Tech Stocks,100,105,110,108,115,125,120,130,135,128,140,145,150");
        ChartControl.AddDataSeries("Energy Stocks,80,85,82,90,95,88,92,98,100,95,105,110,108");
        ChartControl.AddDataSeries("Financial Stocks,120,118,125,130,128,135,140,138,145,150,148,155,160");
        ChartControl.AddDataSeries("Healthcare Stocks,90,92,95,98,100,102,105,108,110,112,115,118,120");
        ChartControl.AddDataSeries("Consumer Goods,75,78,80,82,85,88,90,92,95,98,100,103,105");
        ChartControl.AddDataSeries("Real Estate,60,62,65,68,70,72,75,78,80,82,85,88,90");

        ChartControl.InvalidateVisual();
    }

    private void ClearButton_Click(object? sender, RoutedEventArgs e)
    {
        ChartControl.ClearDataSeries();
        ChartControl.ChartTitle = "TAIDiagramer - Multi-Series Line Chart Demo";
        ChartControl.InvalidateVisual();
    }

    private void ShowGridCheckBox_CheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (ShowGridCheckBox.IsChecked == true)
        {
            ChartControl.ShowGrid = true;
        }
        else
        {
            ChartControl.ShowGrid = false;
        }
        ChartControl.InvalidateVisual();
    }

    private void ShowTrendLinesCheckBox_CheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (ShowTrendLinesCheckBox.IsChecked == true)
        {
            ChartControl.DrawRegressionTrendLines = true;
        }
        else
        {
            ChartControl.DrawRegressionTrendLines = false;
        }
        ChartControl.InvalidateVisual();
    }

    private void FlashSeries1Button_Click(object? sender, RoutedEventArgs e)
    {
        // Flash the first data series (line 0)
        System.Diagnostics.Debug.WriteLine("Flash button clicked - setting FlashDataLine to 0");
        ChartControl.FlashDataLine = 0;
    }

    private void StopFlashButton_Click(object? sender, RoutedEventArgs e)
    {
        // Stop flashing by setting to -1
        System.Diagnostics.Debug.WriteLine("Stop flash button clicked");
        ChartControl.FlashDataLine = -1;
    }
}