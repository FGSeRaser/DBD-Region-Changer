Imports System.Net
Imports System.Net.Sockets
Imports System.Diagnostics
Imports System.Security.Principal
Imports System.IO
Imports System.Text.RegularExpressions

Public Class Form1

    Private hostsPath As String = "C:\Windows\System32\drivers\etc\hosts"

    ' Key = selection number, Value = (DisplayName, Region-ID)
    Private regions As New Dictionary(Of String, KeyValuePair(Of String, String)) From {
        {"0", New KeyValuePair(Of String, String)("Default (no lock)", "")},
        {"1", New KeyValuePair(Of String, String)("N. Virginia", "us-east-1")},
        {"2", New KeyValuePair(Of String, String)("Ohio", "us-east-2")},
        {"3", New KeyValuePair(Of String, String)("N. California", "us-west-1")},
        {"4", New KeyValuePair(Of String, String)("Oregon", "us-west-2")},
        {"5", New KeyValuePair(Of String, String)("Frankfurt", "eu-central-1")},
        {"6", New KeyValuePair(Of String, String)("Ireland", "eu-west-1")},
        {"7", New KeyValuePair(Of String, String)("London", "eu-west-2")},
        {"8", New KeyValuePair(Of String, String)("South America", "sa-east-1")},
        {"9", New KeyValuePair(Of String, String)("Mumbai", "ap-south-1")},
        {"10", New KeyValuePair(Of String, String)("Seoul", "ap-northeast-2")},
        {"11", New KeyValuePair(Of String, String)("Singapore", "ap-southeast-1")},
        {"12", New KeyValuePair(Of String, String)("Sydney", "ap-southeast-2")},
        {"13", New KeyValuePair(Of String, String)("Tokyo", "ap-northeast-1")},
        {"14", New KeyValuePair(Of String, String)("Hong Kong", "ap-east-1")},
        {"15", New KeyValuePair(Of String, String)("Canada", "ca-central-1")}
    }

    ' Region-Key -> ListView index
    Private regionIndex As New Dictionary(Of String, Integer)()

    ' ---------- Form-Load ----------

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Require admin at startup
        If Not IsUserAdministrator() Then
            Dim r = MessageBox.Show("This application must be started as Administrator." & Environment.NewLine &
                                    "Press OK to restart as admin or Cancel to exit.",
                                    "Administrator required",
                                    MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Warning)
            If r = DialogResult.OK Then
                RestartAsAdmin()
            Else
                Application.Exit()
            End If
            Return
        End If

        ' Fill ComboBox without data binding
        ComboBoxRegion.DataSource = Nothing
        ComboBoxRegion.Items.Clear()
        For Each kvp In regions
            If kvp.Key <> "0" Then
                ComboBoxRegion.Items.Add(kvp.Value.Key)
            End If
        Next
        ComboBoxRegion.DropDownStyle = ComboBoxStyle.DropDownList
        If ComboBoxRegion.Items.Count > 0 Then ComboBoxRegion.SelectedIndex = 0

        ' Configure ListView (columns Region/Ping/Status are created in designer)
        ListViewPing.View = View.Details
        ListViewPing.FullRowSelect = True
        ListViewPing.GridLines = True

        ' Build items once
        BuildPingListSkeleton()

        ' Read current region from hosts
        LabelStatus.Text = "Current region: " & GetCurrentRegion()

        TimerPing.Interval = 5000
        TimerPing.Start()
        UpdatePingList()
    End Sub

    Private Sub BuildPingListSkeleton()
        ListViewPing.Items.Clear()
        regionIndex.Clear()

        For Each kvp In regions
            Dim key = kvp.Key
            Dim name = kvp.Value.Key

            Dim item As New ListViewItem(name)   ' col 0: Region
            item.SubItems.Add("… ms")            ' col 1: Ping
            item.SubItems.Add("waiting")         ' col 2: Status

            Dim idx = ListViewPing.Items.Add(item).Index
            regionIndex(key) = idx
        Next
    End Sub

    ' ---------- Admin helpers ----------

    Private Function IsUserAdministrator() As Boolean
        Dim id = WindowsIdentity.GetCurrent()
        Dim princ = New WindowsPrincipal(id)
        Return princ.IsInRole(WindowsBuiltInRole.Administrator)
    End Function

    Private Sub RestartAsAdmin()
        Dim psi As New ProcessStartInfo()
        psi.UseShellExecute = True
        psi.WorkingDirectory = Application.StartupPath
        psi.FileName = Application.ExecutablePath
        psi.Verb = "runas"
        Try
            Process.Start(psi)
            Application.Exit()
        Catch ex As Exception
            MessageBox.Show("Failed to restart as administrator: " & ex.Message,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ---------- Buttons ----------

    Private Sub ButtonAnwenden_Click(sender As Object, e As EventArgs) Handles ButtonAnwenden.Click
        If Not IsUserAdministrator() Then
            MessageBox.Show("Please run this application as Administrator to modify the hosts file.",
                            "Administrator required",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
            RestartAsAdmin()
            Return
        End If

        Dim selectedName = ComboBoxRegion.Text
        Dim selectedKey = regions.First(Function(r) r.Value.Key = selectedName).Key

        ApplyRegion(selectedKey)

        ' Re-read hosts
        Dim current = GetCurrentRegion()
        LabelStatus.Text = "Current region: " & current

        ' Confirm / error message
        If current = selectedName Then
            MessageBox.Show("Region successfully changed to: " & current,
                            "Region changed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information)
        Else
            MessageBox.Show("Region change might have failed." & Environment.NewLine &
                            "Requested: " & selectedName & Environment.NewLine &
                            "Detected in hosts: " & current,
                            "Region change error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)
        End If

        UpdatePingList()
    End Sub

    Private Sub ButtonReset_Click(sender As Object, e As EventArgs) Handles ButtonReset.Click
        If Not IsUserAdministrator() Then
            MessageBox.Show("Please run this application as Administrator to modify the hosts file.",
                            "Administrator required",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
            RestartAsAdmin()
            Return
        End If

        RemoveAllOverrides()
        LabelStatus.Text = "Current region: " & GetCurrentRegion()
        UpdatePingList()
        MessageBox.Show("All GameLift overrides have been removed from the hosts file.",
                        "Hosts reset",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information)
    End Sub

    ' ---------- hosts editing (selected region commented, others blocked) ----------

    Private Sub ApplyRegion(choice As String)
        RemoveAllOverrides()

        If choice = "0" Then
            ' Default -> no gamelift-ping entries
            Return
        End If

        AppendHostsEntries(choice)
    End Sub

    Private Sub RemoveAllOverrides()
        Try
            Dim lines = File.ReadAllLines(hostsPath).ToList()
            Dim filtered = lines.Where(Function(line) Not IsGameliftLine(line)).ToList()

            While filtered.Count > 0 AndAlso String.IsNullOrWhiteSpace(filtered.Last())
                filtered.RemoveAt(filtered.Count - 1)
            End While
            If filtered.Count > 0 Then
                filtered.Add(String.Empty)
            End If

            File.WriteAllLines(hostsPath, filtered)
        Catch ex As Exception
            MessageBox.Show("Error while removing entries from hosts: " & ex.Message,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function IsGameliftLine(line As String) As Boolean
        Return Regex.IsMatch(line, "gamelift-ping\.[^.]+\.api\.aws", RegexOptions.IgnoreCase)
    End Function

    Private Sub AppendHostsEntries(choice As String)
        Try
            Using w As New StreamWriter(hostsPath, True)
                w.WriteLine()
                For Each kvp In regions
                    If kvp.Key = "0" Then Continue For

                    Dim regionId = kvp.Value.Value
                    Dim domain = $"gamelift-ping.{regionId}.api.aws"

                    If kvp.Key = choice Then
                        w.WriteLine("# 0.0.0.0 " & domain)   ' selected region (not blocked)
                    Else
                        w.WriteLine("0.0.0.0 " & domain)    ' all others blocked
                    End If
                Next
            End Using
        Catch ex As Exception
            MessageBox.Show("Error while writing to hosts: " & ex.Message,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ---------- detect current region from hosts ----------

    Private Function GetCurrentRegion() As String
        Try
            If Not File.Exists(hostsPath) Then Return "Default (no lock)"

            Dim content = File.ReadAllText(hostsPath)

            If Not Regex.IsMatch(content, "gamelift-ping\.[^.]+\.api\.aws", RegexOptions.IgnoreCase) Then
                Return "Default (no lock)"
            End If

            For Each kvp In regions
                If kvp.Key = "0" Then Continue For

                Dim regionId = kvp.Value.Value
                Dim pattern As String =
                    "^\s*#\s*0\.0\.0\.0\s+gamelift-ping\." &
                    Regex.Escape(regionId) &
                    "\.api\.aws\s*$"

                If Regex.IsMatch(content, pattern,
                                 RegexOptions.IgnoreCase Or RegexOptions.Multiline) Then
                    Return kvp.Value.Key
                End If
            Next

            Return "Unknown / all blocked"
        Catch
            Return "Unknown"
        End Try
    End Function

    ' ---------- TCP ping ----------

    Private Async Function PingRegion(regionKey As String) As Threading.Tasks.Task(Of (Ping As Double, Status As String))
        If regionKey = "0" Then
            Return (0, "N/A")
        End If

        Dim regionId = regions(regionKey).Value
        Dim host = $"gamelift.{regionId}.amazonaws.com"
        Dim port As Integer = 443

        Dim ms = Await TcpPingAsync(host, port, 3000)

        Dim status As String
        If ms >= 999 Then
            status = "Unreachable"
        ElseIf ms < 80 Then
            status = "Very good"
        ElseIf ms < 160 Then
            status = "Good"
        Else
            status = "Bad"
        End If

        Return (ms, status)
    End Function

    Private Async Function TcpPingAsync(host As String, port As Integer, timeoutMs As Integer) As Threading.Tasks.Task(Of Double)
        Return Await Threading.Tasks.Task.Run(Function()
                                                  Using client As New TcpClient()
                                                      Dim sw As New Stopwatch()
                                                      Try
                                                          sw.Start()
                                                          Dim ar = client.BeginConnect(host, port, Nothing, Nothing)
                                                          If Not ar.AsyncWaitHandle.WaitOne(timeoutMs) Then
                                                              Return 999.0
                                                          End If
                                                          client.EndConnect(ar)
                                                          sw.Stop()
                                                          Return CDbl(sw.ElapsedMilliseconds)
                                                      Catch
                                                          Return 999.0
                                                      End Try
                                                  End Using
                                              End Function)
    End Function

    ' ---------- Ping list: update values only ----------

    Private Async Sub UpdatePingList()
        For Each kvp In regions
            Dim key = kvp.Key
            Dim res = Await PingRegion(key)

            Dim idx As Integer
            If regionIndex.TryGetValue(key, idx) AndAlso
               idx >= 0 AndAlso idx < ListViewPing.Items.Count Then

                Dim item = ListViewPing.Items(idx)
                item.SubItems(1).Text = res.Ping.ToString("F0") & " ms"
                item.SubItems(2).Text = res.Status
            End If
        Next
    End Sub

    Private Sub TimerPing_Tick(sender As Object, e As EventArgs) Handles TimerPing.Tick
        LabelStatus.Text = "Current region: " & GetCurrentRegion()
        UpdatePingList()
    End Sub

End Class
