Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Runtime.InteropServices
Imports System.Security.Principal
Imports System.Text.RegularExpressions
Imports System.Threading.Tasks

Public Class Form1

    ' Order = order of the cards (Europe, North America, South America, Asia-Pacific)
    Private ReadOnly regions As GameRegion() = {
        New GameRegion("Frankfurt", "eu-central-1"),
        New GameRegion("Ireland", "eu-west-1"),
        New GameRegion("London", "eu-west-2"),
        New GameRegion("N. Virginia", "us-east-1"),
        New GameRegion("Ohio", "us-east-2"),
        New GameRegion("N. California", "us-west-1"),
        New GameRegion("Oregon", "us-west-2"),
        New GameRegion("Canada", "ca-central-1"),
        New GameRegion("São Paulo", "sa-east-1"),
        New GameRegion("Mumbai", "ap-south-1"),
        New GameRegion("Seoul", "ap-northeast-2"),
        New GameRegion("Singapore", "ap-southeast-1"),
        New GameRegion("Sydney", "ap-southeast-2"),
        New GameRegion("Tokyo", "ap-northeast-1"),
        New GameRegion("Hong Kong", "ap-east-1")
    }

    Private Shared ReadOnly HostsPath As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "etc", "hosts")

    Private Const HostsMarker As String = "# DBD Region Changer"
    Private Shared ReadOnly GameliftPattern As New Regex("gamelift-ping\.[^.\s]+\.api\.aws", RegexOptions.IgnoreCase)

    Private ReadOnly cards As New List(Of RegionCard)()
    Private selectedRegion As GameRegion
    Private activeRegion As GameRegion
    Private lockStatus As LockState = LockState.NoLock
    Private pingBusy As Boolean

    ' ---------- Startup ----------

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not IsUserAdministrator() Then
            Dim r = MessageBox.Show("This application needs administrator rights to edit the hosts file." & Environment.NewLine &
                                    "Press OK to restart as administrator or Cancel to exit.",
                                    "Administrator required", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)
            If r = DialogResult.OK Then
                RestartAsAdmin()
            Else
                Application.Exit()
            End If
            Return
        End If

        ApplyTheme()
        AddAuthorLink()
        BuildCards()
        RefreshActiveRegion()

        selectedRegion = If(activeRegion, regions(0))
        UpdateSelection()
        SetStatus("Pick a server and lock it before you start matchmaking.", Theme.TextMuted)

        TimerPing.Start()
        RefreshPings()
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        EnableDarkTitleBar()
    End Sub

    Private Sub ApplyTheme()
        BackColor = Theme.Back
        ForeColor = Theme.TextMain
        Font = Theme.BodyFont

        PanelHeader.BackColor = Theme.HeaderBack
        PanelCards.BackColor = Theme.Back
        PanelFooter.BackColor = Theme.Back

        LabelSection.Font = Theme.SectionFont
        LabelSection.ForeColor = Theme.TextMuted
        LabelHint.Font = Theme.SmallFont
        LabelHint.ForeColor = Theme.TextMuted
        LabelStatus.Font = Theme.BodyFont

        ' Panels that are painted by hand should not flicker
        EnableDoubleBuffering(PanelHeader)
        EnableDoubleBuffering(PanelFooter)
    End Sub

    ' ---------- Author ----------

    Private Const AuthorName As String = "FGSeRaser"
    Private Const GitHubUrl As String = "https://github.com/FGSeRaser"

    ''' <summary>Small "by FGSeRaser" link in the header that opens the GitHub profile.</summary>
    Private Sub AddAuthorLink()
        Dim authorLabel As New Label() With {
            .Text = "by " & AuthorName & "  ·  GitHub ↗",
            .AutoSize = True,
            .Font = Theme.SmallFont,
            .ForeColor = Theme.TextMuted,
            .BackColor = Theme.HeaderBack,
            .Cursor = Cursors.Hand,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right}
        PanelHeader.Controls.Add(authorLabel)
        authorLabel.Location = New Point(PanelHeader.Width - 20 - authorLabel.PreferredWidth, 62)
        Dim tip As New ToolTip()
        tip.SetToolTip(authorLabel, GitHubUrl)
        AddHandler authorLabel.MouseEnter, Sub(s, e) authorLabel.ForeColor = Theme.Accent
        AddHandler authorLabel.MouseLeave, Sub(s, e) authorLabel.ForeColor = Theme.TextMuted
        AddHandler authorLabel.Click, Sub(s, e) OpenGitHub()
    End Sub

    Private Shared Sub OpenGitHub()
        Try
            Process.Start(New ProcessStartInfo(GitHubUrl) With {.UseShellExecute = True})
        Catch ex As Exception
            MessageBox.Show("Could not open the browser." & Environment.NewLine & GitHubUrl, AuthorName,
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Try
    End Sub

    Private Shared Sub EnableDoubleBuffering(target As Control)
        Dim prop = GetType(Control).GetProperty("DoubleBuffered",
                                                System.Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.NonPublic)
        prop?.SetValue(target, True, Nothing)
    End Sub

    Private Sub BuildCards()
        Const columns As Integer = 3
        Const gap As Integer = 10
        Const cardHeight As Integer = 60
        Dim cardWidth = (PanelCards.ClientSize.Width - gap * (columns - 1)) \ columns

        PanelCards.SuspendLayout()
        For i = 0 To regions.Length - 1
            Dim card As New RegionCard() With {
                .RegionName = regions(i).Name,
                .RegionId = regions(i).Id,
                .Tag = regions(i)
            }
            card.SetBounds((i Mod columns) * (cardWidth + gap), (i \ columns) * (cardHeight + gap), cardWidth, cardHeight)
            AddHandler card.Click, AddressOf Card_Click
            AddHandler card.DoubleClick, AddressOf Card_DoubleClick
            PanelCards.Controls.Add(card)
            cards.Add(card)
        Next
        PanelCards.ResumeLayout()
    End Sub

    ' ---------- Admin helpers ----------

    Private Shared Function IsUserAdministrator() As Boolean
        Using id = WindowsIdentity.GetCurrent()
            Return New WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator)
        End Using
    End Function

    Private Shared Sub RestartAsAdmin()
        Dim psi As New ProcessStartInfo() With {
            .UseShellExecute = True,
            .WorkingDirectory = Application.StartupPath,
            .FileName = Application.ExecutablePath,
            .Verb = "runas"
        }
        Try
            Process.Start(psi)
            Application.Exit()
        Catch ex As Exception
            MessageBox.Show("Failed to restart as administrator: " & ex.Message,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function EnsureAdmin() As Boolean
        If IsUserAdministrator() Then Return True
        MessageBox.Show("Please run this application as administrator to modify the hosts file.",
                        "Administrator required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        RestartAsAdmin()
        Return False
    End Function

    ' ---------- UI events ----------

    Private Sub Card_Click(sender As Object, e As EventArgs)
        selectedRegion = DirectCast(DirectCast(sender, RegionCard).Tag, GameRegion)
        UpdateSelection()
    End Sub

    Private Sub Card_DoubleClick(sender As Object, e As EventArgs)
        selectedRegion = DirectCast(DirectCast(sender, RegionCard).Tag, GameRegion)
        UpdateSelection()
        LockSelectedRegion()
    End Sub

    Private Sub ButtonApply_Click(sender As Object, e As EventArgs) Handles ButtonApply.Click
        LockSelectedRegion()
    End Sub

    Private Sub ButtonReset_Click(sender As Object, e As EventArgs) Handles ButtonReset.Click
        If Not EnsureAdmin() Then Return
        Try
            WriteHosts(ReadHostsWithoutOverrides())
            FlushDns()
        Catch ex As Exception
            ShowError("Could not reset the hosts file", ex)
            Return
        End Try

        RefreshActiveRegion()
        SetStatus("Lock removed. The game picks its region automatically again.", Theme.Good)
    End Sub

    Private Sub TimerPing_Tick(sender As Object, e As EventArgs) Handles TimerPing.Tick
        RefreshActiveRegion()
        RefreshPings()
    End Sub

    Private Sub UpdateSelection()
        For Each card As RegionCard In cards
            card.IsSelected = card.Tag Is selectedRegion
        Next

        If selectedRegion Is Nothing Then
            ButtonApply.Text = "Lock region"
            ButtonApply.Enabled = False
        ElseIf selectedRegion Is activeRegion Then
            ButtonApply.Text = "Locked: " & selectedRegion.Name
            ButtonApply.Enabled = False
        Else
            ButtonApply.Text = "Lock to " & selectedRegion.Name
            ButtonApply.Enabled = True
        End If

        ButtonReset.Enabled = lockStatus <> LockState.NoLock
    End Sub

    Private Sub SetStatus(text As String, color As Color)
        LabelStatus.Text = text
        LabelStatus.ForeColor = color
    End Sub

    Private Shared Sub ShowError(title As String, ex As Exception)
        MessageBox.Show(ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    ' ---------- Painting of header and footer ----------

    Private Sub PanelHeader_Paint(sender As Object, e As PaintEventArgs) Handles PanelHeader.Paint
        Dim g = e.Graphics
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias

        TextRenderer.DrawText(g, "DBD REGION CHANGER", Theme.TitleFont, New Point(18, 16), Theme.TextMain)
        TextRenderer.DrawText(g, "Lock Dead by Daylight matchmaking to one server region", Theme.SmallFont,
                              New Point(20, 50), Theme.TextMuted)

        ' Status pill on the right
        Dim pillText As String
        Dim pillColor As Color
        Select Case lockStatus
            Case LockState.Locked
                pillText = "LOCKED · " & activeRegion.Name
                pillColor = Theme.Good
            Case LockState.Unknown
                pillText = "UNKNOWN STATE"
                pillColor = Theme.Medium
            Case Else
                pillText = "NO LOCK"
                pillColor = Theme.TextMuted
        End Select

        Dim textSize = TextRenderer.MeasureText(g, pillText, Theme.BodyBoldFont, System.Drawing.Size.Empty, TextFormatFlags.NoPadding)
        Dim pillWidth = textSize.Width + 40
        Dim pill As New Rectangle(PanelHeader.Width - 20 - pillWidth, 26, pillWidth, 30)

        Using path = Theme.RoundedRect(New RectangleF(pill.X, pill.Y, pill.Width, pill.Height), 15)
            Using fill As New SolidBrush(Color.FromArgb(38, pillColor))
                g.FillPath(fill, path)
            End Using
            Using pen As New Pen(Color.FromArgb(110, pillColor))
                g.DrawPath(pen, path)
            End Using
        End Using
        Using dot As New SolidBrush(pillColor)
            g.FillEllipse(dot, pill.X + 14, pill.Y + 11, 8, 8)
        End Using
        TextRenderer.DrawText(g, pillText, Theme.BodyBoldFont,
                              New Rectangle(pill.X + 28, pill.Y, pill.Width - 34, pill.Height), pillColor,
                              TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPadding)

        Using line As New Pen(Theme.Border)
            g.DrawLine(line, 0, PanelHeader.Height - 1, PanelHeader.Width, PanelHeader.Height - 1)
        End Using
    End Sub

    Private Sub PanelFooter_Paint(sender As Object, e As PaintEventArgs) Handles PanelFooter.Paint
        Using line As New Pen(Theme.Border)
            e.Graphics.DrawLine(line, 0, 0, PanelFooter.Width, 0)
        End Using
    End Sub

    ' ---------- hosts file ----------

    Private Sub LockSelectedRegion()
        If selectedRegion Is Nothing OrElse selectedRegion Is activeRegion Then Return
        If Not EnsureAdmin() Then Return

        Dim target = selectedRegion
        Try
            Dim lines = ReadHostsWithoutOverrides()
            lines.Add(HostsMarker)
            For Each entryRegion As GameRegion In regions
                Dim entry = "0.0.0.0 gamelift-ping." & entryRegion.Id & ".api.aws"
                ' The chosen region stays reachable (commented out), all others are blocked
                lines.Add(If(entryRegion Is target, "# " & entry, entry))
            Next
            WriteHosts(lines)
            FlushDns()
        Catch ex As Exception
            ShowError("Could not write the hosts file", ex)
            Return
        End Try

        RefreshActiveRegion()
        If activeRegion Is target Then
            SetStatus("Locked to " & target.Name & ". Restart Dead by Daylight if it is running.", Theme.Good)
        Else
            SetStatus("The lock could not be verified. Check whether another tool changes your hosts file.", Theme.Bad)
        End If
    End Sub

    ''' <summary>Reads the hosts file and drops every line this tool manages.</summary>
    Private Shared Function ReadHostsWithoutOverrides() As List(Of String)
        Dim lines As New List(Of String)()
        If File.Exists(HostsPath) Then
            lines.AddRange(File.ReadAllLines(HostsPath).
                           Where(Function(l) Not GameliftPattern.IsMatch(l) AndAlso l.Trim() <> HostsMarker))
        End If

        While lines.Count > 0 AndAlso String.IsNullOrWhiteSpace(lines(lines.Count - 1))
            lines.RemoveAt(lines.Count - 1)
        End While
        If lines.Count > 0 Then lines.Add(String.Empty)
        Return lines
    End Function

    Private Shared Sub WriteHosts(lines As IEnumerable(Of String))
        Dim info As New FileInfo(HostsPath)
        Dim wasReadOnly = info.Exists AndAlso info.IsReadOnly
        If wasReadOnly Then info.IsReadOnly = False
        Try
            File.WriteAllLines(HostsPath, lines)
        Finally
            If wasReadOnly Then info.IsReadOnly = True
        End Try
    End Sub

    ''' <summary>Clears the Windows DNS cache so the change applies without a reboot.</summary>
    Private Shared Sub FlushDns()
        Try
            Using p = Process.Start(New ProcessStartInfo("ipconfig", "/flushdns") With {
                .CreateNoWindow = True,
                .UseShellExecute = False
            })
                p?.WaitForExit(5000)
            End Using
        Catch
            ' Not critical: the cache expires on its own
        End Try
    End Sub

    Private Sub RefreshActiveRegion()
        Dim newState = LockState.NoLock
        Dim newRegion As GameRegion = Nothing

        Try
            If File.Exists(HostsPath) Then
                Dim content = File.ReadAllText(HostsPath)
                If GameliftPattern.IsMatch(content) Then
                    newState = LockState.Unknown
                    For Each entryRegion As GameRegion In regions
                        Dim pattern = "^\s*#\s*0\.0\.0\.0\s+gamelift-ping\." & Regex.Escape(entryRegion.Id) & "\.api\.aws\s*$"
                        If Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase Or RegexOptions.Multiline) Then
                            newState = LockState.Locked
                            newRegion = entryRegion
                            Exit For
                        End If
                    Next
                End If
            End If
        Catch
            newState = LockState.Unknown
        End Try

        If newState = lockStatus AndAlso newRegion Is activeRegion Then Return

        lockStatus = newState
        activeRegion = newRegion
        For Each card As RegionCard In cards
            card.IsActive = card.Tag Is activeRegion
        Next
        UpdateSelection()
        PanelHeader.Invalidate()
    End Sub

    ' ---------- Ping ----------

    Private Async Sub RefreshPings()
        If pingBusy Then Return
        pingBusy = True
        Try
            Dim results = Await Task.WhenAll(regions.Select(Function(r) MeasurePingAsync(r.Id)))

            Dim reachable = results.Where(Function(ms) ms >= 0).ToList()
            Dim best = If(reachable.Count > 0, reachable.Min(), -1)

            For i = 0 To cards.Count - 1
                cards(i).PingMs = results(i)
                cards(i).IsFastest = best >= 0 AndAlso results(i) = best
            Next
        Finally
            pingBusy = False
        End Try
    End Sub

    ''' <summary>TCP connect time to the GameLift endpoint of a region, or -1 if unreachable.</summary>
    Private Shared Async Function MeasurePingAsync(regionId As String) As Task(Of Integer)
        Const timeoutMs As Integer = 2000
        Try
            ' Resolve first so DNS time is not counted as ping
            Dim addresses = Await Dns.GetHostAddressesAsync("gamelift." & regionId & ".amazonaws.com")
            Dim ip = addresses.FirstOrDefault(Function(a) a.AddressFamily = AddressFamily.InterNetwork)
            If ip Is Nothing Then Return RegionCard.PingUnreachable

            Using client As New TcpClient(AddressFamily.InterNetwork)
                Dim sw = Stopwatch.StartNew()
                Dim connectTask = client.ConnectAsync(ip, 443)
                Dim finished = Await Task.WhenAny(connectTask, Task.Delay(timeoutMs))
                If finished IsNot connectTask Then Return RegionCard.PingUnreachable
                Await connectTask
                sw.Stop()
                Return CInt(Math.Max(1, sw.ElapsedMilliseconds))
            End Using
        Catch
            Return RegionCard.PingUnreachable
        End Try
    End Function

    ' ---------- Dark title bar (Windows 10 20H1+ / Windows 11) ----------

    <DllImport("dwmapi.dll")>
    Private Shared Function DwmSetWindowAttribute(hwnd As IntPtr, attribute As Integer, ByRef value As Integer, size As Integer) As Integer
    End Function

    Private Sub EnableDarkTitleBar()
        Try
            Dim enabled = 1
            If DwmSetWindowAttribute(Handle, 20, enabled, 4) <> 0 Then
                DwmSetWindowAttribute(Handle, 19, enabled, 4)   ' older Windows 10 builds
            End If
        Catch
            ' Older Windows: keep the default title bar
        End Try
    End Sub

End Class
