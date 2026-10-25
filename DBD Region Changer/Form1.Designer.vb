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
        Me.PanelHeader = New System.Windows.Forms.Panel()
        Me.LabelSection = New System.Windows.Forms.Label()
        Me.LabelHint = New System.Windows.Forms.Label()
        Me.PanelCards = New System.Windows.Forms.Panel()
        Me.PanelFooter = New System.Windows.Forms.Panel()
        Me.LabelStatus = New System.Windows.Forms.Label()
        Me.ButtonReset = New DBD_Region_Changer.ModernButton()
        Me.ButtonApply = New DBD_Region_Changer.ModernButton()
        Me.TimerPing = New System.Windows.Forms.Timer(Me.components)
        Me.PanelFooter.SuspendLayout()
        Me.SuspendLayout()
        '
        'PanelHeader
        '
        Me.PanelHeader.Location = New System.Drawing.Point(0, 0)
        Me.PanelHeader.Name = "PanelHeader"
        Me.PanelHeader.Size = New System.Drawing.Size(640, 84)
        Me.PanelHeader.TabIndex = 0
        '
        'LabelSection
        '
        Me.LabelSection.Location = New System.Drawing.Point(20, 98)
        Me.LabelSection.Name = "LabelSection"
        Me.LabelSection.Size = New System.Drawing.Size(200, 18)
        Me.LabelSection.TabIndex = 1
        Me.LabelSection.Text = "SERVERS"
        Me.LabelSection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'LabelHint
        '
        Me.LabelHint.Location = New System.Drawing.Point(300, 98)
        Me.LabelHint.Name = "LabelHint"
        Me.LabelHint.Size = New System.Drawing.Size(320, 18)
        Me.LabelHint.TabIndex = 2
        Me.LabelHint.Text = "Click to select · double-click to lock"
        Me.LabelHint.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'PanelCards
        '
        Me.PanelCards.Location = New System.Drawing.Point(20, 122)
        Me.PanelCards.Name = "PanelCards"
        Me.PanelCards.Size = New System.Drawing.Size(600, 340)
        Me.PanelCards.TabIndex = 3
        '
        'PanelFooter
        '
        Me.PanelFooter.Controls.Add(Me.LabelStatus)
        Me.PanelFooter.Controls.Add(Me.ButtonReset)
        Me.PanelFooter.Controls.Add(Me.ButtonApply)
        Me.PanelFooter.Location = New System.Drawing.Point(0, 480)
        Me.PanelFooter.Name = "PanelFooter"
        Me.PanelFooter.Size = New System.Drawing.Size(640, 80)
        Me.PanelFooter.TabIndex = 4
        '
        'LabelStatus
        '
        Me.LabelStatus.Location = New System.Drawing.Point(20, 10)
        Me.LabelStatus.Name = "LabelStatus"
        Me.LabelStatus.Size = New System.Drawing.Size(300, 60)
        Me.LabelStatus.TabIndex = 0
        Me.LabelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ButtonReset
        '
        Me.ButtonReset.Location = New System.Drawing.Point(330, 21)
        Me.ButtonReset.Name = "ButtonReset"
        Me.ButtonReset.Size = New System.Drawing.Size(110, 38)
        Me.ButtonReset.TabIndex = 1
        Me.ButtonReset.Text = "Remove lock"
        '
        'ButtonApply
        '
        Me.ButtonApply.Location = New System.Drawing.Point(450, 21)
        Me.ButtonApply.Name = "ButtonApply"
        Me.ButtonApply.Primary = True
        Me.ButtonApply.Size = New System.Drawing.Size(170, 38)
        Me.ButtonApply.TabIndex = 2
        Me.ButtonApply.Text = "Lock region"
        '
        'TimerPing
        '
        Me.TimerPing.Interval = 5000
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.ClientSize = New System.Drawing.Size(640, 560)
        Me.Controls.Add(Me.PanelFooter)
        Me.Controls.Add(Me.PanelCards)
        Me.Controls.Add(Me.LabelHint)
        Me.Controls.Add(Me.LabelSection)
        Me.Controls.Add(Me.PanelHeader)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "Form1"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "DBD Region Changer"
        Me.PanelFooter.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents PanelHeader As Panel
    Friend WithEvents LabelSection As Label
    Friend WithEvents LabelHint As Label
    Friend WithEvents PanelCards As Panel
    Friend WithEvents PanelFooter As Panel
    Friend WithEvents LabelStatus As Label
    Friend WithEvents ButtonReset As ModernButton
    Friend WithEvents ButtonApply As ModernButton
    Friend WithEvents TimerPing As Timer
End Class
