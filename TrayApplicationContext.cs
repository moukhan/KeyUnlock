using System.Drawing;

namespace KeyUnlock;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly ToolStripMenuItem _toggleItem;
    private readonly ToolStripMenuItem _intervalItem30;
    private readonly ToolStripMenuItem _intervalItem60;
    private readonly ToolStripMenuItem _intervalItem120;
    private bool _active = true;

    public TrayApplicationContext()
    {
        _toggleItem = new ToolStripMenuItem("Pause", null, OnToggle);

        _intervalItem30 = new ToolStripMenuItem("30 seconds", null, OnIntervalChanged) { Tag = 30, Checked = true };
        _intervalItem60 = new ToolStripMenuItem("60 seconds", null, OnIntervalChanged) { Tag = 60 };
        _intervalItem120 = new ToolStripMenuItem("120 seconds", null, OnIntervalChanged) { Tag = 120 };

        var intervalMenu = new ToolStripMenuItem("Interval");
        intervalMenu.DropDownItems.AddRange([_intervalItem30, _intervalItem60, _intervalItem120]);

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add(_toggleItem);
        contextMenu.Items.Add(intervalMenu);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Exit", null, OnExit);

        _trayIcon = new NotifyIcon
        {
            Icon = CreateIcon(Color.LimeGreen),
            Text = "KeyUnlock — Active",
            ContextMenuStrip = contextMenu,
            Visible = true,
        };
        _trayIcon.DoubleClick += OnToggle;

        _timer = new System.Windows.Forms.Timer { Interval = 30_000 };
        _timer.Tick += OnTick;
        _timer.Start();

        SetAwakeState();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        SendShiftKey();
        SetAwakeState();
    }

    private static void SendShiftKey()
    {
        var inputs = new NativeMethods.INPUT[2];

        inputs[0].Type = NativeMethods.INPUT_KEYBOARD;
        inputs[0].Union.Keyboard.VirtualKey = NativeMethods.VK_SHIFT;

        inputs[1].Type = NativeMethods.INPUT_KEYBOARD;
        inputs[1].Union.Keyboard.VirtualKey = NativeMethods.VK_SHIFT;
        inputs[1].Union.Keyboard.Flags = NativeMethods.KEYEVENTF_KEYUP;

        NativeMethods.SendInput((uint)inputs.Length, inputs, System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.INPUT>());
    }

    private static void SetAwakeState()
    {
        NativeMethods.SetThreadExecutionState(
            NativeMethods.EXECUTION_STATE.ES_CONTINUOUS |
            NativeMethods.EXECUTION_STATE.ES_SYSTEM_REQUIRED |
            NativeMethods.EXECUTION_STATE.ES_DISPLAY_REQUIRED);
    }

    private static void ClearAwakeState()
    {
        NativeMethods.SetThreadExecutionState(NativeMethods.EXECUTION_STATE.ES_CONTINUOUS);
    }

    private void OnToggle(object? sender, EventArgs e)
    {
        _active = !_active;

        if (_active)
        {
            _timer.Start();
            SetAwakeState();
            _toggleItem.Text = "Pause";
            _trayIcon.Icon = CreateIcon(Color.LimeGreen);
            _trayIcon.Text = "KeyUnlock — Active";
        }
        else
        {
            _timer.Stop();
            ClearAwakeState();
            _toggleItem.Text = "Resume";
            _trayIcon.Icon = CreateIcon(Color.Gray);
            _trayIcon.Text = "KeyUnlock — Paused";
        }
    }

    private void OnIntervalChanged(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;

        _intervalItem30.Checked = false;
        _intervalItem60.Checked = false;
        _intervalItem120.Checked = false;
        item.Checked = true;

        _timer.Interval = (int)item.Tag! * 1000;
    }

    private void OnExit(object? sender, EventArgs e)
    {
        ClearAwakeState();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        _timer.Dispose();
        Application.Exit();
    }

    private static Icon CreateIcon(Color color)
    {
        using var bmp = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var brush = new SolidBrush(color);
        g.FillEllipse(brush, 1, 1, 14, 14);
        return Icon.FromHandle(bmp.GetHicon());
    }
}
