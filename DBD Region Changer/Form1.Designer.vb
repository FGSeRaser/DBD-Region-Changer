<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.ComboBoxRegion = New System.Windows.Forms.ComboBox()
        Me.ButtonAnwenden = New System.Windows.Forms.Button()
        Me.LabelStatus = New System.Windows.Forms.Label()
        Me.ListViewPing = New System.Windows.Forms.ListView()
        Me.Region = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.Ping = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.Status = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.TimerPing = New System.Windows.Forms.Timer(Me.components)
        Me.ButtonReset = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'ComboBoxRegion
        '
        Me.ComboBoxRegion.FormattingEnabled = True
        Me.ComboBoxRegion.Location = New System.Drawing.Point(20, 20)
        Me.ComboBoxRegion.Margin = New System.Windows.Forms.Padding(2, 3, 2, 3)
        Me.ComboBoxRegion.Name = "ComboBoxRegion"
        Me.ComboBoxRegion.Size = New System.Drawing.Size(250, 24)
        Me.ComboBoxRegion.TabIndex = 0
        '
        'ButtonAnwenden
        '
        Me.ButtonAnwenden.Location = New System.Drawing.Point(280, 3)
        Me.ButtonAnwenden.Margin = New System.Windows.Forms.Padding(2, 3, 2, 3)
        Me.ButtonAnwenden.Name = "ButtonAnwenden"
        Me.ButtonAnwenden.Size = New System.Drawing.Size(100, 25)
        Me.ButtonAnwenden.TabIndex = 1
        Me.ButtonAnwenden.Text = "Change Region"
        Me.ButtonAnwenden.UseVisualStyleBackColor = True
        '
        'LabelStatus
        '
        Me.LabelStatus.AutoSize = True
        Me.LabelStatus.Location = New System.Drawing.Point(20, 55)
        Me.LabelStatus.Name = "LabelStatus"
        Me.LabelStatus.Size = New System.Drawing.Size(34, 16)
        Me.LabelStatus.TabIndex = 2
        Me.LabelStatus.Text = "Status"
        '
        'ListViewPing
        '
        Me.ListViewPing.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.Region, Me.Ping, Me.Status})
        Me.ListViewPing.FullRowSelect = True
        Me.ListViewPing.GridLines = True
        Me.ListViewPing.HideSelection = False
        Me.ListViewPing.Location = New System.Drawing.Point(20, 85)
        Me.ListViewPing.Name = "ListViewPing"
        Me.ListViewPing.Size = New System.Drawing.Size(360, 300)
        Me.ListViewPing.TabIndex = 3
        Me.ListViewPing.UseCompatibleStateImageBehavior = False
        Me.ListViewPing.View = System.Windows.Forms.View.Details
        '
        'Region
        '
        Me.Region.Text = "Region"
        Me.Region.Width = 125
        '
        'Ping
        '
        Me.Ping.Text = "Ping"
        Me.Ping.Width = 98
        '
        'Status
        '
        Me.Status.Text = "Status"
        Me.Status.Width = 91
        '
        'TimerPing
        '
        Me.TimerPing.Interval = 5000
        '
        'ButtonReset
        '
        Me.ButtonReset.Location = New System.Drawing.Point(280, 34)
        Me.ButtonReset.Margin = New System.Windows.Forms.Padding(2, 3, 2, 3)
        Me.ButtonReset.Name = "ButtonReset"
        Me.ButtonReset.Size = New System.Drawing.Size(100, 25)
        Me.ButtonReset.TabIndex = 4
        Me.ButtonReset.Text = "Reset Host"
        Me.ButtonReset.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(5.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(399, 395)
        Me.Controls.Add(Me.ButtonReset)
        Me.Controls.Add(Me.ListViewPing)
        Me.Controls.Add(Me.LabelStatus)
        Me.Controls.Add(Me.ButtonAnwenden)
        Me.Controls.Add(Me.ComboBoxRegion)
        Me.Font = New System.Drawing.Font("Bahnschrift Condensed", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Form1"
        Me.Text = "DBD Region Changer"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents ComboBoxRegion As ComboBox
    Friend WithEvents ButtonAnwenden As Button
    Friend WithEvents LabelStatus As Label
    Friend WithEvents ListViewPing As ListView
    Friend WithEvents TimerPing As Timer
    Friend WithEvents Region As ColumnHeader
    Friend WithEvents Ping As ColumnHeader
    Friend WithEvents Status As ColumnHeader
    Friend WithEvents ButtonReset As Button
End Class
