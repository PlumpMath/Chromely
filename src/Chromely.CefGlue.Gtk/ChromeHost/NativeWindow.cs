﻿using System;

namespace Chromely.CefGlue.Gtk.ChromeHost
{
    public class NativeWindow
    {
        protected int m_width;
        protected int m_height;

        private IntPtr m_cefHost;
        private IntPtr m_mainWindow;
        private string m_title;
        private string m_iconFile;

        public NativeWindow()
        {
            m_title = "chromely";
            m_width = 1200;
            m_height = 800;
            m_iconFile = null;
        }

        public NativeWindow(string title, int width, int height, string iconFile = null)
        {
            m_title = title;
            m_width = width;
            m_height = height;
            m_iconFile = iconFile;
        }

        public IntPtr Handle
        {
            get
            {
                return m_mainWindow;
            }
        }

        public IntPtr Host
        {
            get
            {
                return m_cefHost;
            }
        }

        public IntPtr HostXid
        {
            get
            {
                return NativeMethods.GetWindowXid(m_cefHost);
            }
        }

        private void CreateWindow()
        {
            m_mainWindow = NativeMethods.NewWindow(NativeMethods.GtkWindowType.GTK_WINDOW_TOPLEVEL);
            NativeMethods.SetTitle(m_mainWindow, m_title);

            NativeMethods.SetIconFromFile(m_mainWindow, m_iconFile);

            NativeMethods.SetWindowSize(m_mainWindow, m_width, m_height);
            NativeMethods.SetWindowResizable(m_mainWindow, true);
            NativeMethods.SetWindowPosition(m_mainWindow, NativeMethods.GtkWindowPosition.GTK_WIN_POS_CENTER);

            m_cefHost = NativeMethods.CreateCefHost(m_mainWindow);

            NativeMethods.AddConfigureEvent(m_cefHost);

            Realized += OnRealized;
            Resized += OnResize;
            Exited += OnExit;

            NativeMethods.ShowAll(m_mainWindow);
        }

        public virtual void ShowWindow()
        {
            CreateWindow();
        }

        public virtual void ResizeHost(IntPtr host, int width, int height)
        {
            NativeMethods.SetWindowSize(host, width, height);
        }

        protected virtual void GetSize(out int width, out int height)
        {
            width = 0;
            height = 0;

            if (m_mainWindow == IntPtr.Zero)
            {
                return;
            }

            NativeMethods.GetWindowSize(m_mainWindow, out width, out height);
        }

        protected virtual void OnRealized(object sender, EventArgs e)
        {
        }

        protected virtual void OnResize(object sender, EventArgs e)
        {
            int width;
            int height;
            GetSize(out width, out height);

            NativeMethods.SetWindowSize(Host, width, height);
        }

        protected virtual void OnExit(object sender, EventArgs e)
        {
            Application.Quit();
        }

        private EventHandler<EventArgs> m_realizeEvent;
        private EventHandler<EventArgs> m_configureEvent;
        private EventHandler<EventArgs> m_destroyEvent;

        private object m_eventLock = new Object();

        private event EventHandler<EventArgs> Realized
        {
            add
            {
                lock (m_eventLock)
                {
                    m_realizeEvent += value;
                    NativeMethods.EventHandler onRealizedHandler = new NativeMethods.EventHandler(LocalRealized);
                    NativeMethods.ConnectSignal(m_cefHost, "realize", onRealizedHandler, 0, IntPtr.Zero, (int)NativeMethods.GConnectFlags.G_CONNECT_AFTER);
                }
            }
            remove
            {
                lock (m_eventLock)
                {
                    m_realizeEvent -= value;
                }
            }
        }

        private event EventHandler<EventArgs> Resized
        {
            add
            {
                lock (m_eventLock)
                {
                    m_configureEvent += value;
                    NativeMethods.EventHandler onConfiguredHandler = new NativeMethods.EventHandler(LocalConfigured);
                    NativeMethods.ConnectSignal(m_mainWindow, "configure-event", onConfiguredHandler, 0, IntPtr.Zero, (int)NativeMethods.GConnectFlags.G_CONNECT_AFTER);
                }
            }
            remove
            {
                lock (m_eventLock)
                {
                    m_configureEvent -= value;
                }
            }
        }

        private event EventHandler<EventArgs> Exited
        {
            add
            {
                lock (m_eventLock)
                {
                    m_destroyEvent += value;
                    NativeMethods.EventHandler onDestroyedHandler = new NativeMethods.EventHandler(LocalDestroyed);
                    NativeMethods.ConnectSignal(m_mainWindow, "destroy", onDestroyedHandler, 0, IntPtr.Zero, (int)NativeMethods.GConnectFlags.G_CONNECT_AFTER);
                }
            }
            remove
            {
                lock (m_eventLock)
                {
                    m_destroyEvent -= value;
                }
            }
        }

        private void LocalRealized(NativeMethods.StructEventArgs eventArgs)
        {
            m_realizeEvent(this, new EventArgs());
        }

        private void LocalConfigured(NativeMethods.StructEventArgs eventArgs)
        {
            m_configureEvent(this, new EventArgs());
        }

        private void LocalDestroyed(NativeMethods.StructEventArgs eventArgs)
        {
            m_destroyEvent(this, new EventArgs());
        }
    }
}
