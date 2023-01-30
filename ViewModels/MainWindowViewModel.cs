using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Xaml.Behaviors.Core;
using MonaJsonToMsp.Models;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MonaJsonToMsp.Common;

namespace MonaJsonToMsp.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private MainWindowModel m_Model;
        private object? Cursor;

        public MainWindowViewModel()
        {
            m_Model = new MainWindowModel();
        }
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string? JsonFolder
        {
            get { return m_Model.JsonFolder; }
            set
            {
                if (m_Model.JsonFolder != value)
                {
                    m_Model.JsonFolder = value;
                    NotifyPropertyChanged(nameof(JsonFolder));
                }
            }
        }
        public string? ExportFolder
        {
            get { return m_Model.ExportFolder; }
            set
            {
                if (m_Model.ExportFolder != value)
                {
                    m_Model.ExportFolder = value;
                    NotifyPropertyChanged(nameof(ExportFolder));
                }
            }
        }
        public string? OntologyFile
        {
            get { return m_Model.OntologyFile; }
            set
            {
                if (m_Model.OntologyFile != value)
                {
                    m_Model.OntologyFile = value;
                    NotifyPropertyChanged(nameof(OntologyFile));
                }
            }
        }
        public bool emptyIsPos
        {
            get { return m_Model.emptyIsPos; }
            set
            {
                m_Model.emptyIsPos = value;
                NotifyPropertyChanged(nameof(emptyIsPos));
            }
        }
        public bool posNegSame
        {
            get { return m_Model.posNegSame; }
            set
            {
                m_Model.posNegSame = value;
                NotifyPropertyChanged(nameof(posNegSame));
            }
        }


        private ActionCommand? selectJsonFolder_Click;
        public ICommand SelectJsonFolder_Click => selectJsonFolder_Click ??= new ActionCommand(PerformSelectJsonFolder_Click);

        private void PerformSelectJsonFolder_Click()
        {
            m_Model.JsonFolder = GetFolderName();
        }


        private ActionCommand? selectExportFolder_Click;
        public ICommand SelectExportFolder_Click => selectExportFolder_Click ??= new ActionCommand(PerformSelectExportFolder_Click);

        private void PerformSelectExportFolder_Click()
        {
            m_Model.ExportFolder = GetFolderName();
        }

        private ActionCommand? selectOntologyFile_Click;
        public ICommand SelectOntologyFile_Click => selectOntologyFile_Click ??= new ActionCommand(PerformSelectOntologyFile_Click);

        private void PerformSelectOntologyFile_Click()
        {
            m_Model.OntologyFile = GetFileName();
        }

        private string? GetFolderName()
        {
            var dr = new CommonOpenFileDialog();
            dr.IsFolderPicker = true;
            dr.Multiselect = false;
            if (dr.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dr.FileName;
            }
            else { return null; }
        }

        private string? GetFileName()
        {
            var dr = new CommonOpenFileDialog();
            dr.IsFolderPicker = false;
            dr.Multiselect = false;
            if (dr.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dr.FileName;
            }
            else { return null; }
        }

        private ActionCommand? run_Click;

        public ICommand Run_Click => run_Click ??= new ActionCommand(PerformRun_Click);

        private async Task LockUI(Func<Task> act)
        {
            //var topElm = (UIElement)VisualTreeHelper.GetChild(this, 0);
            //var oldEnabled = topElm.IsEnabled;
            var oldCursor = Cursor;
            try
            {
                Cursor = Cursors.Wait;
                //topElm.IsEnabled = false;
                await act();
            }
            finally
            {
                //topElm.IsEnabled = oldEnabled;
                Cursor = oldCursor;
            }
        }


        private async void PerformRun_Click()
        {
            await LockUI(async () => { await run_Click2(); });
        }

        private async Task run_Click2()
        {
            var JsonFolderTxt = m_Model.JsonFolder;
            if (JsonFolderTxt == "" || JsonFolderTxt == null || !Directory.Exists(JsonFolderTxt))
            {
                MessageBox("Check Json Folder");
                return;
            }
            var ExportFolderTxt = m_Model.ExportFolder;
            if (ExportFolderTxt == "" || ExportFolderTxt == null || !Directory.Exists(ExportFolderTxt))
            {
                MessageBox("Check Export Folder");
                return;
            }

            var OntologyFileTxt = m_Model.OntologyFile?.Split(",");

            var emptyIsPosCheck = m_Model.emptyIsPos;
            var posNegSameCheck = m_Model.posNegSame;

            Common.program.run(JsonFolderTxt, OntologyFileTxt, ExportFolderTxt, emptyIsPosCheck, posNegSameCheck);
            Cursor = Cursors.Wait;
            await Task.CompletedTask;
            MessageBox("Done.");
        }

        private async void MessageBox(string msg)
        {
            var result = await MsgBox.Show(msg);
        }

    }
}

