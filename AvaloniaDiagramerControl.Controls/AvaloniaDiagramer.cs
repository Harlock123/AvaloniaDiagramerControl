using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using System;
using System.Collections.Generic;

namespace AvaloniaDiagramerControl.Controls;

/// <summary>
/// Avalonia line chart control for rendering multiple data series with trend lines.
/// Ported from the original VB.NET Windows Forms TAIDiagramer control.
/// </summary>
public class AvaloniaDiagramer : Control
{
    #region Fields

    private int _canvasWidth = 500;
    private int _canvasHeight = 500;
    private int _bottomMargin = 90;
    private int _sideMargin = 35;
    private int _topMargin = 40;
    private string _chartTitle = "Sample Title";
    private Typeface _chartTitleFont = new Typeface("Arial", FontStyle.Normal, FontWeight.Bold);
    private double _chartTitleFontSize = 15;
    private Color _chartTitleColor = Colors.Black;
    private Typeface _axisFont = new Typeface("Arial", FontStyle.Normal, FontWeight.Bold);
    private double _axisFontSize = 12;
    private Typeface _yAxisFont = new Typeface("Arial");
    private double _yAxisFontSize = 14;
    private Typeface _xAxisFont = new Typeface("Arial");
    private double _xAxisFontSize = 14;
    private Typeface _axisLabelFont = new Typeface("Arial", FontStyle.Normal, FontWeight.Bold);
    private double _axisLabelFontSize = 12;
    private Color _axisDrawColor = Colors.Black;
    private bool _drawKnots = true;
    private bool _drawYAxisValues = true;
    private bool _drawRegressionTrendLines = true;
    private bool _drawAlternateChartBackground = true;
    private bool _drawIntervalDataSeriesIntersects = true;
    private string _xAxisLabel = "";
    private double _axisLineWeight = 4;
    private bool _antiAlias = true;
    private bool _clearChartArea = true;
    private bool _outlineDataSeries = true;
    private bool _chartGrid = true;
    private int _chartGridGranularity = 10;
    private int _trendLineDegreeValue = 2;
    private int _trendLineWeight = 2;
    private double _drawLineWeight = 4;
    private bool _showGrid = true;
    private bool _showBorder = true;
    private Color _canvasBackColor = Color.Parse("#FAEBD7"); // AntiqueWhite
    private Color _chartBackColor = Color.Parse("#FFFAF0"); // FloralWhite
    private Color _chartAlternateBackColor = Color.Parse("#F0FFF0"); // Honeydew
    private Color _gridColor = Colors.Gray;
    private List<string>? _dataLines;
    private readonly Color[] _dataLineColors = new Color[10];
    private readonly Color[] _oldDataLineColors = new Color[10];
    private readonly bool[] _drawDataLines = new bool[10];
    private int _flashDataLine = -1;
    private int _gridSize = 20;
    private DispatcherTimer? _flashTimer;
    private Color _flashSeriesColor = Color.Parse("#FF0000"); // Bright Red

    #endregion

    #region Constructor

    public AvaloniaDiagramer()
    {
        InitializeDataLineColors();

        // Don't set explicit Width/Height - let the control stretch to fill available space
        // Width and Height will be determined by the layout system

        // Listen for size changes to trigger redraw
        PropertyChanged += (s, e) =>
        {
            if (e.Property == BoundsProperty)
            {
                InvalidateVisual();
            }
        };
    }

    private void InitializeDataLineColors()
    {
        _dataLineColors[0] = Colors.Blue;
        _dataLineColors[1] = Colors.Red;
        _dataLineColors[2] = Colors.Green;
        _dataLineColors[3] = Colors.Purple;
        _dataLineColors[4] = Colors.Cyan;
        _dataLineColors[5] = Colors.Magenta;
        _dataLineColors[6] = Colors.Yellow;
        _dataLineColors[7] = Color.Parse("#228B22"); // ForestGreen
        _dataLineColors[8] = Color.Parse("#FF1493"); // DeepPink
        _dataLineColors[9] = Colors.Gray;

        Array.Copy(_dataLineColors, _oldDataLineColors, 10);

        for (int i = 0; i < 10; i++)
        {
            _drawDataLines[i] = true;
        }
    }

    #endregion

    #region Properties

    public bool AntiAlias
    {
        get => _antiAlias;
        set { _antiAlias = value; InvalidateVisual(); }
    }

    public Color AxisDrawColor
    {
        get => _axisDrawColor;
        set { _axisDrawColor = value; InvalidateVisual(); }
    }

    public double AxisLineWeight
    {
        get => _axisLineWeight;
        set { _axisLineWeight = value; InvalidateVisual(); }
    }

    public Color CanvasBackColor
    {
        get => _canvasBackColor;
        set
        {
            _canvasBackColor = value;
            InvalidateVisual();
        }
    }

    public int CanvasHeight
    {
        get => _canvasHeight;
        set
        {
            _canvasHeight = value;
            // Don't set explicit Height - let it be determined by layout
            InvalidateMeasure();
            InvalidateVisual();
        }
    }

    public int CanvasWidth
    {
        get => _canvasWidth;
        set
        {
            _canvasWidth = value;
            // Don't set explicit Width - let it be determined by layout
            InvalidateMeasure();
            InvalidateVisual();
        }
    }

    public Color ChartBackColor
    {
        get => _chartBackColor;
        set { _chartBackColor = value; InvalidateVisual(); }
    }

    public Color ChartAlternateBackColor
    {
        get => _chartAlternateBackColor;
        set { _chartAlternateBackColor = value; InvalidateVisual(); }
    }

    public bool ChartGrid
    {
        get => _chartGrid;
        set { _chartGrid = value; InvalidateVisual(); }
    }

    public int ChartGridGranularity
    {
        get => _chartGridGranularity;
        set { _chartGridGranularity = value; InvalidateVisual(); }
    }

    public string ChartTitle
    {
        get => _chartTitle;
        set { _chartTitle = value; InvalidateVisual(); }
    }

    public Color ChartTitleColor
    {
        get => _chartTitleColor;
        set { _chartTitleColor = value; InvalidateVisual(); }
    }

    public string ChartTitleFontFamily
    {
        get => _chartTitleFont.FontFamily.Name;
        set
        {
            _chartTitleFont = new Typeface(value, _chartTitleFont.Style, _chartTitleFont.Weight);
            InvalidateVisual();
        }
    }

    public double ChartTitleFontSize
    {
        get => _chartTitleFontSize;
        set { _chartTitleFontSize = value; InvalidateVisual(); }
    }

    public FontStyle ChartTitleFontStyle
    {
        get => _chartTitleFont.Style;
        set
        {
            _chartTitleFont = new Typeface(_chartTitleFont.FontFamily, value, _chartTitleFont.Weight);
            InvalidateVisual();
        }
    }

    public FontWeight ChartTitleFontWeight
    {
        get => _chartTitleFont.Weight;
        set
        {
            _chartTitleFont = new Typeface(_chartTitleFont.FontFamily, _chartTitleFont.Style, value);
            InvalidateVisual();
        }
    }

    public string YAxisFontFamily
    {
        get => _yAxisFont.FontFamily.Name;
        set
        {
            _yAxisFont = new Typeface(value, _yAxisFont.Style, _yAxisFont.Weight);
            InvalidateVisual();
        }
    }

    public double YAxisFontSize
    {
        get => _yAxisFontSize;
        set { _yAxisFontSize = value; InvalidateVisual(); }
    }

    public FontStyle YAxisFontStyle
    {
        get => _yAxisFont.Style;
        set
        {
            _yAxisFont = new Typeface(_yAxisFont.FontFamily, value, _yAxisFont.Weight);
            InvalidateVisual();
        }
    }

    public FontWeight YAxisFontWeight
    {
        get => _yAxisFont.Weight;
        set
        {
            _yAxisFont = new Typeface(_yAxisFont.FontFamily, _yAxisFont.Style, value);
            InvalidateVisual();
        }
    }

    public string XAxisFontFamily
    {
        get => _xAxisFont.FontFamily.Name;
        set
        {
            _xAxisFont = new Typeface(value, _xAxisFont.Style, _xAxisFont.Weight);
            InvalidateVisual();
        }
    }

    public double XAxisFontSize
    {
        get => _xAxisFontSize;
        set { _xAxisFontSize = value; InvalidateVisual(); }
    }

    public FontStyle XAxisFontStyle
    {
        get => _xAxisFont.Style;
        set
        {
            _xAxisFont = new Typeface(_xAxisFont.FontFamily, value, _xAxisFont.Weight);
            InvalidateVisual();
        }
    }

    public FontWeight XAxisFontWeight
    {
        get => _xAxisFont.Weight;
        set
        {
            _xAxisFont = new Typeface(_xAxisFont.FontFamily, _xAxisFont.Style, value);
            InvalidateVisual();
        }
    }

    public string AxisLabelFontFamily
    {
        get => _axisLabelFont.FontFamily.Name;
        set
        {
            _axisLabelFont = new Typeface(value, _axisLabelFont.Style, _axisLabelFont.Weight);
            InvalidateVisual();
        }
    }

    public double AxisLabelFontSize
    {
        get => _axisLabelFontSize;
        set { _axisLabelFontSize = value; InvalidateVisual(); }
    }

    public FontStyle AxisLabelFontStyle
    {
        get => _axisLabelFont.Style;
        set
        {
            _axisLabelFont = new Typeface(_axisLabelFont.FontFamily, value, _axisLabelFont.Weight);
            InvalidateVisual();
        }
    }

    public FontWeight AxisLabelFontWeight
    {
        get => _axisLabelFont.Weight;
        set
        {
            _axisLabelFont = new Typeface(_axisLabelFont.FontFamily, _axisLabelFont.Style, value);
            InvalidateVisual();
        }
    }

    public bool ClearChartArea
    {
        get => _clearChartArea;
        set { _clearChartArea = value; InvalidateVisual(); }
    }

    public bool DrawKnots
    {
        get => _drawKnots;
        set { _drawKnots = value; InvalidateVisual(); }
    }

    public bool DrawYAxisValues
    {
        get => _drawYAxisValues;
        set { _drawYAxisValues = value; InvalidateVisual(); }
    }

    public bool DrawRegressionTrendLines
    {
        get => _drawRegressionTrendLines;
        set { _drawRegressionTrendLines = value; InvalidateVisual(); }
    }

    public bool DrawAlternateChartBackground
    {
        get => _drawAlternateChartBackground;
        set { _drawAlternateChartBackground = value; InvalidateVisual(); }
    }

    public bool DrawIntervalDataSeriesIntersects
    {
        get => _drawIntervalDataSeriesIntersects;
        set { _drawIntervalDataSeriesIntersects = value; InvalidateVisual(); }
    }

    public double DrawLineWeight
    {
        get => _drawLineWeight;
        set { _drawLineWeight = value; InvalidateVisual(); }
    }

    public bool OutlineDataSeries
    {
        get => _outlineDataSeries;
        set { _outlineDataSeries = value; InvalidateVisual(); }
    }

    public bool ShowGrid
    {
        get => _showGrid;
        set { _showGrid = value; InvalidateVisual(); }
    }

    public bool ShowBorder
    {
        get => _showBorder;
        set { _showBorder = value; InvalidateVisual(); }
    }

    public int TrendLineDegreeValue
    {
        get => _trendLineDegreeValue;
        set { _trendLineDegreeValue = value; InvalidateVisual(); }
    }

    public int TrendLineWeight
    {
        get => _trendLineWeight;
        set { _trendLineWeight = value; InvalidateVisual(); }
    }

    public string XAxisLabel
    {
        get => _xAxisLabel;
        set { _xAxisLabel = value; InvalidateVisual(); }
    }

    public int FlashDataLine
    {
        get => _flashDataLine;
        set
        {
            System.Diagnostics.Debug.WriteLine($"FlashDataLine setter called with value: {value}");

            if (_flashDataLine != -1 && _flashDataLine < 10)
            {
                _dataLineColors[_flashDataLine] = _oldDataLineColors[_flashDataLine];
                System.Diagnostics.Debug.WriteLine($"  Restored color for line {_flashDataLine}");
            }

            if (value < 10 && value > -1)
            {
                if (_flashTimer == null)
                {
                    _flashTimer = new DispatcherTimer(
                        TimeSpan.FromMilliseconds(500),
                        DispatcherPriority.Normal,
                        FlashTimer_Tick);
                }
                else
                {
                    _flashTimer.Stop();
                }
                _flashDataLine = value;
                _flashTimer.Start();
                System.Diagnostics.Debug.WriteLine($"  Timer started for line {value}, IsEnabled: {_flashTimer.IsEnabled}");
            }
            else
            {
                _flashTimer?.Stop();
                _flashDataLine = -1;
                System.Diagnostics.Debug.WriteLine($"  Timer stopped");
            }

            InvalidateVisual();
        }
    }

    public Color FlashSeriesColor
    {
        get => _flashSeriesColor;
        set
        {
            _flashSeriesColor = value;
            InvalidateVisual();
        }
    }

    public Color GetDataLineColor(int index)
    {
        if (index >= 0 && index < 10)
            return _dataLineColors[index];
        return Colors.Black;
    }

    public void SetDataLineColor(int index, Color color)
    {
        if (index >= 0 && index < 10)
        {
            _dataLineColors[index] = color;
            _oldDataLineColors[index] = color;
            InvalidateVisual();
        }
    }

    public bool GetDrawDataLine(int index)
    {
        if (index >= 0 && index < 10)
            return _drawDataLines[index];
        return false;
    }

    public void SetDrawDataLine(int index, bool value)
    {
        if (index >= 0 && index < 10)
        {
            _drawDataLines[index] = value;
            InvalidateVisual();
        }
    }

    #endregion

    #region Public Methods

    public void AddDataSeries(string dataSeriesString)
    {
        _dataLines ??= new List<string>();
        _dataLines.Add(dataSeriesString);
    }

    public void ClearDataSeries()
    {
        _dataLines?.Clear();
        _flashTimer?.Stop();
        Array.Copy(_oldDataLineColors, _dataLineColors, 10);
        _flashDataLine = -1;
        InvalidateVisual();
    }

    #endregion

    #region Layout

    protected override Size MeasureOverride(Size availableSize)
    {
        // If available size is specified, use it; otherwise use our default canvas size
        double width = double.IsInfinity(availableSize.Width) ? _canvasWidth : availableSize.Width;
        double height = double.IsInfinity(availableSize.Height) ? _canvasHeight : availableSize.Height;
        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // Accept whatever size we're given and fill it
        return finalSize;
    }

    #endregion

    #region Rendering

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // Fill background
        context.FillRectangle(new SolidColorBrush(_canvasBackColor), new Rect(0, 0, Bounds.Width, Bounds.Height));

        // Draw grid if enabled
        if (_showGrid)
        {
            DrawGrid(context);
        }

        // Draw data series
        if (_dataLines != null && _dataLines.Count > 0)
        {
            RenderDataSeries(context);
        }
    }

    private void DrawGrid(DrawingContext context)
    {
        var dashStyle = new DashStyle(new double[] { 2, 2 }, 0);
        var pen = new Pen(new SolidColorBrush(_gridColor), 1, dashStyle);

        for (int x = 0; x < Bounds.Width; x += _gridSize)
        {
            context.DrawLine(pen, new Point(x, 0), new Point(x, Bounds.Height));
        }

        for (int y = 0; y < Bounds.Height; y += _gridSize)
        {
            context.DrawLine(pen, new Point(0, y), new Point(Bounds.Width, y));
        }
    }

    private void RenderDataSeries(DrawingContext context)
    {
        if (_dataLines == null || _dataLines.Count == 0)
            return;

        // Check if any data lines are enabled
        bool anyEnabled = false;
        for (int i = 0; i < _dataLines.Count && i < 10; i++)
        {
            if (_drawDataLines[i])
            {
                anyEnabled = true;
                break;
            }
        }

        if (!anyEnabled)
            return;

        // Range checking
        if (Bounds.Height - _topMargin - _bottomMargin < 10)
            return;

        // Find max elements across all series
        int maxElements = 0;
        foreach (var line in _dataLines)
        {
            var parts = line.Split(',');
            if (parts.Length - 1 > maxElements)
                maxElements = parts.Length - 1;
        }

        if (maxElements < 1)
            return;

        // Load data into array
        var dataSeries = new string[_dataLines.Count, maxElements + 1];
        for (int i = 0; i < _dataLines.Count; i++)
        {
            var parts = _dataLines[i].Split(',');
            for (int j = 0; j < parts.Length && j <= maxElements; j++)
            {
                dataSeries[i, j] = parts[j];
            }
        }

        // Find min and max values
        double maxNum = double.MinValue;
        double minNum = double.MaxValue;

        for (int i = 0; i < _dataLines.Count && i < 10; i++)
        {
            if (_drawDataLines[i])
            {
                for (int j = 1; j <= maxElements; j++)
                {
                    if (!string.IsNullOrEmpty(dataSeries[i, j]) && double.TryParse(dataSeries[i, j], out double val))
                    {
                        if (val > maxNum) maxNum = val;
                        if (val < minNum) minNum = val;
                    }
                }
            }
        }

        if (maxNum == minNum)
            return;

        // Calculate drawing parameters
        double range = maxNum - minNum;
        double chartHeight = Bounds.Height - _topMargin - _bottomMargin;
        double divisor = range / chartHeight;
        double stepVal = (Bounds.Width - (_sideMargin * 4)) / maxElements;

        // Draw chart background
        if (_clearChartArea)
        {
            DrawChartBackground(context, stepVal);
        }

        // Draw axes
        DrawAxes(context, stepVal, maxElements, dataSeries);

        // Draw Y axis values
        if (_drawYAxisValues)
        {
            RenderYAxisValues(context, minNum, maxNum, divisor, stepVal);
        }

        // Draw data lines
        DrawDataLines(context, dataSeries, maxElements, minNum, divisor, stepVal);

        // Draw title
        DrawTitle(context);
    }

    private void DrawChartBackground(DrawingContext context, double stepVal)
    {
        // Chart background starts at the Y-axis line position
        double x = (_sideMargin * 3) + stepVal;
        double y = Bounds.Height - _bottomMargin;

        if (_drawAlternateChartBackground)
        {
            int sizy = (int)((Bounds.Height - _bottomMargin - _topMargin) / 10);
            for (int i = 0; i < 10; i++)
            {
                double yPos = _topMargin + (sizy * i);
                var brush = i % 2 == 0 ? new SolidColorBrush(_chartBackColor) : new SolidColorBrush(_chartAlternateBackColor);
                context.FillRectangle(brush, new Rect(x, yPos, (Bounds.Width - _sideMargin) - x, sizy));
            }
        }
        else
        {
            context.FillRectangle(new SolidColorBrush(_chartBackColor),
                new Rect(x, _topMargin, (Bounds.Width - _sideMargin) - x, Bounds.Height - _bottomMargin - _topMargin));
        }
    }

    private void DrawAxes(DrawingContext context, double stepVal, int maxElements, string[,] dataSeries)
    {
        var axisPen = new Pen(new SolidColorBrush(_axisDrawColor), _axisLineWeight);

        var xAxisLabels = _xAxisLabel.Split(',');
        double x = (_sideMargin * 3) + stepVal;
        double y = Bounds.Height - _bottomMargin;

        // Y axis
        context.DrawLine(axisPen, new Point(x, y), new Point(x, _topMargin));
        // X axis
        context.DrawLine(axisPen, new Point(x, y), new Point(Bounds.Width - _sideMargin, y));

        // X axis tick marks and labels
        for (int i = 1; i <= maxElements; i++)
        {
            double xPos = ((i - 1) * stepVal) + (_sideMargin * 3) + stepVal;
            context.DrawLine(axisPen, new Point(xPos, y), new Point(xPos, y + 12));

            if (xAxisLabels.Length > i)
            {
                RenderTitleBelow(context, xAxisLabels[i], xPos, y);
            }
        }

        // Draw interval lines if enabled
        if (_drawIntervalDataSeriesIntersects)
        {
            var intervalPen = new Pen(new SolidColorBrush(_axisDrawColor), 1);
            for (int i = 1; i <= maxElements; i++)
            {
                double xPos = ((i - 1) * stepVal) + (_sideMargin * 3) + stepVal;
                context.DrawLine(intervalPen, new Point(xPos, y), new Point(xPos, _topMargin));
            }
        }

        // Draw X axis label
        if (xAxisLabels.Length > 0)
        {
            // Position the X-axis label lower to avoid overlapping with tick labels
            RenderAxisLabelTitleBelow(context, xAxisLabels[0], (((Bounds.Width - _sideMargin) - x) / 2) + x, Bounds.Height - (_bottomMargin / 2));
        }
    }

    private void RenderYAxisValues(DrawingContext context, double minNum, double maxNum, double divisor, double stepVal)
    {
        // Position Y-axis numeric values at the leftmost edge of the chart
        // Y-axis line is at (_sideMargin * 3) + stepVal, so align values just before it
        double yAxisLineX = (_sideMargin * 3) + stepVal;
        double yAxisValueRightEdge = yAxisLineX - 5;  // 5 pixels before the Y-axis line
        int step = Math.Max((int)((maxNum - minNum) / 10), 1);

        for (int value = (int)minNum; value <= (int)maxNum; value += step)
        {
            double y = Bounds.Height - _bottomMargin - ((value - minNum) / divisor);
            // Position Y-axis values right-aligned at the left edge of the chart
            RenderYAxisValue(context, value.ToString(), yAxisValueRightEdge, y);
        }
    }

    private void DrawDataLines(DrawingContext context, string[,] dataSeries, int maxElements, double minNum, double divisor, double stepVal)
    {
        for (int seriesIdx = 0; seriesIdx < _dataLines!.Count && seriesIdx < 10; seriesIdx++)
        {
            if (!_drawDataLines[seriesIdx])
                continue;

            var pen = new Pen(new SolidColorBrush(_dataLineColors[seriesIdx]), _drawLineWeight);
            var outlinePen = new Pen(new SolidColorBrush(Colors.Black), _drawLineWeight + 2);

            double lastX = -1, lastY = -1;

            for (int i = 1; i <= maxElements; i++)
            {
                if (!string.IsNullOrEmpty(dataSeries[seriesIdx, i]) && double.TryParse(dataSeries[seriesIdx, i], out double val))
                {
                    double x = ((i - 1) * stepVal) + (_sideMargin * 3) + stepVal;
                    double y = Bounds.Height - _bottomMargin - ((val - minNum) / divisor);

                    // Clamp y value to ensure it stays within bounds
                    if (y < _topMargin)
                        y = _topMargin;
                    else if (y > Bounds.Height - _bottomMargin)
                        y = Bounds.Height - _bottomMargin;

                    if (lastX == -1 && lastY == -1)
                    {
                        // First point - draw label
                        if (!string.IsNullOrEmpty(dataSeries[seriesIdx, 0]))
                        {
                            RenderSeriesLabel(context, dataSeries[seriesIdx, 0], x, y);
                        }
                        lastX = x;
                        lastY = y;
                    }
                    else
                    {
                        // Draw line segment
                        if (_outlineDataSeries)
                        {
                            context.DrawLine(outlinePen, new Point(lastX, lastY), new Point(x, y));
                        }
                        context.DrawLine(pen, new Point(lastX, lastY), new Point(x, y));
                        lastX = x;
                        lastY = y;
                    }

                    // Draw knot
                    if (_drawKnots)
                    {
                        double knotSize = _drawLineWeight + 4;
                        if (_outlineDataSeries)
                        {
                            context.FillRectangle(new SolidColorBrush(Colors.Black),
                                new Rect(x - ((_drawLineWeight + 6) / 2), y - ((_drawLineWeight + 6) / 2), _drawLineWeight + 6, _drawLineWeight + 6),
                                (float)((_drawLineWeight + 6) / 2));
                        }
                        context.FillRectangle(new SolidColorBrush(_dataLineColors[seriesIdx]),
                            new Rect(x - (knotSize / 2), y - (knotSize / 2), knotSize, knotSize),
                            (float)(knotSize / 2));
                    }
                }
            }

            // Draw trend line if enabled
            if (_drawRegressionTrendLines)
            {
                DrawTrendLine(context, dataSeries, seriesIdx, maxElements, minNum, divisor, stepVal);
            }
        }
    }

    private void DrawTrendLine(DrawingContext context, string[,] dataSeries, int seriesIdx, int maxElements, double minNum, double divisor, double stepVal)
    {
        var reg = new Regressor { Degree = _trendLineDegreeValue };

        for (int i = 1; i <= maxElements; i++)
        {
            if (!string.IsNullOrEmpty(dataSeries[seriesIdx, i]) && double.TryParse(dataSeries[seriesIdx, i], out double val))
            {
                reg.XYAdd(i, val);
            }
        }

        var trendPen = new Pen(new SolidColorBrush(_dataLineColors[seriesIdx]), _trendLineWeight);
        double rlastX = -1, rlastY = -1;

        for (int i = 1; i <= maxElements; i++)
        {
            if (!string.IsNullOrEmpty(dataSeries[seriesIdx, i]))
            {
                double x = ((i - 1) * stepVal) + (_sideMargin * 3) + stepVal;
                double y = Bounds.Height - _bottomMargin - ((reg.RegVal(i) - minNum) / divisor);

                // Clamp y value
                if (y > Bounds.Height - _bottomMargin)
                    y = Bounds.Height - _bottomMargin;
                else if (y < _topMargin)
                    y = _topMargin;

                if (rlastX == -1 && rlastY == -1)
                {
                    rlastX = x;
                    rlastY = y;
                }
                else
                {
                    context.DrawLine(trendPen, new Point(rlastX, rlastY), new Point(x, y));
                    rlastX = x;
                    rlastY = y;
                }
            }
        }
    }

    private void DrawTitle(DrawingContext context)
    {
        if (string.IsNullOrEmpty(_chartTitle))
            return;

        var formattedText = new FormattedText(
            _chartTitle,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _chartTitleFont,
            _chartTitleFontSize,
            new SolidColorBrush(_chartTitleColor));

        double x = (Bounds.Width - formattedText.Width) / 2;
        double y = 5;

        context.DrawText(formattedText, new Point(x, y));
    }

    private void RenderSeriesLabel(DrawingContext context, string text, double x, double y)
    {
        var formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _axisFont,
            _axisFontSize,
            new SolidColorBrush(Colors.Black));

        // Position series labels further left to avoid overlapping with Y-axis numeric values
        context.DrawText(formattedText, new Point(x - formattedText.Width - 20, y - formattedText.Height / 2));
    }

    private void RenderYAxisValue(DrawingContext context, string text, double x, double y)
    {
        var formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _yAxisFont,
            _yAxisFontSize,
            new SolidColorBrush(Colors.Black));

        // Right-align the text - x is the right edge position
        context.DrawText(formattedText, new Point(x - formattedText.Width, y - formattedText.Height / 2));
    }

    private void RenderTitleBelow(DrawingContext context, string text, double x, double y)
    {
        var formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _xAxisFont,
            _xAxisFontSize,
            new SolidColorBrush(Colors.Black));

        context.DrawText(formattedText, new Point(x - formattedText.Width / 2, y + 15));
    }

    private void RenderAxisLabelTitleBelow(DrawingContext context, string text, double x, double y)
    {
        var formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _axisLabelFont,
            _axisLabelFontSize,
            new SolidColorBrush(Colors.Black));

        context.DrawText(formattedText, new Point(x - formattedText.Width / 2, y));
    }

    #endregion

    #region Event Handlers

    private void FlashTimer_Tick(object? sender, EventArgs e)
    {
        if (_flashDataLine > -1 && _flashDataLine < 10)
        {
            System.Diagnostics.Debug.WriteLine($"FlashTimer_Tick - Line {_flashDataLine}, Current Color: {_dataLineColors[_flashDataLine]} (ToUInt32: {_dataLineColors[_flashDataLine].ToUInt32()})");

            // Use ToUInt32 for more reliable color comparison
            if (_dataLineColors[_flashDataLine].ToUInt32() == _flashSeriesColor.ToUInt32())
            {
                _dataLineColors[_flashDataLine] = _oldDataLineColors[_flashDataLine];
                System.Diagnostics.Debug.WriteLine($"  -> Changed to: {_dataLineColors[_flashDataLine]}");
            }
            else
            {
                _dataLineColors[_flashDataLine] = _flashSeriesColor;
                System.Diagnostics.Debug.WriteLine($"  -> Changed to: {_flashSeriesColor}");
            }
            InvalidateVisual();
        }
    }

    #endregion
}
