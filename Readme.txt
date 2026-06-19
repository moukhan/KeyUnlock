KeyUnlock
=========

KeyUnlock is a lightweight Windows tray utility that keeps a machine
from locking or going idle. It runs quietly in the system tray and,
on a timer, taps the Shift key (press + release) and tells Windows
that the system and display are required to stay on
(SetThreadExecutionState). This resets the OS idle/lock timer without
otherwise interfering with normal use of the keyboard or mouse.

Features
--------
- System tray icon showing current state (green = active, gray = paused)
- Pause / Resume from the tray menu or by double-clicking the icon
- Configurable interval: 30, 60, or 120 seconds
- Exit option that restores normal idle/lock behavior

How it works
-------------
- Program.cs        - application entry point
- TrayApplicationContext.cs - tray icon, menu, and timer logic
- NativeMethods.cs   - P/Invoke declarations for SendInput and
                       SetThreadExecutionState (user32.dll / kernel32.dll)

Requirements
------------
- Windows
- .NET 9 (net9.0-windows, Windows Forms)

Building
--------
    dotnet build

Running
-------
    dotnet run

Or run the built executable directly from the output folder
(bin\Debug\net9.0-windows\KeyUnlock.exe after a build).
