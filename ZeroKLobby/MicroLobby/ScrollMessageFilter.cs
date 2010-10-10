﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SpringDownloader;

namespace SpringDownloader.MicroLobby
{
    public class ScrollMessageFilter: IMessageFilter
    {
        const int WM_MOUSEWHEEL = 0x20A;

        Form filterForm;

        public ScrollMessageFilter(Form filterForm)
        {
            this.filterForm = filterForm;
        }


        [DllImport("User32.dll")]
        static extern Int32 SendMessage(int hWnd, int Msg, int wParam, int lParam);

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_MOUSEWHEEL)
            {
                var control = Utils.GetHoveredControl(filterForm);
                if (control is WebBrowser) return false;
								SendMessage((int)control.Handle, m.Msg, (int)m.WParam, (int)m.LParam);
                return true;
            }
            return false;
        }
    }
}