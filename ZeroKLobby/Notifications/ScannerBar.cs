﻿using System;
using System.Windows.Forms;
using PlasmaShared;

namespace SpringDownloader.Notifications
{
	public partial class ScannerBar: UserControl, INotifyBar
	{
		NotifyBarContainer container;

		public ScannerBar(SpringScanner scanner)
		{
			InitializeComponent();
			scanner.WorkStarted += scanner_WorkProgressChanged;
			scanner.WorkStopped += scanner_WorkStopped;
			scanner.WorkProgressChanged += scanner_WorkProgressChanged;
		}

		public void AddedToContainer(NotifyBarContainer container)
		{
			container.btnStop.Enabled = false;
			container.btnDetail.Enabled = false;
			container.btnDetail.Text = "Scan";
			this.container = container;
		}

		public void CloseClicked(NotifyBarContainer container) {}

		public void DetailClicked(NotifyBarContainer container) {}

		public Control GetControl()
		{
			return this;
		}

		void scanner_WorkProgressChanged(object sender, ProgressEventArgs e)
		{
			if (e.WorkTotal > 1)
			{
				Program.FormMain.Invoke(new Action(() =>
					{
						Program.NotifySection.AddBar(this);
						progressBar1.Maximum = e.WorkTotal;
						progressBar1.Value = e.WorkDone;
						label1.Text = "Scanning existing maps and games";
						label2.Text = string.Format("{0}/{1} - {2}", e.WorkDone, e.WorkTotal, e.WorkName);
					}));
			}
		}


		void scanner_WorkStopped(object sender, EventArgs e)
		{
			Program.FormMain.Invoke(new Action(() => Program.NotifySection.RemoveBar(this)));
		}
	}
}