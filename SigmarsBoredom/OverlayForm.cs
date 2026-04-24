using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;

namespace SigmarsBoredom
{
    /// <summary>
    /// A small always-on-top overlay window that lets the user start and stop the solver.
    /// </summary>
    public class OverlayForm : Form
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private Button _startButton;
        private Button _stopButton;
        private Label _statusLabel;

        private CancellationTokenSource _cts;
        private Task _solverTask;

        public OverlayForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form properties
            Text = "Sigmar's Boredom";
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(10, 10);
            ClientSize = new Size(200, 90);
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;
            ShowInTaskbar = true;

            // Status label
            _statusLabel = new Label
            {
                Text = "Idle",
                Dock = DockStyle.Top,
                Height = 24,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Silver,
                Font = new Font("Segoe UI", 9f)
            };

            // Start button
            _startButton = new Button
            {
                Text = "▶  Start",
                Dock = DockStyle.Left,
                Width = 96,
                BackColor = Color.FromArgb(0, 120, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Top = 30
            };
            _startButton.FlatAppearance.BorderSize = 0;
            _startButton.Click += StartButton_Click;

            // Stop button
            _stopButton = new Button
            {
                Text = "■  Stop",
                Dock = DockStyle.Right,
                Width = 96,
                BackColor = Color.FromArgb(160, 30, 30),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Enabled = false
            };
            _stopButton.FlatAppearance.BorderSize = 0;
            _stopButton.Click += StopButton_Click;

            // Button panel
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(4, 4, 4, 4)
            };
            buttonPanel.Controls.Add(_stopButton);
            buttonPanel.Controls.Add(_startButton);

            Controls.Add(buttonPanel);
            Controls.Add(_statusLabel);

            FormClosing += OverlayForm_FormClosing;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (_solverTask != null && !_solverTask.IsCompleted)
                return;

            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _startButton.Enabled = false;
            _stopButton.Enabled = true;
            SetStatus("Running...", Color.LimeGreen);

            _solverTask = Task.Run(() =>
            {
                try
                {
                    var solver = new Solver();
                    solver.Run(token);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    _logger.Error(ex, "Solver encountered an error.");
                    Invoke(new Action(() => SetStatus("Error — see log", Color.OrangeRed)));
                }
            }, token).ContinueWith(t =>
            {
                if (IsHandleCreated)
                    Invoke(new Action(OnSolverStopped));
            });
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
            _stopButton.Enabled = false;
            SetStatus("Stopping...", Color.Orange);
        }

        private void OnSolverStopped()
        {
            _cts?.Dispose();
            _cts = null;
            _startButton.Enabled = true;
            _stopButton.Enabled = false;
            SetStatus("Idle", Color.Silver);
        }

        private void SetStatus(string text, Color color)
        {
            _statusLabel.Text = text;
            _statusLabel.ForeColor = color;
        }

        private void OverlayForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts?.Cancel();
        }
    }
}
