Imports System.Drawing.Drawing2D
Imports System.Drawing.Text

''' <summary>A matchmaking region (display name + AWS id).</summary>
Friend NotInheritable Class GameRegion
    Public ReadOnly Name As String
    Public ReadOnly Id As String

    Public Sub New(name As String, id As String)
        Me.Name = name
        Me.Id = id
    End Sub
End Class

Friend Enum LockState
    NoLock
    Locked
    Unknown
End Enum


''' <summary>Colors, fonts and drawing helpers shared by all custom controls.</summary>
Friend NotInheritable Class Theme

    Private Sub New()
    End Sub

    Public Shared ReadOnly Back As Color = Color.FromArgb(18, 18, 21)
    Public Shared ReadOnly HeaderBack As Color = Color.FromArgb(11, 11, 13)
    Public Shared ReadOnly Surface As Color = Color.FromArgb(28, 28, 32)
    Public Shared ReadOnly SurfaceHover As Color = Color.FromArgb(37, 37, 42)
    Public Shared ReadOnly SurfaceSelected As Color = Color.FromArgb(46, 21, 25)
    Public Shared ReadOnly Border As Color = Color.FromArgb(50, 50, 57)
    Public Shared ReadOnly Accent As Color = Color.FromArgb(196, 28, 44)
    Public Shared ReadOnly AccentHover As Color = Color.FromArgb(222, 44, 60)
    Public Shared ReadOnly AccentPressed As Color = Color.FromArgb(160, 20, 34)
    Public Shared ReadOnly TextMain As Color = Color.FromArgb(236, 236, 240)
    Public Shared ReadOnly TextMuted As Color = Color.FromArgb(138, 138, 150)
    Public Shared ReadOnly Good As Color = Color.FromArgb(72, 199, 116)
    Public Shared ReadOnly Medium As Color = Color.FromArgb(232, 181, 62)
    Public Shared ReadOnly Bad As Color = Color.FromArgb(232, 84, 72)

    Public Shared ReadOnly TitleFont As Font = PickFont(15.0F, FontStyle.Regular, "Bahnschrift SemiBold", "Bahnschrift", "Segoe UI Semibold")
    Public Shared ReadOnly BodyFont As Font = PickFont(9.0F, FontStyle.Regular, "Segoe UI")
    Public Shared ReadOnly BodyBoldFont As Font = PickFont(9.75F, FontStyle.Bold, "Segoe UI")
    Public Shared ReadOnly SmallFont As Font = PickFont(8.25F, FontStyle.Regular, "Segoe UI")
    Public Shared ReadOnly TagFont As Font = PickFont(7.0F, FontStyle.Bold, "Segoe UI")
    Public Shared ReadOnly SectionFont As Font = PickFont(8.25F, FontStyle.Bold, "Bahnschrift SemiBold", "Bahnschrift", "Segoe UI")

    ''' <summary>Returns the first installed font family from the list, otherwise the system UI font.</summary>
    Public Shared Function PickFont(size As Single, style As FontStyle, ParamArray families As String()) As Font
        Using installed As New InstalledFontCollection()
            For Each familyName As String In families
                If installed.Families.Any(Function(f) String.Equals(f.Name, familyName, StringComparison.OrdinalIgnoreCase)) Then
                    Return New Font(familyName, size, style)
                End If
            Next
        End Using
        Return New Font(SystemFonts.MessageBoxFont.FontFamily, size, style)
    End Function

    Public Shared Function PingColor(ms As Integer) As Color
        If ms < 0 Then Return Bad
        If ms < 80 Then Return Good
        If ms < 160 Then Return Medium
        Return Bad
    End Function

    Public Shared Function RoundedRect(rect As RectangleF, radius As Single) As GraphicsPath
        Dim d = radius * 2
        Dim path As New GraphicsPath()
        path.AddArc(rect.X, rect.Y, d, d, 180, 90)
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90)
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90)
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90)
        path.CloseFigure()
        Return path
    End Function

    ''' <summary>Draws a small rounded label like "ACTIVE" and returns its width.</summary>
    Public Shared Function DrawTag(g As Graphics, text As String, x As Integer, y As Integer, tagColor As Color) As Integer
        Dim size = TextRenderer.MeasureText(g, text, TagFont, System.Drawing.Size.Empty, TextFormatFlags.NoPadding)
        Dim rect As New Rectangle(x, y, size.Width + 12, 16)
        Using path = RoundedRect(New RectangleF(rect.X, rect.Y, rect.Width, rect.Height), 8)
            Using fill As New SolidBrush(Color.FromArgb(45, tagColor))
                g.FillPath(fill, path)
            End Using
        End Using
        TextRenderer.DrawText(g, text, TagFont, rect, tagColor,
                              TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPadding)
        Return rect.Width
    End Function

End Class


''' <summary>One clickable server card: name, region id, ping and signal bars.</summary>
Friend Class RegionCard
    Inherits Control

    ''' <summary>Value for <see cref="PingMs"/> while no measurement exists yet.</summary>
    Public Const PingPending As Integer = -2
    ''' <summary>Value for <see cref="PingMs"/> when the server did not answer.</summary>
    Public Const PingUnreachable As Integer = -1

    Private _regionName As String = ""
    Private _regionId As String = ""
    Private _pingMs As Integer = PingPending
    Private _isSelected As Boolean
    Private _isActive As Boolean
    Private _isFastest As Boolean
    Private _hover As Boolean

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.StandardClick Or ControlStyles.StandardDoubleClick, True)
        SetStyle(ControlStyles.Selectable, False)
        BackColor = Theme.Back
        Cursor = Cursors.Hand
        Size = New System.Drawing.Size(190, 60)
    End Sub

    Public Property RegionName As String
        Get
            Return _regionName
        End Get
        Set(value As String)
            _regionName = value
            Invalidate()
        End Set
    End Property

    Public Property RegionId As String
        Get
            Return _regionId
        End Get
        Set(value As String)
            _regionId = value
            Invalidate()
        End Set
    End Property

    Public Property PingMs As Integer
        Get
            Return _pingMs
        End Get
        Set(value As Integer)
            If _pingMs = value Then Return
            _pingMs = value
            Invalidate()
        End Set
    End Property

    Public Property IsSelected As Boolean
        Get
            Return _isSelected
        End Get
        Set(value As Boolean)
            If _isSelected = value Then Return
            _isSelected = value
            Invalidate()
        End Set
    End Property

    ''' <summary>True when the hosts file currently locks matchmaking to this region.</summary>
    Public Property IsActive As Boolean
        Get
            Return _isActive
        End Get
        Set(value As Boolean)
            If _isActive = value Then Return
            _isActive = value
            Invalidate()
        End Set
    End Property

    ''' <summary>True when this region currently has the lowest ping.</summary>
    Public Property IsFastest As Boolean
        Get
            Return _isFastest
        End Get
        Set(value As Boolean)
            If _isFastest = value Then Return
            _isFastest = value
            Invalidate()
        End Set
    End Property

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        _hover = True
        Invalidate()
        MyBase.OnMouseEnter(e)
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        _hover = False
        Invalidate()
        MyBase.OnMouseLeave(e)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias

        ' Card background and border
        Dim fillColor = If(_isSelected, Theme.SurfaceSelected, If(_hover, Theme.SurfaceHover, Theme.Surface))
        Dim borderColor = If(_isSelected, Theme.Accent, If(_isActive, Theme.Good, Theme.Border))
        Using path = Theme.RoundedRect(New RectangleF(0.5F, 0.5F, Width - 1.5F, Height - 1.5F), 8)
            Using fill As New SolidBrush(fillColor)
                g.FillPath(fill, path)
            End Using
            Using pen As New Pen(borderColor, If(_isSelected, 1.5F, 1.0F))
                g.DrawPath(pen, path)
            End Using
        End Using

        Const pad As Integer = 12
        Const pingWidth As Integer = 64

        ' Line 1: name (left) and ping (right)
        TextRenderer.DrawText(g, _regionName, Theme.BodyBoldFont,
                              New Rectangle(pad, 9, Width - pad * 2 - pingWidth, 20), Theme.TextMain,
                              TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPadding)

        Dim pingText As String
        Dim pingColor As Color
        Select Case _pingMs
            Case PingPending
                pingText = "…"
                pingColor = Theme.TextMuted
            Case PingUnreachable
                pingText = "offline"
                pingColor = Theme.Bad
            Case Else
                pingText = _pingMs.ToString() & " ms"
                pingColor = Theme.PingColor(_pingMs)
        End Select
        TextRenderer.DrawText(g, pingText, Theme.BodyBoldFont,
                              New Rectangle(Width - pad - pingWidth, 9, pingWidth, 20), pingColor,
                              TextFormatFlags.Right Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPadding)

        ' Line 2: region id, optional tag, signal bars
        Dim idRect As New Rectangle(pad, 33, Width - pad * 2 - 30, 16)
        TextRenderer.DrawText(g, _regionId, Theme.SmallFont, idRect, Theme.TextMuted,
                              TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPadding)

        Dim idWidth = TextRenderer.MeasureText(g, _regionId, Theme.SmallFont, System.Drawing.Size.Empty, TextFormatFlags.NoPadding).Width
        Dim tagX = pad + idWidth + 6
        If _isActive Then
            Theme.DrawTag(g, "ACTIVE", tagX, 33, Theme.Good)
        ElseIf _isFastest Then
            Theme.DrawTag(g, "BEST", tagX, 33, Theme.TextMain)
        End If

        DrawSignalBars(g, Width - pad - 25, 48)
    End Sub

    Private Sub DrawSignalBars(g As Graphics, x As Integer, bottom As Integer)
        Dim filled As Integer
        If _pingMs < 0 Then
            filled = 0
        ElseIf _pingMs < 50 Then
            filled = 4
        ElseIf _pingMs < 80 Then
            filled = 3
        ElseIf _pingMs < 160 Then
            filled = 2
        Else
            filled = 1
        End If

        Dim onColor = Theme.PingColor(_pingMs)
        For i = 0 To 3
            Dim h = 4 + i * 3
            Using b As New SolidBrush(If(i < filled, onColor, Theme.Border))
                g.FillRectangle(b, x + i * 7, bottom - h, 4, h)
            End Using
        Next
    End Sub

End Class


''' <summary>Flat rounded button. Primary = filled accent, otherwise outlined.</summary>
Friend Class ModernButton
    Inherits Control

    Private _primary As Boolean
    Private _hover As Boolean
    Private _pressed As Boolean

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.StandardClick, True)
        BackColor = Theme.Back
        ForeColor = Theme.TextMain
        Font = Theme.BodyBoldFont
        Cursor = Cursors.Hand
        Size = New System.Drawing.Size(120, 38)
    End Sub

    Public Property Primary As Boolean
        Get
            Return _primary
        End Get
        Set(value As Boolean)
            _primary = value
            Invalidate()
        End Set
    End Property

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        _hover = True
        Invalidate()
        MyBase.OnMouseEnter(e)
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        _hover = False
        _pressed = False
        Invalidate()
        MyBase.OnMouseLeave(e)
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then _pressed = True
        Invalidate()
        MyBase.OnMouseDown(e)
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        _pressed = False
        Invalidate()
        MyBase.OnMouseUp(e)
    End Sub

    Protected Overrides Sub OnEnabledChanged(e As EventArgs)
        Cursor = If(Enabled, Cursors.Hand, Cursors.Default)
        Invalidate()
        MyBase.OnEnabledChanged(e)
    End Sub

    Protected Overrides Sub OnTextChanged(e As EventArgs)
        Invalidate()
        MyBase.OnTextChanged(e)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias

        Dim fillColor As Color
        Dim borderColor As Color
        Dim textColor As Color

        If Not Enabled Then
            fillColor = Theme.Surface
            borderColor = Theme.Border
            textColor = Theme.TextMuted
        ElseIf _primary Then
            fillColor = If(_pressed, Theme.AccentPressed, If(_hover, Theme.AccentHover, Theme.Accent))
            borderColor = fillColor
            textColor = Color.White
        Else
            fillColor = If(_pressed, Theme.Surface, If(_hover, Theme.SurfaceHover, BackColor))
            borderColor = If(_hover, Theme.TextMuted, Theme.Border)
            textColor = Theme.TextMain
        End If

        Using path = Theme.RoundedRect(New RectangleF(0.5F, 0.5F, Width - 1.5F, Height - 1.5F), 6)
            Using fill As New SolidBrush(fillColor)
                g.FillPath(fill, path)
            End Using
            Using pen As New Pen(borderColor)
                g.DrawPath(pen, path)
            End Using
        End Using

        TextRenderer.DrawText(g, Text, Font, ClientRectangle, textColor,
                              TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
    End Sub

End Class
