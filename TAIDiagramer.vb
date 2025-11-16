' History
' Dec 12, 2003  Initial Version
'
' Dec 15, 2003  Made the thing do a simple design time graph render
'               so in design mode you can play with lineweights, and antialias settings, andother things
'               to examine the affect on the run time mode of the control
'
' Dec 17, 2003  Added some functionality to label the Bottom Axis, Added a new font for the Bottom Axis Lable
'               Added some optimization items
'
' Dec 31, 2003  Added CanvasImage property readonly returns an Image construct the is the rendered Chart
'               useful for printing applications
'               Added DrawKnots property. Will draw Circular Knots at each datapoint in Lineweight+6 or Lineweight+4 
'               when outlineing is turned off
'               Added Index property to _dataLineColors you can now set your own colors on each data line
'               Added FlashDataLine property. Set to any integer between 0 and 9 and the resulting displayed data series will
'               alternate between chosen color and blagc every 1/2 second.
'               Added a click event that returns the data series selected as an integer. Can be fed back to the FlashDataLine 
'               property to highlite a selected data series.
'
' Jan 02, 2004  Added System.Componentmodel.Description Strings to all property interfaces for the control
'               to better integrate with the IDE and prepare the item for sale externally
'
' Jan 28, 2004  Added New functionality
'               For Y axis Lables ( Auto ranged Values )
'               For Canvas alternate coloration ( For green bar paper effects )
'               For X Axis Lines at interval points to assist users in lineing up data with intervals
'               For Trendlines (On and Off, Linear or Non Linear, Weight)
'               For turning a given data series on or off
'               New Sub Class for Regression analsys in support of Trendlines
'               Usage:
'                   Dim reg as new Regressor
'                   reg.Degree = {some value 1 = linear > 1 = Non Linear}
'                   For Bla bla bla
'                       reg.XYAdd(x,y)      ' where x is some value and y is the thing you want to trend
'                   Next
'
'                   ' Get results by 
'                   NewY = reg.RegVal(x)    ' where x is the x value used to hand in a given npoint at the first place
'
' Apr 27, 2004  Added some code to the show title to help it display multiline titles better
'                   
'
' TAIDiagramer Object
'   Generates Line charts based on a simple interface
'
'   Sample Usage
'       ' will add 10 lines of data to the the chart and redraw that chart
'
'       TAId.AddDataSeries("Data Series1,100,100,200,200,300,350,400,290,280,190,180,90,80")
'       TAId.AddDataSeries("Data Series2,-50,100,220,240,150,350,410,320,300,240,200,140,100")
'       TAId.AddDataSeries("Data Series3,60,110,75,180,300,350,415,300,310,200,210,100,110")
'       TAId.AddDataSeries("Data Series4,-100,30,90,90,300,310,380,110,120,210,220,310,320")
'       TAId.AddDataSeries("Data Series5,-80,105,130,40,300,330,405,305,295,205,195,105,95")
'       TAId.AddDataSeries("Data Series6,-10,105,130,40,300,330,405,305,295,205,195,105,95")
'       TAId.AddDataSeries("Data Series7,-180,-105,-130,-40,300,330,405,305,295,205,195,105,95")
'       TAId.AddDataSeries("Data Series8,300,250,300,250,300,250,300,250,300,250,300,250,300")
'       TAId.AddDataSeries("Data Series9,200,105,130,40,100,100,50,305,300,370,380,390,400")
'       TAId.AddDataSeries("Data Series10,400,300,350,200,200,100,105,100,80,-10,35,100,95")
'       TAId.Refresh()
'

Public Class TAIDiagramer
    Inherits System.Windows.Forms.UserControl

#Region " Defined storage "

    Private _canvaswidth As Integer = 500
    Private _canvasheight As Integer = 500

    Private _BottomMargin As Integer = 90
    Private _SideMargin As Integer = 25
    Private _TopMargin As Integer = 40
    Private _ChartTitle As String = "Sample Title"
    Private _ChartTitleFont As System.Drawing.Font = New System.Drawing.Font("Arial", 15, FontStyle.Bold, GraphicsUnit.Point)
    Private _ChartTitleColor As System.Drawing.Color = Color.Black

    Private _AxisFont As System.Drawing.Font = New System.Drawing.Font("Arial", 12, FontStyle.Bold, GraphicsUnit.Point)
    Private _YAxisFont As System.Drawing.Font = New System.Drawing.Font("Arial", 8, FontStyle.Regular, GraphicsUnit.Point)
    Private _XAxisFont As System.Drawing.Font = New System.Drawing.Font("Arial", 8, FontStyle.Regular, GraphicsUnit.Point)
    Private _AxisLableFont As System.Drawing.Font = New System.Drawing.Font("Arial", 14, FontStyle.Bold, GraphicsUnit.Point)
    Private _AxisDrawColor As System.Drawing.Color = Color.Black
    Private _DrawKnots As Boolean = True
    Private _DrawYaxisValues As Boolean = True
    Private _DrawRegressionTrendLines As Boolean = True
    Private _DrawAlternateChartBackground As Boolean = True
    Private _DrawIntervalDataSeriesIntersects As Boolean = True

    Private _XAxisLable As String = ""
    Private _YAxisLabel As String = ""

    Private _AxisLineWeight As Single = 4
    Private _Antialias As Boolean = True
    Private _ClearChartArea As Boolean = True
    Private _OutlineDataSeries As Boolean = True
    Private _ChartGrid As Boolean = True
    Private _ChartGridGranularity As Integer = 10
    Private _ChartGridYLables As Boolean = False

    Private _TrendLineDegreeValue As Integer = 2
    Private _TrendLineWeight As Integer = 2

    Private _DrawLineWeight As Single = 4
    Private _DrawLineStyle As Drawing2D.DashStyle = Drawing2D.DashStyle.Dash

    Private _ShowGrid As Boolean = True
    Private _ShowBorder As Boolean = True

    Private _CanvasBackColor As System.Drawing.Color = Color.AntiqueWhite
    Private _ChartBackcolor As System.Drawing.Color = Color.FloralWhite
    Private _ChartAlternateBackColor As System.Drawing.Color = Color.Honeydew

    Private _GridColor As System.Drawing.Color = Color.Gray
    Private _DataLines() As String
    Private _DataLineColors(10) As System.Drawing.Color
    Private _OldDataLineColors(10) As System.Drawing.Color
    Private _DrawDataLines(10) As Boolean
    Private _FlashDataLine As Integer = -1

    Private _DesignModeTrackContainerSize As Boolean = True
    Private _RunModeTrackContainerSize As Boolean = True

    Private _GridSize As Integer = 20
    Private _painting As Boolean = False

    Private _image As System.Drawing.Image

    <System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")> _
    Private Shared Function BitBlt( _
            ByVal hdcDest As IntPtr, _
            ByVal nXDest As Integer, _
            ByVal nYDest As Integer, _
            ByVal nWidth As Integer, _
            ByVal nHeight As Integer, _
            ByVal hdcSrc As IntPtr, _
            ByVal nXSrc As Integer, _
            ByVal nYSrc As Integer, _
            ByVal dwRop As System.Int32) As Boolean
    End Function

#End Region

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.TAIDiagramerCanvas.Width = _canvaswidth
        Me.TAIDiagramerCanvas.Height = _canvasheight

        Me._DataLineColors(0) = Color.Blue
        Me._DataLineColors(1) = Color.Red
        Me._DataLineColors(2) = Color.Green
        Me._DataLineColors(3) = Color.Purple
        Me._DataLineColors(4) = Color.Cyan
        Me._DataLineColors(5) = Color.Magenta
        Me._DataLineColors(6) = Color.Yellow
        Me._DataLineColors(7) = Color.ForestGreen
        Me._DataLineColors(8) = Color.DeepPink
        Me._DataLineColors(9) = Color.Gray

        Me._OldDataLineColors(0) = Color.Blue
        Me._OldDataLineColors(1) = Color.Red
        Me._OldDataLineColors(2) = Color.Green
        Me._OldDataLineColors(3) = Color.Purple
        Me._OldDataLineColors(4) = Color.Cyan
        Me._OldDataLineColors(5) = Color.Magenta
        Me._OldDataLineColors(6) = Color.Yellow
        Me._OldDataLineColors(7) = Color.ForestGreen
        Me._OldDataLineColors(8) = Color.DeepPink
        Me._OldDataLineColors(9) = Color.Gray

        Me._DrawDataLines(0) = True
        Me._DrawDataLines(1) = True
        Me._DrawDataLines(2) = True
        Me._DrawDataLines(3) = True
        Me._DrawDataLines(4) = True
        Me._DrawDataLines(5) = True
        Me._DrawDataLines(6) = True
        Me._DrawDataLines(7) = True
        Me._DrawDataLines(8) = True
        Me._DrawDataLines(9) = True

        Me.Refresh()

    End Sub

    'UserControl1 overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents TAIdiagramerPanel As System.Windows.Forms.Panel
    Friend WithEvents TAIDiagramerCanvas As System.Windows.Forms.PictureBox
    Friend WithEvents tim As System.Windows.Forms.Timer
    Friend WithEvents mnu As System.Windows.Forms.ContextMenu
    Friend WithEvents MenuItem1 As System.Windows.Forms.MenuItem
    Friend WithEvents MenuItem2 As System.Windows.Forms.MenuItem
    Friend WithEvents DesignTimer As System.Windows.Forms.Timer
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Me.TAIdiagramerPanel = New System.Windows.Forms.Panel
        Me.TAIDiagramerCanvas = New System.Windows.Forms.PictureBox
        Me.tim = New System.Windows.Forms.Timer(Me.components)
        Me.mnu = New System.Windows.Forms.ContextMenu
        Me.MenuItem1 = New System.Windows.Forms.MenuItem
        Me.MenuItem2 = New System.Windows.Forms.MenuItem
        Me.DesignTimer = New System.Windows.Forms.Timer(Me.components)
        Me.TAIdiagramerPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'TAIdiagramerPanel
        '
        Me.TAIdiagramerPanel.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TAIdiagramerPanel.AutoScroll = True
        Me.TAIdiagramerPanel.Controls.Add(Me.TAIDiagramerCanvas)
        Me.TAIdiagramerPanel.Location = New System.Drawing.Point(0, 0)
        Me.TAIdiagramerPanel.Name = "TAIdiagramerPanel"
        Me.TAIdiagramerPanel.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.TAIdiagramerPanel.Size = New System.Drawing.Size(500, 500)
        Me.TAIdiagramerPanel.TabIndex = 0
        '
        'TAIDiagramerCanvas
        '
        Me.TAIDiagramerCanvas.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TAIDiagramerCanvas.BackColor = System.Drawing.Color.AntiqueWhite
        Me.TAIDiagramerCanvas.Location = New System.Drawing.Point(0, 0)
        Me.TAIDiagramerCanvas.Name = "TAIDiagramerCanvas"
        Me.TAIDiagramerCanvas.Size = New System.Drawing.Size(500, 500)
        Me.TAIDiagramerCanvas.TabIndex = 0
        Me.TAIDiagramerCanvas.TabStop = False
        '
        'tim
        '
        Me.tim.Interval = 500
        '
        'mnu
        '
        Me.mnu.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.MenuItem1, Me.MenuItem2})
        '
        'MenuItem1
        '
        Me.MenuItem1.Index = 0
        Me.MenuItem1.Text = "Hide This Series"
        '
        'MenuItem2
        '
        Me.MenuItem2.Index = 1
        Me.MenuItem2.Text = "SHow All Series"
        '
        'DesignTimer
        '
        Me.DesignTimer.Interval = 1000
        '
        'TAIDiagramer
        '
        Me.Controls.Add(Me.TAIdiagramerPanel)
        Me.Name = "TAIDiagramer"
        Me.RightToLeft = System.Windows.Forms.RightToLeft.Yes
        Me.Size = New System.Drawing.Size(504, 504)
        Me.TAIdiagramerPanel.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region " Event Templates "

    Public Event SeriesSelected(ByVal sender As Object, ByVal Series As Integer)

#End Region

#Region " Properties "

    '   AntiAlias
    <System.ComponentModel.Description("Turn on or off the Antialiasing or smoothing of the rendered data series and texts")> _
    Public Property AntiAlias() As Boolean
        Get
            Return _Antialias
        End Get
        Set(ByVal Value As Boolean)
            _Antialias = Value
            Me.Refresh()
        End Set
    End Property

    '   AxisDrawColor
    <System.ComponentModel.Description("What color to draw the Axis in")> _
    Public Property AxisDrawColor() As System.Drawing.Color
        Get
            Return _AxisDrawColor
        End Get
        Set(ByVal Value As System.Drawing.Color)
            _AxisDrawColor = Value
            Me.Refresh()
        End Set
    End Property

    '   AxisFont
    <System.ComponentModel.Description("What Font will be used to for the various Axis lables (Data series and Intervals)")> _
    Public Property AxisFont() As System.Drawing.Font
        Get
            Return _AxisFont
        End Get
        Set(ByVal Value As System.Drawing.Font)
            _AxisFont = Value
            Me.Refresh()
        End Set
    End Property

    '   YAxisFont
    <System.ComponentModel.Description("What Font will be used to for Y axis value Lables")> _
    Public Property YAxisFont() As System.Drawing.Font
        Get
            Return _YAxisFont
        End Get
        Set(ByVal Value As System.Drawing.Font)
            _YAxisFont = Value
            Me.Refresh()
        End Set
    End Property

    '   AxisLableFont
    <System.ComponentModel.Description("What Font is used to lable the Lower X axis as a whole. ( Not the individual intervals but the Label assigned to the whole thing")> _
    Public Property AxisLableFont() As System.Drawing.Font
        Get
            Return _AxisLableFont
        End Get
        Set(ByVal Value As System.Drawing.Font)
            _AxisLableFont = Value
            Me.Refresh()
        End Set
    End Property

    '   AxisLineWeight
    <System.ComponentModel.Description("Sets the size of Weight of the rendered Axis")> _
    Public Property AxisLineWeight() As Single
        Get
            Return _AxisLineWeight
        End Get
        Set(ByVal Value As Single)
            _AxisLineWeight = Value
            Me.Refresh()
        End Set
    End Property

    '   CanvasBackColor
    <System.ComponentModel.Description("What color the canvas area will be drawn in")> _
    Public Property CanvasBackColor() As System.Drawing.Color
        Get
            Return _CanvasBackColor
        End Get
        Set(ByVal Value As System.Drawing.Color)
            _CanvasBackColor = Value
            Me.Refresh()
        End Set
    End Property

    '   CanvasHeight
    <System.ComponentModel.Description("How Tall the rendered canvas area is in Pixels")> _
    Public Property CanvasHeight() As Integer
        Get
            Return _canvasheight
        End Get
        Set(ByVal Value As Integer)
            _canvasheight = Value
            Me.TAIDiagramerCanvas.Height = _canvasheight
            Me.Refresh()
        End Set
    End Property

    '   TrendLineDegreeValue
    <System.ComponentModel.Description("Set to 1 for Linear Trend Lines and >1 for Degree of Polynomial Trend Lines")> _
    Public Property TrendLineDegreeValue() As Integer
        Get
            Return _TrendLineDegreeValue
        End Get
        Set(ByVal Value As Integer)
            _TrendLineDegreeValue = Value
            Me.Refresh()
        End Set
    End Property

    '   CanvasImage
    <System.ComponentModel.Description("Read Only Property Returns a Bitmap Image of the current canvas render. Useful for printing applications")> _
    Public ReadOnly Property CanvasImage() As System.Drawing.Image
        Get
            Return _image
        End Get
    End Property

    '   CanvasWidth 
    <System.ComponentModel.Description("How wide the rendered canvas area is in Pixels")> _
    Public Property CanvasWidth() As Integer
        Get
            Return _canvaswidth
        End Get
        Set(ByVal Value As Integer)
            _canvaswidth = Value
            Me.TAIDiagramerCanvas.Width = _canvaswidth
            Me.Refresh()
        End Set
    End Property

    '   ChartBackColor
    <System.ComponentModel.Description("What color the Chart area will be before the data series are rendered into it")> _
    Public Property ChartBackColor() As System.Drawing.Color
        Get
            Return _ChartBackcolor
        End Get
        Set(ByVal Value As System.Drawing.Color)
            _ChartBackcolor = Value
            Me.Refresh()
        End Set
    End Property

    '   ChartAlternateBackColor
    <System.ComponentModel.Description("What Alternate color the Chart area will be before the data series are rendered into it")> _
    Public Property ChartAlternateBackColor() As System.Drawing.Color
        Get
            Return _ChartAlternateBackColor
        End Get
        Set(ByVal Value As System.Drawing.Color)
            _ChartAlternateBackColor = Value
            Me.Refresh()
        End Set
    End Property

    '   ChartGrid
    <System.ComponentModel.Description("Turn on or off the drawing of the internal chart grid. Uses the ChartGridGranularity setting to determine how many subdivisions to draw")> _
    Public Property ChartGrid() As Boolean
        Get
            Return _ChartGrid
        End Get
        Set(ByVal Value As Boolean)
            _ChartGrid = Value
            Me.Refresh()
        End Set
    End Property

    '   ChartGridGranularity
    <System.ComponentModel.Description("How many subdivisions in the Y axis to draw inside the chart area if ChartGrid=True")> _
    Public Property ChartGridGranularity() As Integer
        Get
            Return _ChartGridGranularity
        End Get
        Set(ByVal Value As Integer)
            _ChartGridGranularity = Value
            Me.Refresh()
        End Set
    End Property

    '   ChartGridYLables
    <System.ComponentModel.Description("Label individual sections along the Y axis if the ChartGrid is ChartGrid=True")> _
    Public Property ChartGridYLables() As Boolean
        Get
            Return _ChartGridYLables
        End Get
        Set(ByVal Value As Boolean)
            _ChartGridYLables = Value
            Me.Refresh()
        End Set
    End Property

    '   ChartTitle
    <System.ComponentModel.Description("The string to draw as the chart title")> _
    Public Property ChartTitle() As String
        Get
            Return _ChartTitle
        End Get
        Set(ByVal Value As String)
            _ChartTitle = Value
            Me.Refresh()
        End Set
    End Property

    '   ChartTitleColor
    <System.ComponentModel.Description("What color to render the chart title in")> _
    Public Property ChartTitleColor() As System.Drawing.Color
        Get
            Return _ChartTitleColor
        End Get
        Set(ByVal Value As System.Drawing.Color)
            _ChartTitleColor = Value
            Me.Refresh()
        End Set
    End Property

    '   ChartTitleFont
    <System.ComponentModel.Description("What font is used to draw the chart title")> _
    Public Property ChartTitleFont() As System.Drawing.Font
        Get
            Return _ChartTitleFont
        End Get
        Set(ByVal Value As System.Drawing.Font)
            _ChartTitleFont = Value
            Me.Refresh()
        End Set
    End Property

    '   ClearChartArea
    <System.ComponentModel.Description("Turns on or off the clearing of the chart area to the chart background color")> _
    Public Property ClearChartArea() As Boolean
        Get
            Return _ClearChartArea
        End Get
        Set(ByVal Value As Boolean)
            _ClearChartArea = Value
            Me.Refresh()
        End Set
    End Property

    '   FlashDataLine
    <System.ComponentModel.Description("Will start to flash a given data series number 0 based. Set to -1 to stop the highlite")> _
    Public Property FlashDataLine() As Integer
        Get
            Return _FlashDataLine
        End Get
        Set(ByVal Value As Integer)

            If _FlashDataLine <> -1 And _FlashDataLine < 10 Then
                _DataLineColors(_FlashDataLine) = _OldDataLineColors(_FlashDataLine)
            End If

            If Value < 10 And Value > -1 Then
                tim.Enabled = True
                _FlashDataLine = Value
            Else
                tim.Enabled = False

            End If

            Me.Refresh()

        End Set
    End Property

    '   DataLineColor
    <System.ComponentModel.Description("Indexed property allows to setting of the rendition color for a given data series shifting from the default coloration used by the contol")> _
    Public Property DataLineColor(ByVal index As Integer) As System.Drawing.Color
        Get
            If index > 10 Then
                Return Color.Black
            Else
                Return _DataLineColors(index - 1)
            End If
        End Get
        Set(ByVal Value As System.Drawing.Color)
            If index > 10 Then
                Exit Property
            Else
                _DataLineColors(index - 1) = Value
                _OldDataLineColors(index - 1) = Value
                Me.Refresh()
            End If
        End Set
    End Property

    '   DrawKnots
    <System.ComponentModel.Description("Will draw circular KNOTs at each data point on being True")> _
    Public Property DrawKnots() As Boolean
        Get
            Return _DrawKnots
        End Get
        Set(ByVal Value As Boolean)
            _DrawKnots = Value
            Me.Refresh()
        End Set
    End Property

    '   DrawDataLines
    <System.ComponentModel.Description("Will draw a corresponding data series if one is present and this indexed property = true")> _
    Public Property DrawDataLines(ByVal index As Integer) As Boolean
        Get
            Return _DrawDataLines(index)
        End Get
        Set(ByVal Value As Boolean)

            Try
                _DrawDataLines(index) = Value
                Me.Refresh()
            Catch ex As Exception
                MsgBox(ex.Message)

            End Try

        End Set
    End Property

    '   DrawIntervalDataSeriesIntersects
    <System.ComponentModel.Description("Will draw lines across the chart area at the intervals to help user line up datapoints on the X axis when = True")> _
    Public Property DrawIntervalDataSeriesIntersects() As Boolean
        Get
            Return _DrawIntervalDataSeriesIntersects
        End Get
        Set(ByVal Value As Boolean)
            _DrawIntervalDataSeriesIntersects = Value
            Me.Refresh()
        End Set
    End Property

    '   DrawAlternateChartBackground
    <System.ComponentModel.Description("Will draw a striped chart backdrop on True")> _
    Public Property DrawAlternateChartBackground() As Boolean
        Get
            Return _DrawAlternateChartBackground
        End Get
        Set(ByVal Value As Boolean)
            _DrawAlternateChartBackground = Value
            Me.Refresh()
        End Set
    End Property

    '   DrawYAxisValues
    <System.ComponentModel.Description("Will draw the values assiciated with y axis amplitude on TRUE")> _
    Public Property DrawYAxisValues() As Boolean
        Get
            Return _DrawYaxisValues
        End Get
        Set(ByVal Value As Boolean)
            _DrawYaxisValues = Value
            Me.Refresh()
        End Set
    End Property

    '   DrawRegressionTrendlines
    <System.ComponentModel.Description("Will draw the Parabolic Regression (Non linear) trandlines for all dataseries on True")> _
    Public Property DrawRegressionTrendlines() As Boolean
        Get
            Return _DrawRegressionTrendLines
        End Get
        Set(ByVal Value As Boolean)
            _DrawRegressionTrendLines = Value
            Me.Refresh()
        End Set
    End Property

    ' DesignModeTrackContainerSize
    Public Property DesignModeTrackContainerSize() As Boolean
        Get
            Return _DesignModeTrackContainerSize
        End Get
        Set(ByVal Value As Boolean)
            _DesignModeTrackContainerSize = Value
            Me.Refresh()
        End Set
    End Property

    '   RunModeTrackContainerSize
    Public Property RunModeTrackContainerSize() As Boolean
        Get
            Return _RunModeTrackContainerSize
        End Get
        Set(ByVal Value As Boolean)
            _RunModeTrackContainerSize = Value
            Me.Refresh()
        End Set
    End Property

    '   ShowGrid
    <System.ComponentModel.Description("Will turn on or off the trawing of the Grid in the background of the control")> _
    Public Property ShowGrid() As Boolean
        Get
            Return _ShowGrid
        End Get
        Set(ByVal Value As Boolean)
            _ShowGrid = Value
            Me.Refresh()
        End Set
    End Property

    '   OutlineDataSeries
    <System.ComponentModel.Description("Turn on or off the outlining of all data series drawings in Black")> _
    Public Property OutlineDataSeries() As Boolean
        Get
            Return _OutlineDataSeries
        End Get
        Set(ByVal Value As Boolean)
            _OutlineDataSeries = Value
            Me.Refresh()
        End Set
    End Property

    '   ShowBorder
    <System.ComponentModel.Description("Turn on or off the outlining of the canvas area")> _
    Public Property ShowBorder() As Boolean
        Get
            Return _ShowBorder
        End Get
        Set(ByVal Value As Boolean)
            _ShowBorder = Value
            Me.Refresh()
        End Set
    End Property

    '   DrawLineWeight
    <System.ComponentModel.Description("Sets the size or Weight of the rendered lines on the supplied data series")> _
    Public Property DrawLineWeight() As Single
        Get
            Return _DrawLineWeight
        End Get
        Set(ByVal Value As Single)
            _DrawLineWeight = Value
            Me.Refresh()
        End Set
    End Property

    '   GridSize
    <System.ComponentModel.Description("How large the rendered grid will be in Pixels")> _
    Public Property GridSize() As Integer
        Get
            Return _GridSize
        End Get
        Set(ByVal Value As Integer)
            _GridSize = Value
            Me.Refresh()
        End Set
    End Property

    '   TrendLineWeight
    <System.ComponentModel.Description("How Fat the TrendLines drawn will be in Pixels")> _
    Public Property TrendLineWeight() As Integer
        Get
            Return _TrendLineWeight
        End Get
        Set(ByVal Value As Integer)
            _TrendLineWeight = Value
            Me.Refresh()
        End Set
    End Property

    '   GridColor
    <System.ComponentModel.Description("What color to draw the grid in")> _
    Public Property GridColor() As System.Drawing.Color
        Get
            Return _GridColor
        End Get
        Set(ByVal Value As System.Drawing.Color)
            _GridColor = Value
            Me.Refresh()
        End Set
    End Property

    '   DrawLineStyle
    <System.ComponentModel.Description("What rendering style will be used to render the data series")> _
    Public Property DrawLineStyle() As Drawing2D.DashStyle
        Get
            Return _DrawLineStyle
        End Get
        Set(ByVal Value As Drawing2D.DashStyle)
            _DrawLineStyle = Value
            Me.Refresh()
        End Set
    End Property

    '   XAxisLable
    <System.ComponentModel.Description("The Lable to place at the bottom of the chart. Includes the axis title and the title of each individual interval comma seperated.")> _
    Public Property XAxisLable() As String
        Get
            Return _XAxisLable
        End Get
        Set(ByVal Value As String)
            _XAxisLable = Value
            Me.Refresh()
        End Set
    End Property

#End Region

#Region " Private Routines "

    Private Sub RenderCircle(ByVal g As Graphics, ByVal x As Integer, ByVal y As Integer, ByVal s As Integer)
        Dim pn As New Pen(Color.Black)

        g.DrawEllipse(pn, x - (s \ 2), y - (s \ 2), s, s)

    End Sub

    Private Sub RenderDataSeries(ByVal g As Graphics)

        Dim t, tt, ttt, maxelements As Long
        Dim maxnum As Double = 0
        Dim minnum As Double = 0
        Dim flag As Boolean = True

        If _DataLines Is Nothing Then
            ' we have nothing to render so lets bail
            Exit Sub
        End If

        For t = 0 To _DataLines.GetUpperBound(0) - 1
            If _DrawDataLines(t) Then
                flag = False
                Exit For
            End If
        Next

        If flag Then
            ' we need to bail, because we are not drawing anything
            Exit Sub
        End If

        ' now for some range checking

        If (Me.Height - _TopMargin - _BottomMargin) < 10 Then
            ' we have no room to draw anything so lets bail
            Exit Sub
        End If

        ttt = 0
        maxelements = 0

        t = _DataLines.GetUpperBound(0)

        ' t now has how many lines we have available

        For tt = 0 To t - 1
            ttt = _DataLines(tt).Split(",").GetUpperBound(0)
            If ttt > maxelements Then
                maxelements = ttt
            End If
        Next

        If maxelements < 1 Then
            ' if we dont have any elements then bail
            Exit Sub
        End If

        ' we now know how many datapoints/series we have at maximum
        ' time to load up the array
        Dim DataSeries(t, maxelements + 1) As String
        Dim ds() As String

        For tt = 0 To t - 1
            ds = _DataLines(tt).Split(",")
            For ttt = 0 To ds.GetUpperBound(0)
                DataSeries(tt, ttt) = ds(ttt)
            Next
        Next

        ' now that the array is loaded lets actually draw the damn thing
        ' we need to figure out the maximum

        For tt = 0 To t - 1
            ' are we drawing this data series
            If _DrawDataLines(tt) Then
                For ttt = 1 To maxelements
                    If Val(DataSeries(tt, ttt)) > maxnum Then
                        maxnum = Val(DataSeries(tt, ttt))
                    End If

                    If Val(DataSeries(tt, ttt)) < minnum Then
                        minnum = Val(DataSeries(tt, ttt))
                    End If
                Next
            End If
        Next

        If maxnum = minnum Then
            ' if we have no range to plot then bail
            Exit Sub
        End If

        ' we now have our data range

        Dim range As Double = Math.Abs(maxnum - minnum)
        Dim divisor As Double = range / (Me.Height - _TopMargin - _BottomMargin)
        Dim stepval As Double = (Me.Width - (_SideMargin * 4)) / maxelements

        Dim lastx, lasty, x, y As Integer
        Dim rlastx, rlasty As Double

        ' here we will setup the drawing environment

        If _Antialias Then
            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias
        Else
            g.SmoothingMode = Drawing2D.SmoothingMode.Default
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault
        End If

        ' are we clearing the chart area
        If _ClearChartArea Then
            If _DrawAlternateChartBackground Then
                ' we need to draw an alternating backdrop in the chart area
                Dim sizy As Integer = (Me.Height - _BottomMargin - _TopMargin) / 10

                For tt = 0 To 9
                    x = (_SideMargin * 3) + stepval
                    y = _TopMargin + (sizy * tt)

                    If tt \ 2 = tt / 2 Then
                        ' we are even steven so lets do one thing
                        g.FillRectangle(New System.Drawing.SolidBrush(_ChartBackcolor), x, y, (Me.Width - _SideMargin) - x, sizy)

                    Else
                        g.FillRectangle(New System.Drawing.SolidBrush(_ChartAlternateBackColor), x, y, (Me.Width - _SideMargin) - x, sizy)
                    End If
                Next
            Else
                ' just draw the standard single color chart backdrop
                x = (_SideMargin * 3) + stepval
                y = Me.Height - _BottomMargin

                g.FillRectangle(New System.Drawing.SolidBrush(_ChartBackcolor), x, _TopMargin, (Me.Width - _SideMargin) - x, Me.Height - _BottomMargin - _TopMargin)
            End If
        End If

        ' here we will render the axis
        Dim apn As New Pen(_AxisDrawColor)
        apn.Width = _AxisLineWeight

        Dim xaxislables() As String = _XAxisLable.Split(",")

        x = (_SideMargin * 3) + stepval
        y = Me.Height - _BottomMargin

        g.DrawLine(apn, x, y, x, _TopMargin)                ' the Y axis
        g.DrawLine(apn, x, y, Me.Width - _SideMargin, y)    ' the X axis

        ' here we will loop and draw the xaxis tic marks and if necessary the tic mark lables
        For ttt = 1 To maxelements
            x = ((ttt - 1) * stepval) + (_SideMargin * 3) + stepval
            g.DrawLine(apn, x, y, x, y + 12)
            If Not (xaxislables Is Nothing) Then
                If ttt <= xaxislables.GetUpperBound(0) Then
                    RenderTitleBelow(xaxislables(ttt), g, x, y)
                End If
            End If
        Next

        ' are we drawing the interval lines across the x axis if so lets draw em here
        If _DrawIntervalDataSeriesIntersects Then
            apn.Width = 1

            For ttt = 1 To maxelements
                x = ((ttt - 1) * stepval) + (_SideMargin * 3) + stepval

                g.DrawLine(apn, x, y, x, _TopMargin)

            Next

            apn.Width = _AxisLineWeight

        End If

        ' do we have an xaxis lebleset is so we need to draw the first element of that set as the
        ' xaxis label

        If Not (xaxislables Is Nothing) Then
            x = (_SideMargin * 3) + stepval
            RenderAxisLableTitleBelow(xaxislables(0), g, (((Me.Width - _SideMargin) - x) / 2) + x, Me.Height - (_BottomMargin / 1.2))

        End If

        apn = Nothing

        x = 0
        y = 0
        lastx = -1
        lasty = -1
        rlastx = -1
        rlasty = -1

        ' here we will draw the y axis value numerics 

        ' are we being told to draw Y axis values?
        If _DrawYaxisValues Then
            ' yes then doit it man....
            x = (_SideMargin * 3) + stepval

            For tt = Int(minnum) To Int(maxnum) Step Math.Max(Int((maxnum - minnum) / 10), 1)
                y = Me.Height - _BottomMargin - ((tt + Math.Abs(minnum)) / divisor)
                RenderYAxisValue2TheRightof(Trim(Str(tt)), g, x, y)
            Next
        End If

        For tt = 0 To t - 1
            If _DrawDataLines(tt) Then
                Dim pn As New Pen(_DataLineColors(tt))
                pn.Width = _DrawLineWeight
                pn.DashStyle = _DrawLineStyle

                For ttt = 1 To maxelements
                    If Not (DataSeries(tt, ttt) Is Nothing) Then
                        x = ((ttt - 1) * stepval) + (_SideMargin * 3) + stepval
                        y = Me.Height - _BottomMargin - ((Val(DataSeries(tt, ttt)) + Math.Abs(minnum)) / divisor)

                        If lastx = -1 And lasty = -1 Then
                            RenderTitle2TheRightof(DataSeries(tt, ttt - 1), g, x, y)
                            lastx = x
                            lasty = y
                        Else
                            If _OutlineDataSeries Then
                                ' we need to outline what we are drawing
                                Dim olpn As New Pen(Color.Black)
                                olpn.Width = _DrawLineWeight + 2
                                olpn.DashStyle = _DrawLineStyle
                                g.DrawLine(olpn, lastx, lasty, x, y)
                                olpn = Nothing
                            End If
                            g.DrawLine(pn, lastx, lasty, x, y)

                            lastx = x
                            lasty = y
                        End If
                        If _DrawKnots Then
                            ' we need to render a Knot at the X,Y coord
                            If _OutlineDataSeries Then
                                ' we need to outline what we are drawing
                                g.FillEllipse(Brushes.Black, x - ((DrawLineWeight + 6) / 2), y - ((DrawLineWeight + 6) / 2), DrawLineWeight + 6, DrawLineWeight + 6)
                            End If
                            g.FillEllipse(New SolidBrush(_DataLineColors(tt)), x - ((DrawLineWeight + 4) / 2), y - ((DrawLineWeight + 4) / 2), DrawLineWeight + 4, DrawLineWeight + 4)
                        End If
                    End If

                Next

                ' are we drawing linear regression trendlines?
                If _DrawRegressionTrendLines Then
                    ' do some regression here
                    Dim reg As New Regressor
                    reg.Degree = _TrendLineDegreeValue

                    For ttt = 1 To maxelements
                        reg.XYAdd(ttt, Val(DataSeries(tt, ttt)))
                    Next

                    pn.Width = _TrendLineWeight

                    For ttt = 1 To maxelements
                        If Not (DataSeries(tt, ttt) Is Nothing) Then
                            x = ((ttt - 1) * stepval) + (_SideMargin * 3) + stepval
                            y = Me.Height - _BottomMargin - ((reg.RegVal(ttt) + Math.Abs(minnum)) / divisor)
                            ' do some range checking here

                            If y > Me.Height - _BottomMargin Then
                                y = Me.Height - _BottomMargin
                            Else
                                If y < _TopMargin Then y = _TopMargin
                            End If

                            If rlastx = -1 And rlasty = -1 Then
                                'RenderTitle2TheRightof(DataSeries(tt, ttt - 1), g, x, y)
                                rlastx = x
                                rlasty = y
                            Else
                                'If _OutlineDataSeries Then
                                '    ' we need to outline what we are drawing
                                '    Dim olpn As New Pen(Color.Black)
                                '    olpn.Width = _DrawLineWeight + 2
                                '    olpn.DashStyle = _DrawLineStyle
                                '    g.DrawLine(olpn, lastx, lasty, x, y)
                                '    olpn = Nothing
                                'End If
                                g.DrawLine(pn, CInt(rlastx), CInt(rlasty), x, y)

                                rlastx = x
                                rlasty = y
                            End If
                            'If _DrawKnots Then
                            '    ' we need to render a Knot at the X,Y coord
                            '    If _OutlineDataSeries Then
                            '        ' we need to outline what we are drawing
                            '        g.FillEllipse(Brushes.Black, x - ((DrawLineWeight + 6) / 2), y - ((DrawLineWeight + 6) / 2), DrawLineWeight + 6, DrawLineWeight + 6)
                            '    End If
                            '    g.FillEllipse(New SolidBrush(_DataLineColors(tt)), x - ((DrawLineWeight + 4) / 2), y - ((DrawLineWeight + 4) / 2), DrawLineWeight + 4, DrawLineWeight + 4)
                            'End If
                        End If

                    Next

                    reg = Nothing
                    rlastx = -1
                    rlasty = -1

                End If

                x = 0
                y = 0
                lastx = -1
                lasty = -1
                pn = Nothing

            End If

        Next

        DrawTitle(g)

    End Sub

    Private Sub RenderTitle2TheRightof(ByVal tit As String, ByVal g As Graphics, ByVal x As Integer, ByVal y As Integer)

        Dim siz As System.Drawing.SizeF = g.MeasureString(tit & "WW", _AxisFont)
        Dim tw As Integer = siz.Width
        Dim th As Integer = siz.Height

        g.DrawString(tit, _AxisFont, Brushes.Black, x - tw, y - (th / 2))

    End Sub

    Private Sub RenderYAxisValue2TheRightof(ByVal tit As String, ByVal g As Graphics, ByVal x As Integer, ByVal y As Integer)

        Dim siz As System.Drawing.SizeF = g.MeasureString(tit & "", _YAxisFont)
        Dim tw As Integer = siz.Width
        Dim th As Integer = siz.Height

        g.DrawString(tit, _YAxisFont, Brushes.Black, x - tw, y - (th / 2))

    End Sub

    Private Sub RenderTitleBelow(ByVal tit As String, ByVal g As Graphics, ByVal x As Integer, ByVal y As Integer)
        Dim siz As System.Drawing.SizeF = g.MeasureString(tit, _AxisFont)
        Dim tw As Integer = siz.Width
        Dim th As Integer = siz.Height

        g.DrawString(tit, _AxisFont, Brushes.Black, x - (tw / 2), y + th + (th / 3))

    End Sub

    Private Sub RenderAxisLableTitleBelow(ByVal tit As String, ByVal g As Graphics, ByVal x As Integer, ByVal y As Integer)
        Dim siz As System.Drawing.SizeF = g.MeasureString(tit, _AxisLableFont)
        Dim tw As Integer = siz.Width
        Dim th As Integer = siz.Height

        g.DrawString(tit, _AxisLableFont, Brushes.Black, x - (tw / 2), y + th + (th / 3))

    End Sub

    Private Sub DrawTitle(ByVal g As Graphics)
        If _ChartTitle = "" Then
            Exit Sub
        End If

        Dim siz As System.Drawing.SizeF = g.MeasureString(_ChartTitle, _ChartTitleFont)
        Dim fmt As New System.Drawing.StringFormat
        Dim rect As New System.Drawing.RectangleF(5, 1, Me.Width - 10, _TopMargin - 2)

        Dim tw As Integer = siz.Width
        Dim th As Integer = siz.Height

        fmt.Alignment = StringAlignment.Center
        'fmt.LineAlignment = StringAlignment.Center

        g.DrawString(_ChartTitle, _ChartTitleFont, New System.Drawing.SolidBrush(_ChartTitleColor), _
                    rect, fmt)

    End Sub

#End Region

#Region " Event Handlers "

    Private Sub TAIDiagramerCanvas_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles TAIDiagramerCanvas.Paint

        Dim g As Graphics = e.Graphics
        Dim x, y As Integer
        Dim pn As New Pen(_GridColor)

        If _painting Then
            Exit Sub
        Else
            _painting = True
            If Me.DesignMode Then
                Me.Invalidate()
            End If
        End If

        Me.TAIDiagramerCanvas.BackColor = _CanvasBackColor
        g.FillRectangle(New SolidBrush(_CanvasBackColor), 0, 0, Me.Width, Me.Height)

        If _ShowBorder Then
            Me.TAIdiagramerPanel.BorderStyle = BorderStyle.FixedSingle
        Else
            Me.TAIdiagramerPanel.BorderStyle = BorderStyle.None
        End If


        If _ShowGrid Then
            pn.DashStyle = Drawing.Drawing2D.DashStyle.Dash

            For x = 0 To _canvaswidth Step _GridSize
                g.DrawLine(pn, x, 0, x, _canvasheight)
            Next
            For y = 0 To _canvasheight Step _GridSize
                g.DrawLine(pn, 0, y, _canvaswidth, y)
            Next

            'RenderCircle(g, 20, 20, 10)
            'RenderCircle(g, 20, 30, 10)
            'RenderCircle(g, 20, 40, 10)

        End If

        If Me.DesignMode Then
            Me.AddDataSeries("Data Series1,100,100,200,200,300,350,400,290,280,190,180,90,80")
            Me.AddDataSeries("Data Series2,-50,100,220,240,150,350,410,320,300,240,200,140,100")
            Me.AddDataSeries("Data Series3,60,110,75,180,300,350,415,300,310,200,210,100,110")
            Me.XAxisLable = "PERIOD,1,2,3,4,5,6,7,8,9,10,11,12,13"
            DesignTimer.Enabled = True
            'Dim reg As New Regressor
            'reg.XYAdd(1, 100)
            'reg.XYAdd(2, 150)
            'reg.XYAdd(3, 200)
            'reg.XYAdd(4, 150)
            'reg.XYAdd(5, 100)

            'MsgBox(reg.RegVal(1) & " - " & reg.RegVal(2) & " - " & reg.RegVal(3) & " - " & reg.RegVal(4) & " - " & reg.RegVal(5))

        Else
            DesignTimer.Enabled = False
        End If

        If Not (_DataLines Is Nothing) Then
            ' we have some data so lets draw it
            RenderDataSeries(g)
        End If

        If Me.DesignMode Then
            Me.ClearDataSeries()
        End If

        pn.Dispose()
        pn = Nothing

        ' here we will get a picture of the attached canvas 
        Dim h As Integer
        Dim w As Integer

        Dim g1 As Graphics = g
        h = CInt(g1.VisibleClipBounds.Height)
        w = CInt(g1.VisibleClipBounds.Width)
        If Not _image Is Nothing Then
            _image = Nothing    ' clear and release the last image gathered 
        End If
        _image = New Bitmap(w, h)
        Dim g2 As Graphics = Graphics.FromImage(_image) ' click
        Dim dc1 As IntPtr = g1.GetHdc
        Dim dc2 As IntPtr = g2.GetHdc
        BitBlt(dc2, 0, 0, w, h, dc1, 0, 0, 13369376)

        g1.ReleaseHdc(dc1) ' clean up
        g2.ReleaseHdc(dc2)

        _painting = False

    End Sub

    Private Sub TAIDiagramer_SizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.SizeChanged

        If Me.Width < 5 Then
            Exit Sub
        End If

        If Me.Height < 5 Then
            Exit Sub
        End If

        If Me.DesignMode And _DesignModeTrackContainerSize Then

            Me.CanvasWidth = Me.Width
            Me.CanvasHeight = Me.Height

        Else
            If Not Me.DesignMode And _RunModeTrackContainerSize Then
                Me.CanvasWidth = Me.Width
                _canvaswidth = Me.Width
                Me.CanvasHeight = Me.Height
                _canvasheight = Me.Height
                Me.Refresh()
            End If
        End If



    End Sub

    Private Sub TAIDiagramerCanvas_VisibleChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TAIDiagramerCanvas.VisibleChanged
        Me.Refresh()
    End Sub

    Private Sub TAIDiagramerCanvas_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles TAIDiagramerCanvas.Validating
        Me.Refresh()
    End Sub

    Private Sub TAIDiagramerCanvas_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles TAIDiagramerCanvas.Validated
        Me.Refresh()
    End Sub

    Private Sub TAIDiagramerCanvas_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles TAIDiagramerCanvas.Resize
        Me.Refresh()
    End Sub

    Private Sub tim_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tim.Tick
        If _FlashDataLine > -1 And _FlashDataLine < 10 Then
            If _DataLineColors(_FlashDataLine).Equals(Color.Black) Then
                _DataLineColors(_FlashDataLine) = _OldDataLineColors(_FlashDataLine)
            Else
                _DataLineColors(_FlashDataLine) = Color.Black
            End If
            Me.Refresh()
        End If
    End Sub

    Private Sub TAIDiagramerCanvas_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles TAIDiagramerCanvas.Click
        Dim p As Point
        Dim x, y, xx, yy, r, c, rr, cc As Integer

        Try
            p = Me.PointToClient(Me.MousePosition)

            x = p.X
            y = p.Y

            Dim b As New Bitmap(_image)

            Dim col As System.Drawing.Color = b.GetPixel(x, y)


            For xx = 0 To 9
                If _DataLineColors(xx).A = col.A And _DataLineColors(xx).R = col.R And _DataLineColors(xx).G = col.G And _DataLineColors(xx).B = col.B Then
                    ' raise our event
                    RaiseEvent SeriesSelected(Me, xx)
                    Exit For
                End If
            Next
            col = Nothing
            b = Nothing
        Catch ex As Exception
            ' opps something happened here
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "Recovering from a mess...")
        End Try
    End Sub

#End Region

#Region " Public Methods and Functions "

    Public Sub AddDataSeries(ByVal ds As String)
        If Not (_DataLines Is Nothing) Then
            ReDim Preserve _DataLines(_DataLines.GetUpperBound(0) + 1)
            _DataLines(_DataLines.GetUpperBound(0) - 1) = ds
        Else
            ReDim _DataLines(1)
            _DataLines(0) = ds
        End If

        'Me.Refresh()
    End Sub

    Public Sub ClearDataSeries()
        _DataLines = Nothing
        tim.Enabled = False
        _OldDataLineColors.CopyTo(_DataLineColors, 0)
        _FlashDataLine = -1
        Me.Refresh()
    End Sub

#End Region

#Region "Private Classes REGRESSOR "

    Private Class Regressor

        Private Const MaxO = 25
        Private GlobalO
        Private Finished As Boolean

        Private SumX(2 * MaxO)
        Private SumYX(MaxO)
        Private M(MaxO, MaxO + 1)
        Private C(MaxO) 'coefficients in: Y = C(0)*X^0 + C(1)*X^1 + C(2)*X^2 + ...

        Private Sub GaussSolve(ByVal O)
            'gauss algorithm implementation,
            'following R.Sedgewick's "Algorithms in C", Addison-Wesley, with minor modifications
            Dim i&, j&, k&, iMax&, T#, O1#
            O1 = O + 1
            'first triangulize the matrix
            For i = 0 To O
                iMax = i : T = Math.Abs(M(iMax, i))
                For j = i + 1 To O 'find the line with the largest absvalue in this row
                    If T < Math.Abs(M(j, i)) Then iMax = j : T = Math.Abs(M(iMax, i))
                Next j
                If i < iMax Then 'exchange the two lines
                    For k = i To O1
                        T = M(i, k)
                        M(i, k) = M(iMax, k)
                        M(iMax, k) = T
                    Next k
                End If
                For j = i + 1 To O 'scale all following lines to have a leading zero
                    T = M(j, i) / M(i, i)
                    M(j, i) = 0.0#
                    For k = i + 1 To O1
                        M(j, k) = M(j, k) - M(i, k) * T
                    Next k
                Next j
            Next i
            'then substitute the coefficients
            For j = O To 0 Step -1
                T = M(j, O1)
                For k = j + 1 To O
                    T = T - M(j, k) * C(k)
                Next k
                C(j) = T / M(j, j)
            Next j
            Finished = True
        End Sub

        Private Sub BuildMatrix(ByVal O)
            Dim i&, k&, O1&
            O1 = O + 1
            For i = 0 To O
                For k = 0 To O
                    M(i, k) = SumX(i + k)
                Next k
                M(i, O1) = SumYX(i)
            Next i
        End Sub

        Private Sub FinalizeMatrix(ByVal O)
            Dim i&, O1&
            O1 = O + 1
            For i = 0 To O
                M(i, O1) = SumYX(i)
            Next i
        End Sub

        Private Sub Solve()
            Dim O&
            O = GlobalO
            If XYCount <= O Then O = XYCount - 1
            If O < 0 Then Exit Sub
            BuildMatrix(O)
            On Error Resume Next
            GaussSolve(O)
            While (Err.Number <> 0) And (1 < O)
                Err.Clear()
                C(0) = 0.0
                O = O - 1
                FinalizeMatrix(O)
            End While
            On Error GoTo 0
        End Sub

        Public Sub New()
            Init()
            GlobalO = 2
        End Sub

        Public Sub Init()
            Dim i&
            Finished = False
            For i = 0 To MaxO
                SumX(i) = 0.0
                SumX(i + MaxO) = 0.0
                SumYX(i) = 0.0
                C(i) = 0.0
            Next i
        End Sub

        Public ReadOnly Property Coeff(ByVal Exponent)
            Get
                Dim Ex, O
                If Not Finished Then Solve()
                Ex = Math.Abs(Exponent)
                O = GlobalO
                If XYCount <= O Then O = XYCount - 1
                If O < Ex Then Return 0.0 Else Return C(Ex)
            End Get
        End Property

        Public Property Degree()
            Get
                Return GlobalO
            End Get
            Set(ByVal Value)
                If Value < 0 Or MaxO < Value Then
                    GlobalO = MaxO
                Else
                    GlobalO = Value
                End If
            End Set
        End Property

        Public ReadOnly Property XYCount()
            Get
                Return CLng(SumX(0))
            End Get
        End Property

        Public Function XYAdd(ByVal NewX, ByVal NewY)
            Dim i&, j&, TX#, Max2O&
            Finished = False
            Max2O = 2 * GlobalO
            TX = 1.0
            SumX(0) = SumX(0) + 1
            SumYX(0) = SumYX(0) + NewY
            For i = 1 To GlobalO
                TX = TX * NewX
                SumX(i) = SumX(i) + TX
                SumYX(i) = SumYX(i) + NewY * TX
            Next i
            For i = GlobalO + 1 To Max2O
                TX = TX * NewX
                SumX(i) = SumX(i) + TX
            Next i
        End Function

        Public Function RegVal(ByVal X)
            Dim i, O
            If Not Finished Then Solve()
            RegVal = 0.0
            O = GlobalO
            If XYCount <= O Then O = XYCount - 1
            For i = 0 To O
                RegVal = RegVal + C(i) * X ^ i
            Next i
        End Function

    End Class

#End Region

End Class

